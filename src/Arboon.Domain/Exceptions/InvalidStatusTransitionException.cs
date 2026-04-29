using Arboon.Domain.Enums;

namespace Arboon.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(EscrowStatus current, EscrowStatus target)
        : base("INVALID_STATUS_TRANSITION",
            $"لا يمكن الانتقال من الحالة {current} إلى {target}")
    {
    }
}
