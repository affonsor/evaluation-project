using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="GetUserHandler"/>: validation, not-found and mapping.</summary>
public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given empty id When getting user Then throws validation exception")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given unknown user When getting Then throws KeyNotFoundException")]
    public async Task Handle_UserNotFound_ThrowsKeyNotFound()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(new GetUserCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing user When getting Then returns mapped result")]
    public async Task Handle_ExistingUser_ReturnsMappedResult()
    {
        var user = UserTestData.GenerateValidUser();
        var expected = new GetUserResult { Id = Guid.NewGuid() };
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(expected);

        var result = await _handler.Handle(new GetUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeSameAs(expected);
    }
}
