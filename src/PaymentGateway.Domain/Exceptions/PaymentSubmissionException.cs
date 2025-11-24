namespace PaymentGateway.Domain.Exceptions;

public class PaymentSubmissionException(string message) : Exception(message);