using System.Text.Json.Serialization;

namespace PaymentGateway.Infrastructure.ExternalModels;

public class BankPaymentRequest
{
    [JsonPropertyName("card_number")]
    public required string CardNumber { get; set; }
    
    [JsonPropertyName("expiry_date")]
    public required string ExpiryDate { get; set; }
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    [JsonPropertyName("amount")]
    public required decimal Amount { get; set; }
    [JsonPropertyName("cvv")]
    public required string Cvv { get; set; }
    
}