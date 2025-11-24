using FluentAssertions;

using FluentValidation.Results;
using PaymentGateway.Api.Validators;
using PaymentGateway.Domain.Models.Requests;

namespace PaymentGateWay.Api.Unit.Tests;

public class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _validator = new();

    private PostPaymentRequest CreateValidRequest()
    {
        var now = DateTime.UtcNow;

        return new PostPaymentRequest
        {
            CardNumber = "41111111111111", 
            ExpiryMonth = 12,
            ExpiryYear = now.Year + 1,
            Currency = "GBP",
            Amount = 100,
            Cvv = 123
        };
    }

    [Fact]
    public void ValidRequestPassesValidation()
    {
        var request = CreateValidRequest();
        
        ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyCardNumberShouldFail()
    {
        var request = CreateValidRequest();
        request.CardNumber = string.Empty;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number is required."
        );
    }

    [Fact]
    public void ShortCardNumberShouldFail()
    {
        var request = CreateValidRequest();
        request.CardNumber = "1234567890123"; 

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number must be between 14 and 19 characters long."
        );
    }

    [Fact]
    public void NonNumericCardNumberShouldFail()
    {
        var request = CreateValidRequest();
        request.CardNumber = "1234abcd567890";

        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number must contain only numeric characters."
        );
    }
    
    [Fact]
    public void OutOfRangeMonthShouldFail()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 13;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.ExpiryMonth) &&
            e.ErrorMessage == "Expiry month must be between 1 and 12."
        );
        
    }


    [Fact]
    public void PastExpiryDateShouldFail()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = 2000; 

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.ErrorMessage == "Expiry month and year must be in the future."
        );
    }

    [Fact]
    public void FutureExpiryDateShouldPass()
    {
        var now = DateTime.UtcNow;
        var request = CreateValidRequest();
        request.ExpiryMonth = 12;
        request.ExpiryYear = now.Year + 2; 

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
    

    [Fact]
    public void EmptyCurrencyShouldFail()
    {
        var request = CreateValidRequest();
        request.Currency = string.Empty;

        var result = _validator.Validate(request);

        
        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency is required."
        );
    }

    [Fact]
    public void InvalidCurrencyShouldFail()
    {
        var request = CreateValidRequest();
        request.Currency = "GB"; 

        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency must be a 3-letter ISO code."
        );
    }

    [Fact]
    public void UnsupportedCurrencyShouldFail()
    {
        var request = CreateValidRequest();
        request.Currency = "INR";

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency is not supported."
        );
    }

    [Fact]
    public void LowerCaseCurrencyShouldPass()
    {
        var request = CreateValidRequest();
        request.Currency = "gbp"; 

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ZeroAmountShouldFail()
    {
        var request = CreateValidRequest();
        request.Amount = 0;

        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Amount) &&
            e.ErrorMessage == "Amount must be greater than zero."
        );
    }

    [Fact]
    public void LongMaxValueFails()
    {
        var request = CreateValidRequest();
        request.Amount = long.MaxValue;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Amount) &&
            e.ErrorMessage == $"Value should be less than{long.MaxValue}"
        );
    }

    [Fact]
    public void AmountBelowLongMaxShouldPass()
    {
        var request = CreateValidRequest();
        request.Amount = long.MaxValue - 1;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CvvLessThanThreeShouldFail()
    {
        var request = CreateValidRequest();
        request.Cvv = 99;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Cvv) &&
            e.ErrorMessage == "CVV must be 3 or 4 digits long."
        );
    }

    [Fact]
    public void CvvMoreThanFourShouldFail()
    {
        var request = CreateValidRequest();
        request.Cvv = 10000;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(PostPaymentRequest.Cvv) &&
            e.ErrorMessage == "CVV must be 3 or 4 digits long."
        );
    }

    [Theory]
    [InlineData(123)]
    [InlineData(1234)]
    public void ValidCvvShouldPass(int requestCvv)
    {
        var request = CreateValidRequest();
        request.Cvv = requestCvv;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
