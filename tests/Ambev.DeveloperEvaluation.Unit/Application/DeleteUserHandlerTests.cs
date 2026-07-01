using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="DeleteUserHandler"/>: validation, not-found and success.</summary>
public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _handler = new DeleteUserHandler(_userRepository);
    }

    [Fact(DisplayName = "Given empty id When deleting user Then throws validation exception")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new DeleteUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given unknown user When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NotDeleted_ThrowsKeyNotFound()
    {
        _userRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing user When deleting Then returns success")]
    public async Task Handle_Deleted_ReturnsSuccess()
    {
        _userRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.Success.Should().BeTrue();
    }
}
