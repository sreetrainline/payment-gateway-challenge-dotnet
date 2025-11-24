using FluentValidation;

using PaymentGateway.Domain.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly HashSet<string> AllowedCurrencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "GBP",
            "USD",
            "EUR" // up to 3 currencies as per your rule
        };

    public PaymentRequestValidator()
    {
        // Card Number
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters long.")
            .Matches("^[0-9]+$").WithMessage("Card number must contain only numeric characters.");

        // Expiry Month (1–12)
        RuleFor(x => x.ExpiryMonth)
            .NotNull().WithMessage("Expiry month is required.")
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

        // Expiry Year (basic sanity; detailed future-check is in BeInTheFuture)
        RuleFor(x => x.ExpiryYear)
            .NotNull().WithMessage("Expiry year is required.")
            .GreaterThan(0).WithMessage("Expiry year must be a valid year.");

        // Combined Expiry Validation (month + year in the future)
        RuleFor(x => x)
            .Must(BeInTheFuture)
            .WithMessage("Expiry month and year must be in the future.");

        // Currency
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .Must(c => AllowedCurrencies.Contains(c))
            .WithMessage("Currency is not supported.");

        // Amount (minor currency units, integer > 0)
        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .LessThan(long.MaxValue).WithMessage($"Value should be less than{long.MaxValue}");

        // CVV (3–4 digits; note: int cannot preserve leading zeros)
        RuleFor(x => x.Cvv)
            .NotNull().WithMessage("CVV is required.")
            .InclusiveBetween(100, 9999).WithMessage("CVV must be 3 or 4 digits long.");
    }

    private bool BeInTheFuture(PostPaymentRequest req)
    {
        // Guard invalid ranges to avoid DateTime exceptions
        if (req.ExpiryMonth < 1 || req.ExpiryMonth > 12) return false;
        if (req.ExpiryYear < 1 || req.ExpiryYear > 9999) return false;

        var lastMomentOfMonth = new DateTime(req.ExpiryYear, req.ExpiryMonth, 1)
            .AddMonths(1)
            .AddTicks(-1);

        return lastMomentOfMonth >= DateTime.UtcNow;
    }
}