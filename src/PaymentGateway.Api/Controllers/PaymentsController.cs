using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, IValidator<PostPaymentRequest> paymentRequestValidator) : Controller
{
    [HttpGet("{id:guid}")]
    [ActionName("GetPayment")]
    public ActionResult<GetPaymentResponse?> GetPaymentAsync(Guid id)
    {
        var payment = paymentService.GetPaymentDetails(id);
        
        return  Ok(payment);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> AddPaymentAsync([FromBody]PostPaymentRequest paymentRequest)
    {
        var validationResult = await paymentRequestValidator.ValidateAsync(paymentRequest);

        if (validationResult.IsValid)
        {
            var paymentResponse = await paymentService.ProcessPayment(paymentRequest);

            return CreatedAtAction("GetPayment", new { id = paymentResponse.Id }, paymentResponse);
        }
        
        return ReturnInvalidRequest(validationResult);
    }

    private ActionResult<PostPaymentResponse?> ReturnInvalidRequest(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .ToArray();

        var response = new PaymentErrorResponse
        {
            Status = Status.Rejected,
            Message = "The request is invalid.",
            Errors = errors
        };

        return BadRequest(response);
    }
}

public class PaymentErrorResponse
{
    public Status Status { get; set; }
    public string Message { get; set; }
    public string[] Errors { get; set; }
}



