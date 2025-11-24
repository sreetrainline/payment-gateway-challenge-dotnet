using PaymentGateway.Api.Services;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;
using System;
using System.Threading.Tasks;
using Moq;
using PaymentGateway.Domain.ExternalModels;
using Xunit;

namespace PaymentGateWay.Api.Unit.Tests;


public class PaymentServiceTests
{
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock;
    private readonly Mock<IPaymentProvider> _paymentProviderMock;
    private readonly Mock<IPaymentMapper> _paymentMapperMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
        _paymentProviderMock = new Mock<IPaymentProvider>();
        _paymentMapperMock = new Mock<IPaymentMapper>();
        _sut = new PaymentService(_paymentsRepositoryMock.Object, _paymentProviderMock.Object, _paymentMapperMock.Object);
    }

    [Fact]
    public async Task ProcessPayment_WhenProviderSucceeds_ReturnsMappedResponseAndSavesPayment()
    {
        var request = new PostPaymentRequest();
        var bankRequest = new BankPaymentRequest();
        var bankResponse = new BankResponse { IsAuthorized = true, AuthorisationCode = Guid.NewGuid() };
        var payment = new Payment();
        var mappedResponse = new PostPaymentResponse();

        _paymentMapperMock
            .Setup(m => m.ToBankingRequest(request))
            .Returns(bankRequest);

        _paymentProviderMock
            .Setup(p => p.SubmitPayment(bankRequest))
            .ReturnsAsync(bankResponse);

        _paymentMapperMock
            .Setup(m => m.ToPayment(request, bankResponse))
            .Returns(payment);

        _paymentMapperMock
            .Setup(m => m.ToResponse(payment))
            .Returns(mappedResponse);

        var result = await _sut.ProcessPayment(request);

        Assert.Same(mappedResponse, result);
        _paymentMapperMock.Verify(m => m.ToBankingRequest(request), Times.Once);
        _paymentProviderMock.Verify(p => p.SubmitPayment(bankRequest), Times.Once);
        _paymentMapperMock.Verify(m => m.ToPayment(request, bankResponse), Times.Once);
        _paymentsRepositoryMock.Verify(r => r.Add(payment), Times.Once);
        _paymentMapperMock.Verify(m => m.ToResponse(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_WhenProviderThrowsUnknownPaymentException_ReturnsDeclinedAndSavesPayment()
    {
        var request = new PostPaymentRequest();
        var bankRequest = new BankPaymentRequest();
        var payment = new Payment();
        var mappedResponse = new PostPaymentResponse();

        _paymentMapperMock
            .Setup(m => m.ToBankingRequest(request))
            .Returns(bankRequest);

        _paymentProviderMock
            .Setup(p => p.SubmitPayment(bankRequest))
            .ThrowsAsync(new UnknownPaymentException("unknown",new Exception()));

        _paymentMapperMock
            .Setup(m => m.ToPayment(
                request,
                It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)))
            .Returns(payment);

        _paymentMapperMock
            .Setup(m => m.ToResponse(payment))
            .Returns(mappedResponse);

        var result = await _sut.ProcessPayment(request);

        Assert.Same(mappedResponse, result);
        _paymentMapperMock.Verify(m => m.ToBankingRequest(request), Times.Once);
        _paymentProviderMock.Verify(p => p.SubmitPayment(bankRequest), Times.Once);
        _paymentMapperMock.Verify(m => m.ToPayment(
            request,
            It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)),
            Times.Once);
        _paymentsRepositoryMock.Verify(r => r.Add(payment), Times.Once);
        _paymentMapperMock.Verify(m => m.ToResponse(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_WhenProviderThrowsPaymentSubmissionException_ReturnsDeclinedAndSavesPayment()
    {
        var request = new PostPaymentRequest();
        var bankRequest = new BankPaymentRequest();
        var payment = new Payment();
        var mappedResponse = new PostPaymentResponse();

        _paymentMapperMock
            .Setup(m => m.ToBankingRequest(request))
            .Returns(bankRequest);

        _paymentProviderMock
            .Setup(p => p.SubmitPayment(bankRequest))
            .ThrowsAsync(new PaymentSubmissionException("submit failed"));

        _paymentMapperMock
            .Setup(m => m.ToPayment(
                request,
                It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)))
            .Returns(payment);

        _paymentMapperMock
            .Setup(m => m.ToResponse(payment))
            .Returns(mappedResponse);

        var result = await _sut.ProcessPayment(request);

        Assert.Same(mappedResponse, result);
        _paymentMapperMock.Verify(m => m.ToBankingRequest(request), Times.Once);
        _paymentProviderMock.Verify(p => p.SubmitPayment(bankRequest), Times.Once);
        _paymentMapperMock.Verify(m => m.ToPayment(
            request,
            It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)),
            Times.Once);
        _paymentsRepositoryMock.Verify(r => r.Add(payment), Times.Once);
        _paymentMapperMock.Verify(m => m.ToResponse(payment), Times.Once);
    }

    [Fact]
    public void GetPaymentDetails_WhenPaymentExists_ReturnsMappedResponse()
    {
        var id = Guid.NewGuid();
        var payment = new Payment();
        var mappedGetResponse = new GetPaymentResponse();

        _paymentsRepositoryMock
            .Setup(r => r.Get(id))
            .Returns(payment);

        _paymentMapperMock
            .Setup(m => m.ToGetResponse(payment))
            .Returns(mappedGetResponse);

        var result = _sut.GetPaymentDetails(id);

        Assert.Same(mappedGetResponse, result);
        _paymentsRepositoryMock.Verify(r => r.Get(id), Times.Once);
        _paymentMapperMock.Verify(m => m.ToGetResponse(payment), Times.Once);
    }

    [Fact]
    public void GetPaymentDetails_WhenPaymentDoesNotExist_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _paymentsRepositoryMock
            .Setup(r => r.Get(id))
            .Returns((Payment)null);

        var result = _sut.GetPaymentDetails(id);

        Assert.Null(result);
        _paymentsRepositoryMock.Verify(r => r.Get(id), Times.Once);
        _paymentMapperMock.Verify(m => m.ToGetResponse(It.IsAny<Payment>()), Times.Never);
    }
}
