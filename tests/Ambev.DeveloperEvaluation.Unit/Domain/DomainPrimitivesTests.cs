using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>Covers small domain primitives: events, exceptions and <see cref="BaseEntity"/> comparison.</summary>
public class DomainPrimitivesTests
{
    [Fact(DisplayName = "UserRegisteredEvent exposes the user")]
    public void UserRegisteredEvent_HoldsUser()
    {
        var user = UserTestData.GenerateValidUser();

        var evt = new UserRegisteredEvent(user);

        evt.User.Should().BeSameAs(user);
    }

    [Fact(DisplayName = "DomainException keeps message and inner exception")]
    public void DomainException_Constructors()
    {
        var inner = new InvalidOperationException("inner");

        new DomainException("boom").Message.Should().Be("boom");

        var withInner = new DomainException("outer", inner);
        withInner.Message.Should().Be("outer");
        withInner.InnerException.Should().BeSameAs(inner);
    }

    [Fact(DisplayName = "BaseEntity.CompareTo orders by id and treats null as smaller")]
    public void BaseEntity_CompareTo()
    {
        var a = new BaseEntity { Id = Guid.NewGuid() };
        var b = new BaseEntity { Id = a.Id };

        a.CompareTo(null).Should().Be(1);
        b.CompareTo(a).Should().Be(0);
    }
}
