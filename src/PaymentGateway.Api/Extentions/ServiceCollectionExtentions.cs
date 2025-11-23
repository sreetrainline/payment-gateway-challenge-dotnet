using System.Net;

using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Services;

using Polly;
using Polly.Retry;

namespace PaymentGateway.Api.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureModelBindingBehaviour(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(pair => pair.Value?.Errors.Count > 0)
                    .SelectMany(pair => pair.Value!.Errors)
                    .Select(error => error.ErrorMessage)
                    .ToArray();

                var response = new PaymentErrorResponse
                {
                    Status = Status.Rejected,
                    Message = "The request is invalid.",
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
    
    public static IServiceCollection AddHttpClientWithRetry(this IServiceCollection services)
    {
        services.AddHttpClient<PaymentProvider>()
            .AddResilienceHandler("default", pipeline =>
            {
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromMilliseconds(100),
                    BackoffType = DelayBackoffType.Exponential,

                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout),

                    OnRetry = args =>
                    {
                        Console.WriteLine(
                            $"Retry {args.AttemptNumber} after {args.RetryDelay} due to {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");
                        return default;
                    }
                });
            });

        return services;
    }
}