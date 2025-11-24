namespace PaymentGateway.Domain.Models;

public class Payment
{
    public Guid Id { get; set; }
    
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public long Amount { get; set; }
    
    public bool IsAuthorized { get; set; }
    public Guid AuthorisationCode { get; set; }
}