using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure;
using System;
using Xunit;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentsRepositoryTests
{
    [Fact]
    public void Add_NullPayment_ThrowsArgumentNullException()
    {
        var repository = new PaymentsRepository();

        Assert.Throws<ArgumentNullException>(() => repository.Add(null));
    }

    [Fact]
    public void Add_PaymentWithEmptyId_ThrowsArgumentException()
    {
        var repository = new PaymentsRepository();
        var payment = new Payment { Id = Guid.Empty };

        Assert.Throws<ArgumentException>(() => repository.Add(payment));
    }

    [Fact]
    public void Add_ValidPayment_StoresPayment()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };

        repository.Add(payment);
        var result = repository.Get(id);

        Assert.Same(payment, result);
    }

    [Fact]
    public void Add_DuplicatePaymentId_ThrowsPaymentNotAddedException()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };

        repository.Add(payment);

        Assert.Throws<PaymentNotAddedException>(() => repository.Add(payment));
    }

    [Fact]
    public void Get_PaymentDoesNotExist_ReturnsNull()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();

        var result = repository.Get(id);

        Assert.Null(result);
    }

    [Fact]
    public void Get_PaymentExists_ReturnsPayment()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };

        repository.Add(payment);
        var result = repository.Get(id);

        Assert.Same(payment, result);
    }
}
