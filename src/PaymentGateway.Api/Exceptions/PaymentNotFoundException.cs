namespace PaymentGateway.Api.Exceptions;

public class PaymentNotFoundException(string message) : Exception(message);