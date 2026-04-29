using Arboon.Application.DTOs.Escrow;
using FluentValidation;

namespace Arboon.Application.Validators;

public class PayEscrowValidator : AbstractValidator<PayEscrowDto>
{
    public PayEscrowValidator()
    {
        RuleFor(x => x.BuyerEmail)
            .NotEmpty().WithMessage("بريد المشتري مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");
    }
}
