using Arboon.Application.DTOs.Escrow;
using FluentValidation;

namespace Arboon.Application.Validators;

public class CreateEscrowValidator : AbstractValidator<CreateEscrowDto>
{
    private static readonly string[] AllowedCurrencies = { "SAR", "EGP", "USD", "EUR", "AED" };

    public CreateEscrowValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان العُهدة مطلوب")
            .MaximumLength(200).WithMessage("عنوان العُهدة يجب ألا يتجاوز 200 حرف");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("المبلغ يجب أن يكون أكبر من صفر");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة")
            .Must(c => AllowedCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage("العملة يجب أن تكون واحدة من: SAR, EGP, USD, EUR, AED");
    }
}
