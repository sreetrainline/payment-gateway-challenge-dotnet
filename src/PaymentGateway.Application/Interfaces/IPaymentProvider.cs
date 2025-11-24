using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentProvider
{
    Task<BankResponse> SubmitPayment(BankPaymentRequest request);
}