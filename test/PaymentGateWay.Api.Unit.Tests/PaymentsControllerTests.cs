using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
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

    public PaymentsControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _validatorMock = new Mock<IValidator<PostPaymentRequest>>();
        _sut = new PaymentsController(_paymentServiceMock.Object, _validatorMock.Object);
    }

    [Fact]
    public void GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _paymentServiceMock
            .Setup(s => s.GetPaymentDetails(id))
            .Returns((GetPaymentResponse?)null);

        // Act
        var result = _sut.GetPaymentAsync(id);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundObjectResult>();

        var notFound = (NotFoundObjectResult)actionResult!;
        notFound.Value.Should().Be($"Payment not found for id {id}");
    }

    [Fact]
    public void GetPayment_ReturnsOk_WhenPaymentExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentResponse = new GetPaymentResponse
        {
            // set properties as needed, e.g.
            Id = id
        };

        _paymentServiceMock
            .Setup(s => s.GetPaymentDetails(id))
            .Returns(paymentResponse);

        // Act
        var result = _sut.GetPaymentAsync(id);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();

        var okResult = (OkObjectResult)actionResult!;
        okResult.Value.Should().Be(paymentResponse);
    }
    
    [Fact]
    public async Task AddPayment_ReturnsCreatedAtAction_WhenRequestIsValid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            // fill required fields
        };

        var validationResult = new ValidationResult(); // IsValid == true
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var paymentResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            // other props as needed
        };

        _paymentServiceMock
            .Setup(s => s.ProcessPayment(request))
            .ReturnsAsync(paymentResponse);

        // Act
        var result = await _sut.AddPaymentAsync(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var created = (CreatedAtActionResult)result.Result!;
        created.ActionName.Should().Be("GetPayment");
        created.RouteValues.Should().NotBeNull();
        created.RouteValues!["id"].Should().Be(paymentResponse.Id);
        created.Value.Should().Be(paymentResponse);

        _validatorMock.Verify(
            v => v.ValidateAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);

        _paymentServiceMock.Verify(
            s => s.ProcessPayment(request),
            Times.Once);
    }

    [Fact]
    public async Task AddPayment_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            // fill required fields (even invalid ones)
        };

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Amount", "Amount must be greater than zero"),
            new ValidationFailure("CardNumber", "Card number is invalid")
        };

        var validationResult = new ValidationResult(failures);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _sut.AddPaymentAsync(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result.Result!;
        badRequest.Value.Should().BeOfType<PaymentErrorResponse>();

        var errorResponse = (PaymentErrorResponse)badRequest.Value!;
        errorResponse.Status.Should().Be(PaymentStatus.Rejected);
        errorResponse.Message.Should().Be("The request is invalid.");
        errorResponse.Errors.Should().BeEquivalentTo(new[]
        {
            "Amount must be greater than zero",
            "Card number is invalid"
        });

        _paymentServiceMock.Verify(
            s => s.ProcessPayment(It.IsAny<PostPaymentRequest>()),
            Times.Never);
    }
}
