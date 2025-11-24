using FluentAssertions;

using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateWay.Api.Unit.Tests;


public class PaymentMapperTests
{
    private readonly PaymentMapper _sut = new();
    private static BankResponse CreateBankResponse(bool authorized = true)
    {
        return new BankResponse
        {
            IsAuthorized = authorized,
            AuthorisationCode = authorized ? new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6") : Guid.Empty
        };
    }

    [Fact]
    public void ToResponseMapsAllFieldsForAuthorizedPayment()
    {
        var payment = CreateAuthorizedPayment();

        var result = _sut.ToResponse(payment);

        result.Id.Should().Be(payment.Id);
        result.Amount.Should().Be(payment.Amount);
        result.Currency.Should().Be(payment.Currency);
        result.ExpiryMonth.Should().Be(payment.ExpiryMonth);
        result.ExpiryYear.Should().Be(payment.ExpiryYear);
        result.Status.Should().Be(PaymentStatus.Authorized);
        result.CardNumberLastFour.Should().Be(payment.CardNumberLastFour);
    }

    [Fact]
    public void ToResponseMapsStatusAsDeclinedWhenNotAuthorized()
    {
        var payment = CreateDeclinedPayment();

        var result = _sut.ToResponse(payment);

        result.Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public void ToGetResponseMapsAllFieldsForAuthorizedPayment()
    {
        var payment = CreateAuthorizedPayment();

        var result = _sut.ToGetResponse(payment);

        result.Id.Should().Be(payment.Id);
        result.Amount.Should().Be(payment.Amount);
        result.Currency.Should().Be(payment.Currency);
        result.ExpiryMonth.Should().Be(payment.ExpiryMonth);
        result.ExpiryYear.Should().Be(payment.ExpiryYear);
        result.Status.Should().Be(PaymentStatus.Authorized);
        result.CardNumberLastFour.Should().Be(payment.CardNumberLastFour);
    }

    [Fact]
    public void ToGetResponseMapsStatusAsDeclinedWhenNotAuthorized()
    {
        var payment = CreateDeclinedPayment();

        var result = _sut.ToGetResponse(payment);

        result.Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public void ToPaymentMapsRequestAndBankResponseCorrectly()
    {
        var request = CreatePostPaymentRequest();
        var bankResponse = CreateBankResponse(true);

        var payment = _sut.ToPayment(request, bankResponse);

        payment.Id.Should().NotBe(Guid.Empty);
        payment.Amount.Should().Be(request.Amount);
        payment.Currency.Should().Be(request.Currency);
        payment.ExpiryMonth.Should().Be(request.ExpiryMonth);
        payment.ExpiryYear.Should().Be(request.ExpiryYear);
        payment.IsAuthorized.Should().Be(bankResponse.IsAuthorized);
        payment.AuthorisationCode.Should().Be(bankResponse.AuthorisationCode);
        payment.CardNumberLastFour.Should().Be(request.CardNumber[^4..]);
    }

    [Fact]
    public void ToPaymentSetsIsAuthorizedFromBankResponse()
    {
        var request = CreatePostPaymentRequest();
        var bankResponse = CreateBankResponse(false);

        var payment = _sut.ToPayment(request, bankResponse);

        payment.IsAuthorized.Should().BeFalse();
        payment.AuthorisationCode.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToBankingRequestMapsAllFieldsCorrectly()
    {
        var request = CreatePostPaymentRequest();
        request.ExpiryMonth = 5;
        request.ExpiryYear = 2040;
        request.Amount = 12345;
        request.Cvv = 987;

        var bankRequest = _sut.ToBankingRequest(request);

        bankRequest.CardNumber.Should().Be(request.CardNumber);
        bankRequest.ExpiryDate.Should().Be("5/2040");
        bankRequest.Currency.Should().Be(request.Currency);
        bankRequest.Amount.Should().Be(request.Amount);
        bankRequest.Cvv.Should().Be(request.Cvv.ToString());
    }
    
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
}
