using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentProvider
{
    Task<BankResponse> SubmitPayment(BankPaymentRequest request);
}
public class PaymentProvider(IHttpClientFactory httpClientFactory, IOptions<BankConfig> bankConfig)
    : IPaymentProvider
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly BankConfig _bankConfig = bankConfig.Value;

    public async Task<BankResponse> SubmitPayment(BankPaymentRequest request)
    {
        try
        {
            var response =   await _httpClient.PostAsJsonAsync(_bankConfig.BaseUrl, request);

            if (!response.IsSuccessStatusCode)
            {
                throw new PaymentSubmissionException($"Payment Submission failed with status {response.StatusCode} ");
            }

            var bankResponse =  await response.Content.ReadFromJsonAsync<BankResponse>();

            if (bankResponse == null)
            {
                throw new PaymentSubmissionException("Payment Submission came back with an empty response");
            }

            return bankResponse;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw new UnknownPaymentException("Unexpected Error in Payments",exception);
        }
    }
}

public class BankConfig
{
    public string BaseUrl { get; set; }
}

public class BankPaymentRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; }
    
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }
    
}

public class BankResponse
{
    [JsonPropertyName("authorized")]
    public bool IsAuthorized { get; set; }
    [JsonPropertyName("authorization_code")]
    public Guid AuthorisationCode { get; set; }
    
}