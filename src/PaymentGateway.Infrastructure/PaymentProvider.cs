using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Services;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ExternalModels;
using PaymentGateway.Infrastructure.ExternalModels;

namespace PaymentGateway.Infrastructure;

public class PaymentProvider(IHttpClientFactory httpClientFactory, IOptions<BankConfig> bankConfig)
    : IPaymentProvider
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(nameof(PaymentProvider));
    private readonly BankConfig _bankConfig = bankConfig.Value;

    public async Task<BankResponse> SubmitPayment(BankPaymentRequest request)
    {
        try
        {
            var response =   await _httpClient.PostAsJsonAsync(_bankConfig.BaseUrl, request);

            if (!response.IsSuccessStatusCode)
            {
                throw new PaymentSubmissionException($"Payment Submission failed with status {response.StatusCode} ");
            }

            var bankResponse =  await response.Content.ReadFromJsonAsync<BankResponse>();

            if (bankResponse == null)
            {
                throw new PaymentSubmissionException("Payment Submission came back with an empty response");
            }

            return bankResponse;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw new UnknownPaymentException("Unexpected Error in Payments",exception);
        }
    }
}