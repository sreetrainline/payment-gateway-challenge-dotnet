using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using FluentAssertions;
using PaymentGateway.Domain.Enums;
using Xunit;

namespace PaymentGateway.Api.Integration.Tests;

public class PaymentsIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Random _random = new();
    
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SavesAndRetrievesAPaymentSuccessfully()
    {
        // Create a Payment
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234567");
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        
        //Get Payment
        GetPaymentResponse expectedGetResponse = SetupExpectedGetResponse(postPaymentResponse.Id);
       
        var response = await _client.GetAsync(postResponse.Headers.Location);
        
        var getPaymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();
        
        //Verify that the same values come back 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        getPaymentResponse.Should().BeEquivalentTo(expectedGetResponse);
    }
    
    [Fact]
    public async Task CardErrorReturnsDeclinedPayment()
    {
        // Create a Payment
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234560");
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        //Verify that the same values come back 
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        postPaymentResponse.Status.Should().Be(PaymentStatus.Declined);
    }
    
    [Fact]
    public async Task CardSubmissionSuccessReturnsAuthorizedPayment()
    {
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234567");
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        postPaymentResponse.Status.Should().Be(PaymentStatus.Authorized);
    }
    
    [Fact]
    public async Task CardSubmissionFailureReturnsAuthorizedPayment()
    {
        PostPaymentRequest request = SetupPostPaymentRequest("1234567891234568");
        
        var postResponse = await _client.PostAsync($"/api/payments",CreateRequestContent(request));
        
        var postPaymentResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        postPaymentResponse.Status.Should().Be(PaymentStatus.Declined);
    }
    
    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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