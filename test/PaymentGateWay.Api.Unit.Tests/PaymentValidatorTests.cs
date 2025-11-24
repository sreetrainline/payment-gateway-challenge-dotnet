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
    public void Valid_request_should_pass_validation()
    {
        var request = CreateValidRequest();
        
        ValidationResult result = _validator.Validate(request);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CardNumber_empty_should_fail()
    {
        var request = CreateValidRequest();
        request.CardNumber = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number is required.");
    }

    [Fact]
    public void CardNumber_too_short_should_fail()
    {
        var request = CreateValidRequest();
        request.CardNumber = "1234567890123"; 

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number must be between 14 and 19 characters long.");
    }

    [Fact]
    public void CardNumber_with_non_numeric_chars_should_fail()
    {
        var request = CreateValidRequest();
        request.CardNumber = "1234abcd567890";

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage == "Card number must contain only numeric characters.");
    }
    
    [Fact]
    public void ExpiryMonth_out_of_range_should_fail()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 13;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.ExpiryMonth) &&
            e.ErrorMessage == "Expiry month must be between 1 and 12.");
    }

    [Fact]
    public void ExpiryYear_less_than_one_should_fail()
    {
        var request = CreateValidRequest();
        request.ExpiryYear = 0;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.ExpiryYear) &&
            e.ErrorMessage == "Expiry year must be a valid year.");
    }
    

    [Fact]
    public void Expiry_in_the_past_should_fail_future_check()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = 2000; // safely in the past

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage == "Expiry month and year must be in the future.");
    }

    [Fact]
    public void Expiry_in_the_future_should_pass_future_check()
    {
        var now = DateTime.UtcNow;
        var request = CreateValidRequest();
        request.ExpiryMonth = 12;
        request.ExpiryYear = now.Year + 2; 

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
    

    [Fact]
    public void Currency_empty_should_fail()
    {
        var request = CreateValidRequest();
        request.Currency = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency is required.");
    }

    [Fact]
    public void Currency_not_three_letters_should_fail()
    {
        var request = CreateValidRequest();
        request.Currency = "GB"; // 2 letters

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency must be a 3-letter ISO code.");
    }

    [Fact]
    public void Unsupported_currency_should_fail()
    {
        var request = CreateValidRequest();
        request.Currency = "INR";

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Currency) &&
            e.ErrorMessage == "Currency is not supported.");
    }

    [Fact]
    public void Supported_currency_case_insensitive_should_pass()
    {
        var request = CreateValidRequest();
        request.Currency = "gbp"; 

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }


    [Fact]
    public void Amount_less_than_or_equal_zero_should_fail()
    {
        var request = CreateValidRequest();
        request.Amount = 0;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Amount) &&
            e.ErrorMessage == "Amount must be greater than zero.");
    }

    [Fact]
    public void Amount_equal_to_long_max_value_should_fail()
    {
        var request = CreateValidRequest();
        request.Amount = long.MaxValue;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Amount) &&
            e.ErrorMessage == $"Value should be less than{long.MaxValue}");
    }

    [Fact]
    public void Amount_just_below_long_max_should_pass()
    {
        var request = CreateValidRequest();
        request.Amount = long.MaxValue - 1;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Cvv_less_than_three_digits_should_fail()
    {
        var request = CreateValidRequest();
        request.Cvv = 99;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Cvv) &&
            e.ErrorMessage == "CVV must be 3 or 4 digits long.");
    }

    [Fact]
    public void Cvv_more_than_four_digits_should_fail()
    {
        var request = CreateValidRequest();
        request.Cvv = 10000;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.Cvv) &&
            e.ErrorMessage == "CVV must be 3 or 4 digits long.");
    }

    [Fact]
    public void Cvv_three_digits_or_four_digits_should_pass()
    {
        var request3 = CreateValidRequest();
        request3.Cvv = 123;

        var result3 = _validator.Validate(request3);
        Assert.True(result3.IsValid);

        var request4 = CreateValidRequest();
        request4.Cvv = 1234;

        var result4 = _validator.Validate(request4);
        Assert.True(result4.IsValid);
    }
}
