using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateWay.Api.Unit.Tests;


public class PaymentMapperTests
{
    private readonly PaymentMapper _sut = new();

    private Payment CreateAuthorizedPayment()
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = 12345,
            Currency = "GBP",
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            IsAuthorized = true,
            AuthorisationCode = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            CardNumberLastFour = "1234"
        };
    }

    private Payment CreateDeclinedPayment()
    {
        var payment = CreateAuthorizedPayment();
        payment.IsAuthorized = false;
        return payment;
    }

    private PostPaymentRequest CreatePostPaymentRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 10,
            ExpiryYear = 2032,
            Currency = "USD",
            Amount = 9999,
            Cvv = 123
        };
    }

    private BankResponse CreateBankResponse(bool authorized = true)
    {
        return new BankResponse
        {
            IsAuthorized = authorized,
            AuthorisationCode = authorized ? new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6") : Guid.Empty
        };
    }

    [Fact]
    public void ToResponse_Maps_All_Fields_For_Authorized_Payment()
    {
        var payment = CreateAuthorizedPayment();
        var result = _sut.ToResponse(payment);

        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(payment.Currency, result.Currency);
        Assert.Equal(payment.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(payment.ExpiryYear, result.ExpiryYear);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal(payment.CardNumberLastFour, result.CardNumberLastFour);
    }

    [Fact]
    public void ToResponse_Maps_Status_As_Declined_When_Not_Authorized()
    {
        var payment = CreateDeclinedPayment();
        var result = _sut.ToResponse(payment);

        Assert.Equal(PaymentStatus.Declined, result.Status);
    }

    [Fact]
    public void ToGetResponse_Maps_All_Fields_For_Authorized_Payment()
    {
        var payment = CreateAuthorizedPayment();
        var result = _sut.ToGetResponse(payment);

        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(payment.Currency, result.Currency);
        Assert.Equal(payment.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(payment.ExpiryYear, result.ExpiryYear);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal(payment.CardNumberLastFour, result.CardNumberLastFour);
    }

    [Fact]
    public void ToGetResponse_Maps_Status_As_Declined_When_Not_Authorized()
    {
        var payment = CreateDeclinedPayment();
        var result = _sut.ToGetResponse(payment);

        Assert.Equal(PaymentStatus.Declined, result.Status);
    }

    [Fact]
    public void ToPayment_Maps_Request_And_BankResponse_Correctly()
    {
        var request = CreatePostPaymentRequest();
        var bankResponse = CreateBankResponse(true);
        var payment = _sut.ToPayment(request, bankResponse);

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(request.Amount, payment.Amount);
        Assert.Equal(request.Currency, payment.Currency);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(bankResponse.IsAuthorized, payment.IsAuthorized);
        Assert.Equal(bankResponse.AuthorisationCode, payment.AuthorisationCode);
        Assert.Equal(request.CardNumber[^4..], payment.CardNumberLastFour);
    }

    [Fact]
    public void ToPayment_Sets_IsAuthorized_From_BankResponse()
    {
        var request = CreatePostPaymentRequest();
        var bankResponse = CreateBankResponse(false);
        var payment = _sut.ToPayment(request, bankResponse);

        Assert.False(payment.IsAuthorized);
        Assert.Equal(Guid.Empty, payment.AuthorisationCode);
    }

    [Fact]
    public void ToBankingRequest_Maps_All_Fields_Correctly()
    {
        var request = CreatePostPaymentRequest();
        request.ExpiryMonth = 5;
        request.ExpiryYear = 2040;
        request.Amount = 12345;
        request.Cvv = 987;

        var bankRequest = _sut.ToBankingRequest(request);

        Assert.Equal(request.CardNumber, bankRequest.CardNumber);
        Assert.Equal("5/2040", bankRequest.ExpiryDate);
        Assert.Equal(request.Currency, bankRequest.Currency);
        Assert.Equal(Convert.ToDecimal(request.Amount), bankRequest.Amount);
        Assert.Equal(request.Cvv.ToString(), bankRequest.Cvv);
    }
}
