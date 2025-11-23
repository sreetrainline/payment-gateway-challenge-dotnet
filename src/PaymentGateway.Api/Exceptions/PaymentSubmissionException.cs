namespace PaymentGateway.Api.Exceptions;

public class PaymentSubmissionException(string message) : Exception(message);