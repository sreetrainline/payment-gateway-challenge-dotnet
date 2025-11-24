using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Infrastructure;
using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentProviderTests
{
    private PaymentProvider CreatePaymentProvider(MockHttpMessageHandler mockHttp, string baseUrl)
    {
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri(baseUrl);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(client);

        var bankConfig = Options.Create(new BankConfig { BaseUrl = baseUrl });

        return new PaymentProvider(httpClientFactoryMock.Object, bankConfig);
    }

    [Fact]
    public async Task SubmitPayment_WhenResponseIsSuccessful_ReturnsBankResponse()
    {
        var baseUrl = "https://bank.test/pay";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, baseUrl)
            .Respond("application/json", @"{ ""authorized"": true, ""authorization_code"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"" }");

        var sut = CreatePaymentProvider(mockHttp, baseUrl);

        var request = new BankPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "GBP",
            Amount = 100m,
            Cvv = "123"
        };

        var result = await sut.SubmitPayment(request);

        Assert.True(result.IsAuthorized);
        Assert.Equal(new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), result.AuthorisationCode);
    }

    [Fact]
    public async Task SubmitPayment_WhenResponseIsNotSuccessful_ThrowsUnknownPaymentExceptionWithInnerPaymentSubmissionException()
    {
        var baseUrl = "https://bank.test/pay";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, baseUrl)
            .Respond(HttpStatusCode.InternalServerError);

        var sut = CreatePaymentProvider(mockHttp, baseUrl);

        var request = new BankPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "GBP",
            Amount = 100m,
            Cvv = "123"
        };

        var ex = await Assert.ThrowsAsync<UnknownPaymentException>(() => sut.SubmitPayment(request));
        Assert.IsType<PaymentSubmissionException>(ex.InnerException);
    }

    [Fact]
    public async Task SubmitPayment_WhenResponseBodyIsNull_ThrowsUnknownPaymentExceptionWithInnerPaymentSubmissionException()
    {
        var baseUrl = "https://bank.test/pay";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, baseUrl)
            .Respond("application/json", "null");

        var sut = CreatePaymentProvider(mockHttp, baseUrl);

        var request = new BankPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "GBP",
            Amount = 100m,
            Cvv = "123"
        };

        var ex = await Assert.ThrowsAsync<UnknownPaymentException>(() => sut.SubmitPayment(request));
        Assert.IsType<PaymentSubmissionException>(ex.InnerException);
    }

    [Fact]
    public async Task SubmitPayment_WhenHttpClientThrows_ThrowsUnknownPaymentException()
    {
        var baseUrl = "https://bank.test/pay";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, baseUrl)
            .Throw(new HttpRequestException("Network error"));

        var sut = CreatePaymentProvider(mockHttp, baseUrl);

        var request = new BankPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "GBP",
            Amount = 100m,
            Cvv = "123"
        };

        var ex = await Assert.ThrowsAsync<UnknownPaymentException>(() => sut.SubmitPayment(request));
        Assert.IsType<HttpRequestException>(ex.InnerException);
    }
}
