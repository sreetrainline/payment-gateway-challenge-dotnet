using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentMapper
{
    PostPaymentResponse ToResponse(Payment payment);
    GetPaymentResponse ToGetResponse(Payment payment);
    Payment ToPayment(PostPaymentRequest request, BankResponse bankResponse);
    BankPaymentRequest ToBankingRequest(PostPaymentRequest request);
}