using AutoMapper;
using Arboon.Application.DTOs.Escrow;
using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Arboon.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arboon.Application.Services;

public class EscrowService : IEscrowService
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<EscrowService> _logger;
    private readonly string _baseUrl;

    public EscrowService(
        IAppDbContext context,
        IMapper mapper,
        IEmailService emailService,
        IPaymentService paymentService,
        IWebhookService webhookService,
        ILogger<EscrowService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _paymentService = paymentService;
        _webhookService = webhookService;
        _logger = logger;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://arboon.app";
    }

    public async Task<EscrowResponseDto> CreateAsync(Guid sellerId, CreateEscrowDto dto)
    {
        var escrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = sellerId,
            Title = dto.Title,
            Amount = dto.Amount,
            Currency = dto.Currency.ToUpperInvariant(),
            Conditions = dto.Conditions,
        };

        escrow.CalculateAndSetFee();
        escrow.PaymentUrl = $"{_baseUrl}/pay/{escrow.Id}";

        _context.Escrows.Add(escrow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Escrow {EscrowId} created by seller {SellerId}", escrow.Id, sellerId);

        await DispatchEscrowWebhook("escrow.created", escrow);

        // Load seller for mapping
        var result = await _context.Escrows
            .Include(e => e.Seller)
            .FirstAsync(e => e.Id == escrow.Id);

        return _mapper.Map<EscrowResponseDto>(result);
    }

    public async Task<EscrowResponseDto> GetByIdAsync(string escrowId)
    {
        var escrow = await _context.Escrows
            .Include(e => e.Seller)
            .FirstOrDefaultAsync(e => e.Id == escrowId)
            ?? throw new EscrowNotFoundException(escrowId);

        return _mapper.Map<EscrowResponseDto>(escrow);
    }

    public async Task<List<EscrowResponseDto>> GetBySellerIdAsync(Guid sellerId)
    {
        var escrows = await _context.Escrows
            .Include(e => e.Seller)
            .Where(e => e.SellerId == sellerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<EscrowResponseDto>>(escrows);
    }

    public async Task<object> PayAsync(string escrowId, PayEscrowDto dto)
    {
        // Use a transaction for atomicity
        var escrow = await _context.Escrows
            .Include(e => e.Seller)
            .FirstOrDefaultAsync(e => e.Id == escrowId)
            ?? throw new EscrowNotFoundException(escrowId);

        // 1. Validate escrow is PENDING (domain method handles this)
        // 2. Process payment
        var paymentSuccess = await _paymentService.ProcessPaymentAsync(
            dto.CardToken, escrow.TotalToPay, escrow.Currency);

        if (!paymentSuccess)
            throw new PaymentFailedException("فشلت عملية معالجة الدفع");

        // 3. Transition state
        escrow.Pay(dto.BuyerEmail);

        // 4. Generate buyer token
        var buyerToken = new BuyerToken
        {
            EscrowId = escrowId,
            Token = Guid.NewGuid()
        };
        _context.BuyerTokens.Add(buyerToken);

        // 5. Freeze funds in seller's wallet
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == escrow.SellerId);

        if (wallet != null)
        {
            wallet.FreezeFunds(escrow.Amount);
        }

        await _context.SaveChangesAsync();

        // 6. Send magic link email (fire-and-forget style, but await for reliability)
        try
        {
            await _emailService.SendMagicLinkAsync(
                dto.BuyerEmail, escrowId, buyerToken.Token, escrow.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send magic link email for escrow {EscrowId}", escrowId);
            // Don't fail the payment if email fails
        }

        _logger.LogInformation("Escrow {EscrowId} paid by {BuyerEmail}", escrowId, dto.BuyerEmail);

        await DispatchEscrowWebhook("escrow.funded", escrow);

        return new
        {
            status = "FROZEN",
            magic_link_sent = true,
            buyer_email = dto.BuyerEmail,
            buyer_token = buyerToken.Token // returned so controller can set cookie
        };
    }

    public async Task<object> ReleaseAsync(string escrowId, Guid? tokenFromBody, Guid? tokenFromCookie)
    {
        var token = tokenFromBody ?? tokenFromCookie
            ?? throw new InvalidBuyerTokenException();

        var escrow = await _context.Escrows
            .FirstOrDefaultAsync(e => e.Id == escrowId)
            ?? throw new EscrowNotFoundException(escrowId);

        // Validate token
        var buyerToken = await _context.BuyerTokens
            .FirstOrDefaultAsync(bt => bt.Token == token && bt.EscrowId == escrowId)
            ?? throw new InvalidBuyerTokenException();

        if (buyerToken.IsUsed)
            throw new TokenAlreadyUsedException();

        // Transition state (domain validates FROZEN → RELEASED)
        escrow.Release();

        // Mark token as used
        buyerToken.IsUsed = true;

        // Release funds in wallet
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == escrow.SellerId);

        if (wallet != null)
        {
            wallet.ReleaseFunds(escrow.Amount);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Escrow {EscrowId} released", escrowId);

        await DispatchEscrowWebhook("escrow.released", escrow);

        return new
        {
            status = "RELEASED",
            released_at = escrow.ReleasedAt
        };
    }

    public async Task<BuyerStatusResponseDto> GetBuyerStatusAsync(string escrowId, Guid? token)
    {
        var escrow = await _context.Escrows
            .FirstOrDefaultAsync(e => e.Id == escrowId)
            ?? throw new EscrowNotFoundException(escrowId);

        var isAuthorized = false;

        if (token.HasValue)
        {
            isAuthorized = await _context.BuyerTokens
                .AnyAsync(bt => bt.Token == token.Value
                    && bt.EscrowId == escrowId
                    && !bt.IsUsed);
        }

        return new BuyerStatusResponseDto
        {
            EscrowStatus = escrow.Status.ToString(),
            IsAuthorizedBuyer = isAuthorized,
            Title = escrow.Title
        };
    }

    public async Task CancelAsync(string escrowId, Guid sellerId)
    {
        var escrow = await _context.Escrows
            .FirstOrDefaultAsync(e => e.Id == escrowId)
            ?? throw new EscrowNotFoundException(escrowId);

        if (escrow.SellerId != sellerId)
            throw new UnauthorizedException("لا يمكنك إلغاء عُهدة لا تملكها");

        // Domain method validates PENDING → CANCELLED
        escrow.Cancel();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Escrow {EscrowId} cancelled by seller {SellerId}", escrowId, sellerId);

        await DispatchEscrowWebhook("escrow.cancelled", escrow);
    }

    private async Task DispatchEscrowWebhook(string eventName, Escrow escrow)
    {
        var payload = new
        {
            escrow_id = escrow.Id,
            title = escrow.Title,
            amount = escrow.Amount,
            currency = escrow.Currency,
            status = escrow.Status.ToString(),
            buyer_email = escrow.BuyerEmail
        };

        await _webhookService.DispatchAsync(eventName, escrow.Id, payload);
    }
}
