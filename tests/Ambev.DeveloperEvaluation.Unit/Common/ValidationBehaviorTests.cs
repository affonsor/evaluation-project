using Ambev.DeveloperEvaluation.Common.Validation;
using FluentValidation;
using MediatR;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

/// <summary>Tests for the MediatR <see cref="ValidationBehavior{TRequest,TResponse}"/> pipeline step.</summary>
public class ValidationBehaviorTests
{
    public record Ping(string Message) : IRequest<string>;

    private static RequestHandlerDelegate<string> Next(string value = "pong") => () => Task.FromResult(value);

    [Fact(DisplayName = "Given no validators When handling Then passes through to the next handler")]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<Ping, string>(Array.Empty<IValidator<Ping>>());

        var result = await behavior.Handle(new Ping("hi"), Next(), CancellationToken.None);

        result.Should().Be("pong");
    }

    [Fact(DisplayName = "Given a passing validator When handling Then passes through")]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = new InlineValidator<Ping>();
        validator.RuleFor(p => p.Message).NotEmpty();
        var behavior = new ValidationBehavior<Ping, string>(new[] { validator });

        var result = await behavior.Handle(new Ping("hi"), Next(), CancellationToken.None);

        result.Should().Be("pong");
    }

    [Fact(DisplayName = "Given a failing validator When handling Then throws ValidationException")]
    public async Task Handle_InvalidRequest_Throws()
    {
        var validator = new InlineValidator<Ping>();
        validator.RuleFor(p => p.Message).NotEmpty();
        var behavior = new ValidationBehavior<Ping, string>(new[] { validator });

        var act = () => behavior.Handle(new Ping(string.Empty), Next(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
