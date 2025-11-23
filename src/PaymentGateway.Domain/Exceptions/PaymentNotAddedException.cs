namespace PaymentGateway.Domain.Exceptions;

public class PaymentNotAddedException(string message) : Exception(message);