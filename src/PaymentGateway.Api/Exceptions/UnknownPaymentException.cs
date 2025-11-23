namespace PaymentGateway.Api.Exceptions;

public class UnknownPaymentException(string message, Exception inner): Exception(message,inner)
{
    
}