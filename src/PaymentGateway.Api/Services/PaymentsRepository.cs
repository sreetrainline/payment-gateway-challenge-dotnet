using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;



public class PaymentsRepository : IPaymentsRepository
{
    public List<Payment> Payments = new();
    
    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public Payment Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}

public class Payment
{
    public Guid Id { get; set; }
    
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
}