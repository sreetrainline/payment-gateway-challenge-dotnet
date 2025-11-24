namespace PaymentGateway.Domain.Exceptions;

public class PaymentNotFoundException(string message) : Exception(message);