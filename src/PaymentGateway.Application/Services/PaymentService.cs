using PaymentGateway.Api.Services;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Application.Services;

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