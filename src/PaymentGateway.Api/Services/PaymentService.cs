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
        var payment = PaymentMapper.ToPayment(paymentRequest);
        
        // First call bank client , So we get status 
        //var bankResponse = bankClient.SubmitPayment(paymentRequest);
        
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
            ExpiryYear = payment.ExpiryYear
        };
    }

    public static Payment ToPayment(PostPaymentRequest request)
    {
        return new Payment
        {
            Id = new Guid(),
            Amount = request.Amount,
            Currency = request.Currency,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear
        };
    }
}





public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment Get(Guid id);
}

public interface IBankClient
{
    Status SubmitPayment(PostPaymentResponse payment);
}

public class BankClient : IBankClient
{
    public Status SubmitPayment(PostPaymentResponse payment)
    {
        throw new NotImplementedException();
    }
}

public enum Status
{
    Authorized,
    Declined
}