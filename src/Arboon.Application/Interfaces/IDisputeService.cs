using Arboon.Application.DTOs.Dispute;
using Arboon.Domain.Enums;

namespace Arboon.Application.Interfaces;

public interface IDisputeService
{
    Task<DisputeResponseDto> OpenBySellerAsync(Guid sellerId, OpenDisputeDto dto);
    Task<DisputeResponseDto> OpenByBuyerAsync(OpenDisputeBuyerDto dto);
    Task<List<DisputeResponseDto>> GetBySellerAsync(Guid sellerId);
    Task<List<DisputeResponseDto>> GetAllOpenAsync();
    Task<DisputeDetailDto> GetByIdAsync(Guid disputeId);
    Task<DisputeResponseDto> ResolveAsync(Guid disputeId, ResolveDisputeDto dto);
    Task<DisputeMessageDto> AddMessageAsync(Guid disputeId, AddDisputeMessageDto dto, DisputeParty senderType);
    Task<List<DisputeMessageDto>> GetMessagesAsync(Guid disputeId);
}
