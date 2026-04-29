using Arboon.Application.DTOs.Dispute;
using Arboon.Domain.Enums;
using FluentValidation;

namespace Arboon.Application.Validators;

public class ResolveDisputeValidator : AbstractValidator<ResolveDisputeDto>
{
    public ResolveDisputeValidator()
    {
        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("القرار مطلوب")
            .IsEnumName(typeof(DisputeResolution), caseSensitive: false).WithMessage("القرار يجب أن يكون ReleasedToSeller أو RefundedToBuyer");
    }
}
