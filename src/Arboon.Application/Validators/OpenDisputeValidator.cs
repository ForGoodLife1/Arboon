using Arboon.Application.DTOs.Dispute;
using FluentValidation;

namespace Arboon.Application.Validators;

public class OpenDisputeValidator : AbstractValidator<OpenDisputeDto>
{
    public OpenDisputeValidator()
    {
        RuleFor(x => x.EscrowId)
            .NotEmpty().WithMessage("رقم العُهدة مطلوب");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("سبب النزاع مطلوب")
            .MinimumLength(10).WithMessage("يجب أن يكون سبب النزاع 10 أحرف على الأقل");
    }
}

public class OpenDisputeBuyerValidator : AbstractValidator<OpenDisputeBuyerDto>
{
    public OpenDisputeBuyerValidator()
    {
        Include(new OpenDisputeValidator());
    }
}
