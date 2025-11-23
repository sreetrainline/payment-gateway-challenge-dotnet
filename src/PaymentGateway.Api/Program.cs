using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Extentions;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

using Polly;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository,PaymentsRepository>();
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddTransient<IPaymentProvider,PaymentProvider>();

builder.Services.AddHttpClientWithRetry();
builder.Services.ConfigureModelBindingBehaviour();


builder.Services.Configure<BankConfig>(
    builder.Configuration.GetSection("BankConfig"));

builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// void SetupModelBindingBehaviour(WebApplicationBuilder webApplicationBuilder)
// {
//     webApplicationBuilder.Services.Configure<ApiBehaviorOptions>(options =>
//     {
//         options.InvalidModelStateResponseFactory = context =>
//         {
//             var errors = context.ModelState
//                 .Where(pair => pair.Value?.Errors.Count > 0)
//                 .SelectMany(pair => pair.Value!.Errors)
//                 .Select(error => error.ErrorMessage)
//                 .ToArray();
//
//             var response = new PaymentErrorResponse
//             {
//                 Status = Status.Rejected,
//                 Message = "The request is invalid.",
//                 Errors = errors
//             };
//
//             return new BadRequestObjectResult(response);
//         };
//     });
// }

// void SetupHttpClientWithRetry(WebApplicationBuilder builder1)
// {
//     builder1.Services.AddHttpClient<PaymentProvider>()
//         .AddResilienceHandler("default", pipeline =>
//         {
//             pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
//             {
//                 MaxRetryAttempts = 5,
//                 Delay = TimeSpan.FromMilliseconds(100),
//                 BackoffType = DelayBackoffType.Exponential,
//             
//                 ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
//                     .Handle<HttpRequestException>()
//                     .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout),
//             
//                 OnRetry = args =>
//                 {
//                     Console.WriteLine(
//                         $"Retry {args.AttemptNumber} after {args.RetryDelay} due to {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");
//                     return default;
//                 }
//             });
//         });
// }

