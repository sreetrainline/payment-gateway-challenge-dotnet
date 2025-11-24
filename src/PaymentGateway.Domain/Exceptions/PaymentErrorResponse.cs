using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Exceptions;

public class PaymentErrorResponse
{
    public PaymentStatus Status { get; set; }
    public required string Message { get; set; }
    public required string[] Errors { get; set; }
}