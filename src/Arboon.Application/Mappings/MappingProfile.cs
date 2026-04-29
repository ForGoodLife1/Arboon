using AutoMapper;
using Arboon.Domain.Entities;
using Arboon.Application.DTOs.Escrow;
using Arboon.Application.DTOs.Wallet;
using Arboon.Application.DTOs.Webhook;
using Arboon.Application.DTOs.Dispute;

namespace Arboon.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Escrow → EscrowResponseDto
        CreateMap<Escrow, EscrowResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Name : null));

        // Wallet → WalletBalanceDto
        CreateMap<Wallet, WalletBalanceDto>();

        // Webhook → DTOs
        CreateMap<WebhookEndpoint, WebhookEndpointResponseDto>();
        CreateMap<WebhookDelivery, WebhookDeliveryResponseDto>();
        CreateMap<WebhookDelivery, WebhookDeliveryDetailDto>();

        // Dispute → DTOs
        CreateMap<Dispute, DisputeResponseDto>()
            .ForMember(dest => dest.OpenedBy, opt => opt.MapFrom(src => src.OpenedBy.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Resolution, opt => opt.MapFrom(src => src.Resolution.HasValue ? src.Resolution.Value.ToString() : null));
        CreateMap<Dispute, DisputeDetailDto>()
            .IncludeBase<Dispute, DisputeResponseDto>();
        CreateMap<DisputeMessage, DisputeMessageDto>()
            .ForMember(dest => dest.SenderType, opt => opt.MapFrom(src => src.SenderType.ToString()));
    }
}
