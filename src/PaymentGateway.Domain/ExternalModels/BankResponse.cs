using System.Text.Json.Serialization;

namespace PaymentGateway.Infrastructure.ExternalModels;

public class BankResponse
{
    [JsonPropertyName("authorized")]
    public bool IsAuthorized { get; set; }
    [JsonPropertyName("authorization_code")]
    public Guid AuthorisationCode { get; set; }
    
}