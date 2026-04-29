using AutoMapper;
using Arboon.Domain.Entities;
using Arboon.Application.DTOs.Escrow;
using Arboon.Application.DTOs.Wallet;

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
    }
}
