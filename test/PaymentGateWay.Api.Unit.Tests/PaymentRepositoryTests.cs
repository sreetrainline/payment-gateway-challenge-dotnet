using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure;
using System;

using FluentAssertions;

using Xunit;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentsRepositoryTests
{
    [Fact]
    public void NullPaymentThrowsException()
    {
        Action act = () => new PaymentsRepository().Add(null);
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EmptyPaymentIdThrowsException()
    {
        var repository = new PaymentsRepository();
        var payment = new Payment { Id = Guid.Empty };
        
        Action act = () => new PaymentsRepository().Add(payment);

        act.Should().Throw<ArgumentException>();

    }

    [Fact]
    public void ValidPaymentStoresPayment()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };

        repository.Add(payment);
        var result = repository.Get(id);

        result.Should().BeEquivalentTo(payment);
    }

    [Fact]
    public void DuplicatePaymentIdThrowsException()
    {
        var paymentRepository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };
        
        paymentRepository.Add(payment);
        
        Action act = () => paymentRepository.Add(payment);

        act.Should().Throw<PaymentNotAddedException>();
    }

    [Fact]
    public void NoPaymentReturnsNull()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();

        var result = repository.Get(id);

        result.Should().BeNull();
    }

    [Fact]
    public void PaymentExistsReturnsPayment()
    {
        var repository = new PaymentsRepository();
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };

        repository.Add(payment);
        var result = repository.Get(id);

        result.Should().BeEquivalentTo(payment);
    }
}
