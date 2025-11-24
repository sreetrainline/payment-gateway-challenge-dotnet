using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest payment);
    GetPaymentResponse? GetPaymentDetails(Guid id);
}