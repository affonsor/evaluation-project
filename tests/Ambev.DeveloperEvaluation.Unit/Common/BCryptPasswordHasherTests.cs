using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

/// <summary>Tests for <see cref="BCryptPasswordHasher"/> hash/verify round-trip.</summary>
public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact(DisplayName = "Given a password When hashed Then verify succeeds for the same password")]
    public void HashThenVerify_CorrectPassword_ReturnsTrue()
    {
        const string password = "Sup3r$ecret";

        var hash = _hasher.HashPassword(password);

        hash.Should().NotBeNullOrWhiteSpace().And.NotBe(password);
        _hasher.VerifyPassword(password, hash).Should().BeTrue();
    }

    [Fact(DisplayName = "Given a wrong password When verified Then returns false")]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.HashPassword("original");

        _hasher.VerifyPassword("different", hash).Should().BeFalse();
    }
}
