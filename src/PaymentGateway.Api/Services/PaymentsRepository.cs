using System.Collections.Concurrent;
using PaymentGateway.Api.Exceptions;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid,Payment> _paymentsStorage = new();
    
    public void Add(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));
        
        if (payment.Id == Guid.Empty)
            throw new ArgumentException(nameof(payment.Id));
        
        var paymentAdded = _paymentsStorage.TryAdd(payment.Id,payment);

        if (!paymentAdded)
            throw new PaymentNotAddedException($"Payment with id {payment.Id} already exists");
    }

    public Payment? Get(Guid id)
    {
        return _paymentsStorage.GetValueOrDefault(id);
    }
}

public class Payment
{
    public Guid Id { get; set; }
    
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    
    public bool IsAuthorized { get; set; }
    public Guid AuthorisationCode { get; set; }
}