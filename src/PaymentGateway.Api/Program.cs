using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Extentions;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository,PaymentsRepository>();
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddScoped<IPaymentProvider,PaymentProvider>();
builder.Services.AddSingleton<IPaymentMapper, PaymentMapper>();

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

public partial class Program
{
    
}
