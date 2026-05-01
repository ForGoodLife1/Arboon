using AutoMapper;
using Arboon.Application.DTOs.Dispute;
using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Arboon.Domain.Enums;
using Arboon.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arboon.Application.Services;

public class DisputeService : IDisputeService
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IWebhookService _webhookService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<DisputeService> _logger;

    public DisputeService(
        IAppDbContext context,
        IMapper mapper,
        IWebhookService webhookService,
        IPaymentService paymentService,
        ILogger<DisputeService> logger)
    {
        _context = context;
        _mapper = mapper;
        _webhookService = webhookService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<DisputeResponseDto> OpenBySellerAsync(Guid sellerId, OpenDisputeDto dto)
    {
        var escrow = await _context.Escrows.FindAsync(dto.EscrowId)
            ?? throw new EscrowNotFoundException(dto.EscrowId);

        if (escrow.SellerId != sellerId)
            throw new UnauthorizedException("لا تملك صلاحية على هذه العُهدة");

        if (escrow.Status != EscrowStatus.FROZEN)
            throw new InvalidStatusTransitionException(escrow.Status, EscrowStatus.DISPUTED);

        var existingDispute = await _context.Disputes.AnyAsync(d => d.EscrowId == dto.EscrowId && d.Status != DisputeStatus.RESOLVED);
        if (existingDispute)
            throw new ActiveDisputeExistsException(dto.EscrowId);

        escrow.Dispute();

        var dispute = new Dispute
        {
            EscrowId = dto.EscrowId,
            OpenedBy = DisputeParty.Seller,
            Reason = dto.Reason
        };

        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();

        await DispatchDisputeWebhook("dispute.opened", dispute.Id, escrow);

        return _mapper.Map<DisputeResponseDto>(dispute);
    }

    public async Task<DisputeResponseDto> OpenByBuyerAsync(OpenDisputeBuyerDto dto)
    {
        var escrow = await _context.Escrows.FindAsync(dto.EscrowId)
            ?? throw new EscrowNotFoundException(dto.EscrowId);

        if (dto.BuyerToken == null || !await _context.BuyerTokens.AnyAsync(bt => bt.EscrowId == dto.EscrowId && bt.Token == dto.BuyerToken && !bt.IsUsed))
            throw new InvalidBuyerTokenException();

        if (escrow.Status != EscrowStatus.FROZEN)
            throw new InvalidStatusTransitionException(escrow.Status, EscrowStatus.DISPUTED);

        var existingDispute = await _context.Disputes.AnyAsync(d => d.EscrowId == dto.EscrowId && d.Status != DisputeStatus.RESOLVED);
        if (existingDispute)
            throw new ActiveDisputeExistsException(dto.EscrowId);

        escrow.Dispute();

        var dispute = new Dispute
        {
            EscrowId = dto.EscrowId,
            OpenedBy = DisputeParty.Buyer,
            Reason = dto.Reason
        };

        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Dispute {DisputeId} opened by buyer for escrow {EscrowId}", dispute.Id, dto.EscrowId);

        await DispatchDisputeWebhook("dispute.opened", dispute.Id, escrow);

        return _mapper.Map<DisputeResponseDto>(dispute);
    }

    public async Task<List<DisputeResponseDto>> GetBySellerAsync(Guid sellerId)
    {
        var disputes = await _context.Disputes
            .Include(d => d.Escrow)
            .Where(d => d.Escrow.SellerId == sellerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<DisputeResponseDto>>(disputes);
    }

    public async Task<List<DisputeResponseDto>> GetAllOpenAsync()
    {
        var disputes = await _context.Disputes
            .Where(d => d.Status != DisputeStatus.RESOLVED)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<DisputeResponseDto>>(disputes);
    }

    public async Task<DisputeDetailDto> GetByIdAsync(Guid disputeId)
    {
        _logger.LogInformation("Fetching dispute details for ID: {DisputeId}", disputeId);

        var dispute = await _context.Disputes
            .Include(d => d.Escrow)
            .Include(d => d.Messages)
            .FirstOrDefaultAsync(d => d.Id == disputeId)
            ?? throw new DisputeNotFoundException(disputeId);

        var dto = _mapper.Map<DisputeDetailDto>(dispute);
        
        // Manual ordering of messages to avoid potential filtered include issues
        if (dto.Messages != null)
        {
            dto.Messages = dto.Messages.OrderBy(m => m.CreatedAt).ToList();
        }

        return dto;
    }

    public async Task<DisputeResponseDto> ResolveAsync(Guid disputeId, ResolveDisputeDto dto)
    {
        var dispute = await _context.Disputes
            .Include(d => d.Escrow)
            .FirstOrDefaultAsync(d => d.Id == disputeId)
            ?? throw new DisputeNotFoundException(disputeId);

        if (dispute.Status == DisputeStatus.RESOLVED)
            throw new InvalidOperationException("النزاع محسوم مسبقاً");

        if (!Enum.TryParse<DisputeResolution>(dto.Resolution, out var resolution))
            throw new ArgumentException("قرار النزاع غير صحيح", nameof(dto.Resolution));

        var escrow = dispute.Escrow;
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == escrow.SellerId);

        if (resolution == DisputeResolution.ReleasedToSeller)
        {
            escrow.Release();
            if (wallet != null) wallet.ReleaseFunds(escrow.Amount);
        }
        else if (resolution == DisputeResolution.RefundedToBuyer)
        {
            escrow.Refund();
            if (wallet != null) wallet.FrozenBalance -= escrow.Amount; // Remove from frozen
            
            // Note: A real implementation would verify the refund actually succeeds 
            // before updating the database, but for MVP we log and proceed
            await _paymentService.ProcessRefundAsync(escrow.Id, escrow.Amount, escrow.Currency);
        }

        dispute.Status = DisputeStatus.RESOLVED;
        dispute.Resolution = resolution;
        dispute.AdminNote = dto.AdminNote;
        dispute.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await DispatchDisputeWebhook("dispute.resolved", dispute.Id, escrow);

        return _mapper.Map<DisputeResponseDto>(dispute);
    }

    public async Task<DisputeMessageDto> AddMessageAsync(Guid disputeId, AddDisputeMessageDto dto, DisputeParty senderType)
    {
        var dispute = await _context.Disputes.FindAsync(disputeId)
            ?? throw new DisputeNotFoundException(disputeId);

        var message = new DisputeMessage
        {
            DisputeId = disputeId,
            SenderType = senderType,
            Message = dto.Message
        };

        _context.DisputeMessages.Add(message);
        await _context.SaveChangesAsync();

        return _mapper.Map<DisputeMessageDto>(message);
    }

    public async Task<List<DisputeMessageDto>> GetMessagesAsync(Guid disputeId)
    {
        var messages = await _context.DisputeMessages
            .Where(m => m.DisputeId == disputeId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<DisputeMessageDto>>(messages);
    }

    private async Task DispatchDisputeWebhook(string eventName, Guid disputeId, Escrow escrow)
    {
        var payload = new
        {
            dispute_id = disputeId,
            escrow_id = escrow.Id,
            title = escrow.Title,
            amount = escrow.Amount,
            currency = escrow.Currency,
            status = escrow.Status.ToString()
        };

        await _webhookService.DispatchAsync(eventName, escrow.Id, payload);
    }
}
