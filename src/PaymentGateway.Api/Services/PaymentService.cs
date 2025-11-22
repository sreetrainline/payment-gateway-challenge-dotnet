using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    PostPaymentResponse ProcessPayment(PostPaymentRequest payment);
    PostPaymentResponse GetPaymentDetails(Guid id);
}

public class PaymentService(IPaymentsRepository paymentsRepository, IBankClient bankClient)
    : IPaymentService
{
    public PostPaymentResponse ProcessPayment(PostPaymentRequest paymentRequest)
    {
        
        // First call bank client , So we get status 
        var bankResponse = bankClient.SubmitPayment(PaymentMapper.ToBankingRequest(paymentRequest));
        
        var payment = PaymentMapper.ToPayment(paymentRequest,bankResponse);
        
        //What happens if repository call fails ?.
        paymentsRepository.Add(payment);

        var response = PaymentMapper.ToResponse(payment);

        return response;
    }

    public PostPaymentResponse GetPaymentDetails(Guid id)
    {
        var payment = paymentsRepository.Get(id);

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
            Amount = request.Amount,
            Currency = request.Currency,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsAuthorized = bankResponse.IsAuthorized,
            AuthorisationCode = bankResponse.AuthorisationCode,
            CardNumberLastFour = request.CardNumber.Substring(11,4)
        };
    }
    
    public static BankPaymentRequest ToBankingRequest(PostPaymentRequest request)
    {
        return new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };
    }
}





public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment Get(Guid id);
}



public enum Status
{
    Authorized,
    Declined,
    Error
}