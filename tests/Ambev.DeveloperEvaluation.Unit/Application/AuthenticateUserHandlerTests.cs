using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="AuthenticateUserHandler"/>: success, bad credentials and inactive user.</summary>
public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    private static AuthenticateUserCommand Command() =>
        new() { Email = "user@test.com", Password = "Passw0rd!" };

    [Fact(DisplayName = "Given valid active credentials When authenticating Then returns token and user data")]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("jwt-token");

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Username);
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact(DisplayName = "Given unknown user When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given wrong password When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => _handler.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given inactive user When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_InactiveUser_ThrowsUnauthorized()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var act = () => _handler.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
