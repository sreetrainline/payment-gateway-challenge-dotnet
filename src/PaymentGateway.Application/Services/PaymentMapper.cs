using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Application.Services;

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