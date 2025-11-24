using FluentAssertions;
using Moq;
using PaymentGateway.Api.Services;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateWay.Api.Unit.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock;
        private readonly Mock<IPaymentProvider> _paymentProviderMock;
        private readonly Mock<IPaymentMapper> _paymentMapperMock;
        private readonly PaymentService _sut;
        
        private readonly PostPaymentRequest _postPaymentRequest;
        private readonly BankPaymentRequest _bankPaymentRequest;
        private readonly Payment _payment;
        private readonly PostPaymentResponse _postPaymentResponse;
        private readonly GetPaymentResponse _getPaymentResponse;
        private readonly BankResponse _successfulBankResponse;
        private readonly Guid _existingPaymentId;

        public PaymentServiceTests()
        {
            _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
            _paymentProviderMock = new Mock<IPaymentProvider>();
            _paymentMapperMock = new Mock<IPaymentMapper>();

            _sut = new PaymentService(
                _paymentsRepositoryMock.Object,
                _paymentProviderMock.Object,
                _paymentMapperMock.Object);

            _postPaymentRequest = new PostPaymentRequest();
            _bankPaymentRequest = new BankPaymentRequest();
            _payment = new Payment();
            _postPaymentResponse = new PostPaymentResponse();
            _getPaymentResponse = new GetPaymentResponse();
            _existingPaymentId = Guid.NewGuid();

            _successfulBankResponse = new BankResponse
            {
                IsAuthorized = true,
                AuthorisationCode = Guid.NewGuid()
            };
        }

        [Fact]
        public async Task ProcessPaymentWhenProviderSucceedsReturnsMappedResponseAndSavesPayment()
        {
            _paymentMapperMock
                .Setup(m => m.ToBankingRequest(_postPaymentRequest))
                .Returns(_bankPaymentRequest);

            _paymentProviderMock
                .Setup(p => p.SubmitPayment(_bankPaymentRequest))
                .ReturnsAsync(_successfulBankResponse);

            _paymentMapperMock
                .Setup(m => m.ToPayment(_postPaymentRequest, _successfulBankResponse))
                .Returns(_payment);

            _paymentMapperMock
                .Setup(m => m.ToResponse(_payment))
                .Returns(_postPaymentResponse);

            var result = await _sut.ProcessPayment(_postPaymentRequest);

            result.Should().BeSameAs(_postPaymentResponse);

            _paymentMapperMock.Verify(m => m.ToBankingRequest(_postPaymentRequest), Times.Once);
            _paymentProviderMock.Verify(p => p.SubmitPayment(_bankPaymentRequest), Times.Once);
            _paymentMapperMock.Verify(m => m.ToPayment(_postPaymentRequest, _successfulBankResponse), Times.Once);
            _paymentsRepositoryMock.Verify(r => r.Add(_payment), Times.Once);
            _paymentMapperMock.Verify(m => m.ToResponse(_payment), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentWhenProviderThrowsUnknownPaymentExceptionReturnsDeclinedAndSavesPayment()
        {
            _paymentMapperMock
                .Setup(m => m.ToBankingRequest(_postPaymentRequest))
                .Returns(_bankPaymentRequest);

            _paymentProviderMock
                .Setup(p => p.SubmitPayment(_bankPaymentRequest))
                .ThrowsAsync(new UnknownPaymentException("unknown", new Exception()));

            _paymentMapperMock
                .Setup(m => m.ToPayment(
                    _postPaymentRequest,
                    It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)))
                .Returns(_payment);

            _paymentMapperMock
                .Setup(m => m.ToResponse(_payment))
                .Returns(_postPaymentResponse);

            var result = await _sut.ProcessPayment(_postPaymentRequest);

            result.Should().BeSameAs(_postPaymentResponse);

            _paymentMapperMock.Verify(m => m.ToBankingRequest(_postPaymentRequest), Times.Once);
            _paymentProviderMock.Verify(p => p.SubmitPayment(_bankPaymentRequest), Times.Once);
            _paymentMapperMock.Verify(
                m => m.ToPayment(
                    _postPaymentRequest,
                    It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)),
                Times.Once);
            _paymentsRepositoryMock.Verify(r => r.Add(_payment), Times.Once);
            _paymentMapperMock.Verify(m => m.ToResponse(_payment), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentWhenProviderThrowsPaymentSubmissionExceptionReturnsDeclinedAndSavesPayment()
        {
            _paymentMapperMock
                .Setup(m => m.ToBankingRequest(_postPaymentRequest))
                .Returns(_bankPaymentRequest);

            _paymentProviderMock
                .Setup(p => p.SubmitPayment(_bankPaymentRequest))
                .ThrowsAsync(new PaymentSubmissionException("submit failed"));

            _paymentMapperMock
                .Setup(m => m.ToPayment(
                    _postPaymentRequest,
                    It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)))
                .Returns(_payment);

            _paymentMapperMock
                .Setup(m => m.ToResponse(_payment))
                .Returns(_postPaymentResponse);

            var result = await _sut.ProcessPayment(_postPaymentRequest);

            result.Should().BeSameAs(_postPaymentResponse);

            _paymentMapperMock.Verify(m => m.ToBankingRequest(_postPaymentRequest), Times.Once);
            _paymentProviderMock.Verify(p => p.SubmitPayment(_bankPaymentRequest), Times.Once);
            _paymentMapperMock.Verify(
                m => m.ToPayment(
                    _postPaymentRequest,
                    It.Is<BankResponse>(b => b.IsAuthorized == false && b.AuthorisationCode == Guid.Empty)),
                Times.Once);
            _paymentsRepositoryMock.Verify(r => r.Add(_payment), Times.Once);
            _paymentMapperMock.Verify(m => m.ToResponse(_payment), Times.Once);
        }

        [Fact]
        public void GetPaymentDetailsWhenPaymentExistsReturnsMappedResponse()
        {
            _paymentsRepositoryMock
                .Setup(r => r.Get(_existingPaymentId))
                .Returns(_payment);

            _paymentMapperMock
                .Setup(m => m.ToGetResponse(_payment))
                .Returns(_getPaymentResponse);

            var result = _sut.GetPaymentDetails(_existingPaymentId);

            result.Should().BeSameAs(_getPaymentResponse);

            _paymentsRepositoryMock.Verify(r => r.Get(_existingPaymentId), Times.Once);
            _paymentMapperMock.Verify(m => m.ToGetResponse(_payment), Times.Once);
        }

        [Fact]
        public void GetPaymentDetailsWhenPaymentDoesNotExistReturnsNull()
        {
            var id = Guid.NewGuid();

            _paymentsRepositoryMock
                .Setup(r => r.Get(id))
                .Returns((Payment)null);

            var result = _sut.GetPaymentDetails(id);

            result.Should().BeNull();

            _paymentsRepositoryMock.Verify(r => r.Get(id), Times.Once);
            _paymentMapperMock.Verify(m => m.ToGetResponse(It.IsAny<Payment>()), Times.Never);
        }
    }
}
