using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Infrastructure;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentProviderTests
{
    private const string BaseUrl = "https://bank.test/pay";

    [Fact]
    public async Task SuccessfulSubmitPaymentShouldReturnResponse()
    {
        var mockHttp = new MockHttpMessageHandler();
        
        mockHttp.When(HttpMethod.Post, BaseUrl)
            .Respond("application/json", @"{ ""authorized"": true, ""authorization_code"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"" }");

        var sut = CreatePaymentProvider(mockHttp, BaseUrl);

        var request = GetBankRequest();

        var result = await sut.SubmitPayment(request);

        result.IsAuthorized.Should().BeTrue();
        result.AuthorisationCode.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
    }

    [Fact]
    public async Task UnSuccessfulSubmitPaymentThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, BaseUrl)
            .Respond(HttpStatusCode.InternalServerError);

        var sut = CreatePaymentProvider(mockHttp, BaseUrl);

        var request = GetBankRequest();

        var act = () => sut.SubmitPayment(request);
        var ex = await act.Should().ThrowAsync<UnknownPaymentException>();
    }

    [Fact]
    public async Task SubmitPaymentWithNoResponseBodyThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, BaseUrl)
            .Respond("application/json", "null");

        var sut = CreatePaymentProvider(mockHttp, BaseUrl);

        var request = GetBankRequest();

        var act = () => sut.SubmitPayment(request);
        var ex = await act.Should().ThrowAsync<UnknownPaymentException>();
    }

    [Fact]
    public async Task SubmitPaymentExceptionCausesError()
    { 
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, BaseUrl)
            .Throw(new HttpRequestException("Network error"));

        var sut = CreatePaymentProvider(mockHttp, BaseUrl);

        var request = GetBankRequest();

        var act = () => sut.SubmitPayment(request);

        var ex = await act.Should().ThrowAsync<UnknownPaymentException>();
        ex.Which.InnerException.Should().BeOfType<HttpRequestException>();
    }
    
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
    
    private static BankPaymentRequest GetBankRequest()
    {
        return new BankPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "GBP",
            Amount = 100m,
            Cvv = "123"
        };
    }
}
