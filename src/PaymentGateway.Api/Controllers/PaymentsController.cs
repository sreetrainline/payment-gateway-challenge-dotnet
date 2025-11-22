using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentService.GetPaymentDetails(id);

        return new OkObjectResult(payment);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> AddPaymentAsync([FromBody]PostPaymentRequest paymentRequest)
    {
        var paymentResponse = paymentService.ProcessPayment(paymentRequest);

        return new OkObjectResult(paymentResponse); // Change to Created and handle non success
    }
}



