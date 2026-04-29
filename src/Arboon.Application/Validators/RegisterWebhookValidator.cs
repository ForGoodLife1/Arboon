using Arboon.Application.DTOs.Webhook;
using FluentValidation;

namespace Arboon.Application.Validators;

public class RegisterWebhookValidator : AbstractValidator<RegisterWebhookDto>
{
    private static readonly string[] ValidEvents = { "escrow.created", "escrow.funded", "escrow.released", "escrow.cancelled", "dispute.opened", "dispute.resolved" };

    public RegisterWebhookValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("الرابط مطلوب")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("الرابط غير صحيح");

        RuleFor(x => x.Events)
            .NotEmpty().WithMessage("يجب تحديد حدث واحد على الأقل")
            .Must(events => events.All(e => ValidEvents.Contains(e)))
            .WithMessage($"الأحداث المدعومة فقط هي: {string.Join(", ", ValidEvents)}");
    }
}
