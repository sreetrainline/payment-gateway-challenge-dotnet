using PaymentGateway.Api.Services;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Application.Services;

public class PaymentService(IPaymentsRepository paymentsRepository, IPaymentProvider paymentProvider, IPaymentMapper paymentMapper)
    : IPaymentService
{
    public async Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest)
    {
        try
        {
            var bankResponse = await paymentProvider.SubmitPayment(paymentMapper.ToBankingRequest(paymentRequest));

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
        var payment = paymentMapper.ToPayment(paymentRequest,bankResponse);
        
        paymentsRepository.Add(payment);

        return paymentMapper.ToResponse(payment);
    }

    public GetPaymentResponse? GetPaymentDetails(Guid id)
    {
        var payment = paymentsRepository.Get(id);

        return payment == null ? null : paymentMapper.ToGetResponse(payment);
    }
}