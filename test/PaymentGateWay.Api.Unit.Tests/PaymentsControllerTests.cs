using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IValidator<PostPaymentRequest>> _validatorMock;
    private readonly PaymentsController _sut;
    private readonly Guid _paymentId = Guid.NewGuid();
    private GetPaymentResponse _getPaymentResponse;
    private readonly ValidationResult _validationResult = new();
    private PostPaymentRequest _postPaymentRequest;
    private PostPaymentResponse _paymentResponse;

    public PaymentsControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _validatorMock = new Mock<IValidator<PostPaymentRequest>>();
        _sut = new PaymentsController(_paymentServiceMock.Object, _validatorMock.Object);
    }

    [Fact]
    public void GetPaymentReturnsNotFoundWhenPaymentDoesNotExist()
    {
       _paymentServiceMock
            .Setup(s => s.GetPaymentDetails(_paymentId))
            .Returns((GetPaymentResponse?)null);

        var result =  _sut.GetPayment(_paymentId);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be($"Payment not found for id {_paymentId}");
}

    [Fact]
    public void GetPaymentReturnsOkWhenPaymentExists()
    {
        _getPaymentResponse = GetPaymentResponse();

        _paymentServiceMock
            .Setup(s => s.GetPaymentDetails(_paymentId))
            .Returns(_getPaymentResponse);
        
        var result = _sut.GetPayment(_paymentId);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(_getPaymentResponse);
    }

    [Theory]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Declined)]
    [InlineData(PaymentStatus.Rejected)]
    public async Task AddPaymentReturnsCreatedWhenRequestIsValid(PaymentStatus paymentStatus)
    {
        _postPaymentRequest = GetPostPaymentRequest();

        _validatorMock
            .Setup(v => v.ValidateAsync(_postPaymentRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_validationResult);

        _paymentResponse = GetPostPaymentResponse(paymentStatus);

        _paymentServiceMock
            .Setup(s => s.ProcessPayment(_postPaymentRequest))
            .ReturnsAsync(_paymentResponse);
        
        var result = await _sut.AddPaymentAsync(_postPaymentRequest);
        
        result.Should().BeOfType<CreatedAtActionResult>();

        var created = result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Which;

        created.ActionName.Should().Be("GetPayment");
        created.RouteValues!["id"].Should().Be(_paymentResponse.Id);
        created.Value.Should().Be(_paymentResponse);

        _validatorMock.Verify(
            v => v.ValidateAsync(_postPaymentRequest, It.IsAny<CancellationToken>()),
            Times.Once);

        _paymentServiceMock.Verify(
            s => s.ProcessPayment(_postPaymentRequest),
            Times.Once);
    }

    [Fact]
    public async Task AddPaymentReturnsBadRequestWhenRequestIsInvalid()
    {
        var request = GetPostPaymentRequest();

        var failures = new List<ValidationFailure>
        {
            new("Amount", "Amount must be greater than zero"),
            new("CardNumber", "Card number is invalid")
        };

        var validationResult = new ValidationResult(failures);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await _sut.AddPaymentAsync(request);

        var error = result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeOfType<PaymentErrorResponse>()
            .Which;

        error.Status.Should().Be(PaymentStatus.Rejected);
        error.Message.Should().Be("The request is invalid.");
        error.Errors.Should().BeEquivalentTo(new[]
        {
            "Amount must be greater than zero",
            "Card number is invalid"
        });

        _paymentServiceMock.Verify(
            s => s.ProcessPayment(It.IsAny<PostPaymentRequest>()),
            Times.Never);
    }
    
    private GetPaymentResponse GetPaymentResponse()
    {
        return new GetPaymentResponse
        {
            Id = _paymentId,
            ExpiryYear = 2030,
            ExpiryMonth = 12,
            Amount = 1000,
            CardNumberLastFour = "4567",
            Currency = "GBP",
            Status = PaymentStatus.Authorized
        };
    }
    
    private static PostPaymentRequest GetPostPaymentRequest()
    {
        return new PostPaymentRequest
        {
            ExpiryYear = 2030,
            ExpiryMonth = 12,
            Amount = 1000,
            CardNumber = "1234567891234567",
            Currency = "GBP",
            Cvv = 123
        };
    }
    private static PostPaymentResponse GetPostPaymentResponse(PaymentStatus paymentStatus)
    {
        return new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = 2030,
            ExpiryMonth = 12,
            Amount = 1000,
            CardNumberLastFour = "4567",
            Currency = "GBP",
            Status = paymentStatus
        };
    }
}
