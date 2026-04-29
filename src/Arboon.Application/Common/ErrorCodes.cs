namespace Arboon.Application.Common;

/// <summary>
/// Constants for error codes used across the application.
/// </summary>
public static class ErrorCodes
{
    public const string EscrowNotFound = "ESCROW_NOT_FOUND";
    public const string InvalidStatusTransition = "INVALID_STATUS_TRANSITION";
    public const string InvalidBuyerToken = "INVALID_BUYER_TOKEN";
    public const string TokenAlreadyUsed = "TOKEN_ALREADY_USED";
    public const string PaymentFailed = "PAYMENT_FAILED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InternalError = "INTERNAL_ERROR";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
}
