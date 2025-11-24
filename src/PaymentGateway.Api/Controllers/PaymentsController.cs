using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Models.Requests;
using PaymentGateway.Domain.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, IValidator<PostPaymentRequest> paymentRequestValidator) : Controller
{
    [HttpGet("{id:guid}")]
    [ActionName("GetPayment")]
    public IActionResult GetPayment(Guid id)
    {
        var payment = paymentService.GetPaymentDetails(id);

        if (payment == null)
            return NotFound($"Payment not found for id {id}");
        
        return  Ok(payment);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPaymentAsync([FromBody]PostPaymentRequest paymentRequest)
    {
        var validationResult = await paymentRequestValidator.ValidateAsync(paymentRequest);

        if (!validationResult.IsValid)
            return ReturnInvalidRequest(validationResult);

        var paymentResponse = await paymentService.ProcessPayment(paymentRequest);

        return CreatedAtAction("GetPayment", new { id = paymentResponse.Id }, paymentResponse);

    }

    private IActionResult ReturnInvalidRequest(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .ToArray();

        var response = new PaymentErrorResponse
        {
            Status = PaymentStatus.Rejected,
            Message = "The request is invalid.",
            Errors = errors
        };

        return BadRequest(response);
    }
}