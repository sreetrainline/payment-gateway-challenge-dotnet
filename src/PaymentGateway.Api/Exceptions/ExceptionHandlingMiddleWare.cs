using PaymentGateway.Api.Controllers;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Api.Exceptions;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;;
        context.Response.ContentType = "application/json";

        var paymentErrorResponse = new PaymentErrorResponse
        {
            Status = PaymentStatus.Declined,
            Message = "An Unexpected Error Occured. Please try again",
            Errors = [] // Don't want to leak system internals to client.
        };

        await context.Response.WriteAsJsonAsync(paymentErrorResponse);
    }
}
