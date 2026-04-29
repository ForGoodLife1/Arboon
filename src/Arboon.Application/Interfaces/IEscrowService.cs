using Arboon.Application.DTOs.Escrow;

namespace Arboon.Application.Interfaces;

public interface IEscrowService
{
    Task<EscrowResponseDto> CreateAsync(Guid sellerId, CreateEscrowDto dto);
    Task<EscrowResponseDto> GetByIdAsync(string escrowId);
    Task<List<EscrowResponseDto>> GetBySellerIdAsync(Guid sellerId);
    Task<object> PayAsync(string escrowId, PayEscrowDto dto);
    Task<object> ReleaseAsync(string escrowId, Guid? tokenFromBody, Guid? tokenFromCookie);
    Task<BuyerStatusResponseDto> GetBuyerStatusAsync(string escrowId, Guid? token);
    Task CancelAsync(string escrowId, Guid sellerId);
}
