using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; // Change ?
using PaymentGateway.Api.Controllers;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;

using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PaymentGateway.Api.Tests;

public class PaymentsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WireMockServer _wiremock;
    private readonly HttpClient _client;
    private int _retryCounter = 0;

    public PaymentsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _wiremock = WireMockServer.Start();

        var overriddenFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var overrideConfig = new Dictionary<string, string?>
                {
                    ["BankConfig:BaseUrl"] = _wiremock.Url + "/payment"
                };

                config.AddInMemoryCollection(overrideConfig);
            });
        });

        _client = overriddenFactory.CreateClient();
     }


    [Fact]
    public async Task SuccessSetupAuthorisesPayment()
    {
        // Create a Payment
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234567");
        
        SetupBankCall(200, @"{ ""authorized"": true, ""authorization_code"": ""9f5f3e6b-3e6f-4e3d-8d22-4f0dd6b7089c"" }");
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.Equal(postPaymentResponse.Status,PaymentStatus.Authorized);
    }
    
    [Fact]
    public async Task ErrorSetupDeclinesPaymentAndRetries()
    {
        // Create a Payment
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234567");
        
        SetupBankCallError();
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.Equal(postPaymentResponse.Status,PaymentStatus.Declined);
        Assert.Equal(4,_retryCounter);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    private void SetupBankCall(int statusCode, string responseBody)
    {
        _wiremock.Given(
                Request.Create()
                    .WithPath("/payment")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(statusCode)
                    .WithBody(responseBody)
            );
    }
    
    private void SetupBankCallError()
    {
        _wiremock.Given(
                Request.Create()
                    .WithPath("/payment")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithCallback(req =>
                    {
                        _retryCounter++;
                        return new WireMock.ResponseMessage
                        {
                            StatusCode = 500
                        };
                    }));
    }
    
    private static StringContent CreateRequestContent(PostPaymentRequest paymentRequest)
    {
        return new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json");
    }
    
    private static GetPaymentResponse SetupExpectedGetResponse(Guid guid)
    {
        var expectedGetResponse = new GetPaymentResponse
        {
            Id = guid,
            ExpiryYear = 2030,
            ExpiryMonth = 12,
            Amount = 1000,
            CardNumberLastFour = "4567",
            Currency = "GBP"
        };
        return expectedGetResponse;
    }

    private static PostPaymentRequest SetupPostPaymentRequest(string cardNumber)
    {
        var request = new PostPaymentRequest()
        {
            ExpiryYear = 2030,
            ExpiryMonth = 12,
            Amount = 1000,
            CardNumber = cardNumber,
            Currency = "GBP",
            Cvv = 123
        };
        return request;
    }
}