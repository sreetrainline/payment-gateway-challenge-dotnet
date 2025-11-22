using System.Text.Json.Serialization;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IBankClient
{
    BankResponse SubmitPayment(BankPaymentRequest request);
}
public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;

    public BankClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public BankResponse SubmitPayment(BankPaymentRequest request)
    {
        var response =   _httpClient.PostAsJsonAsync("http://localhost:8080/payments", request).Result;

        return  response.Content.ReadFromJsonAsync<BankResponse>().Result;
    }
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
    public int Cvv { get; set; }
    
}

public class BankResponse
{
    [JsonPropertyName("authorized")]
    public bool IsAuthorized { get; set; }
    [JsonPropertyName("authorization_code")]
    public Guid AuthorisationCode { get; set; }
    
}