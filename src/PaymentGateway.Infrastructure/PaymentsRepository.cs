using System.Collections.Concurrent;

using PaymentGateway.Api.Services;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Infrastructure;

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