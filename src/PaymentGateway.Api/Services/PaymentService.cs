using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest payment);
    PostPaymentResponse GetPaymentDetails(Guid id);
}

public class PaymentService(IPaymentsRepository paymentsRepository, IPaymentProvider paymentProvider)
    : IPaymentService
{
    public async Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest)
    {
        try
        {
            var bankResponse = await paymentProvider.SubmitPayment(PaymentMapper.ToBankingRequest(paymentRequest));

            return CreatePaymentResponse(paymentRequest, bankResponse);
        }
        catch (Exception exception ) when (exception is UnknownPaymentException or PaymentSubmissionException)
        {
            return CreatePaymentResponse(paymentRequest,
                new BankResponse() { IsAuthorized = false, AuthorisationCode = Guid.Empty });
        }
    }

    private PostPaymentResponse CreatePaymentResponse(PostPaymentRequest paymentRequest, BankResponse bankResponse)
    {
        var payment = PaymentMapper.ToPayment(paymentRequest,bankResponse);
        
        paymentsRepository.Add(payment);

        var response = PaymentMapper.ToResponse(payment);

        return response;
    }

    public PostPaymentResponse GetPaymentDetails(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        
        if (payment == null)
            throw new PaymentNotFoundException($"Payment with id {payment.Id} not Found");

        return PaymentMapper.ToResponse(payment);
    }
}

public static class PaymentMapper
{
    public static PostPaymentResponse ToResponse(Payment payment)
    {
        return new PostPaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Status = payment.IsAuthorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = payment.CardNumberLastFour
        };
    }

    public static Payment ToPayment(PostPaymentRequest request, BankResponse bankResponse)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = Convert.ToDecimal(request.Amount),
            Currency = request.Currency,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsAuthorized = bankResponse.IsAuthorized,
            AuthorisationCode = bankResponse.AuthorisationCode,
            CardNumberLastFour = request.CardNumber[^4..]
        };
    }
    
    public static BankPaymentRequest ToBankingRequest(PostPaymentRequest request)
    {
        return new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = Convert.ToDecimal(request.Amount),
            Cvv = request.Cvv.ToString()
        };
    }
}

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}



public enum Status
{
    Authorized,
    Declined,
    Rejected
}