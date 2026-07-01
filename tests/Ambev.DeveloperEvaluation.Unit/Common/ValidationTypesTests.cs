using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

/// <summary>Covers <see cref="ValidationResultDetail"/> and the broken-by-template <see cref="Validator"/> helper.</summary>
public class ValidationTypesTests
{
    [Fact(DisplayName = "Given a failed FluentValidation result When wrapped Then maps IsValid and errors")]
    public void ValidationResultDetail_FromFailures_MapsErrors()
    {
        var source = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Email", "Email is invalid")
        });

        var detail = new ValidationResultDetail(source);

        detail.IsValid.Should().BeFalse();
        detail.Errors.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Given the default constructor When created Then it is an empty valid detail")]
    public void ValidationResultDetail_Default_IsEmpty()
    {
        var detail = new ValidationResultDetail();

        detail.IsValid.Should().BeFalse();
        detail.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Validator.ValidateAsync cannot instantiate the interface and throws")]
    public async Task Validator_ValidateAsync_Throws()
    {
        var act = () => Validator.ValidateAsync(new object());

        await act.Should().ThrowAsync<Exception>();
    }
}
