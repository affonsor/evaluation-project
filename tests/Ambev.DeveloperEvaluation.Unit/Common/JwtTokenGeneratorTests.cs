using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

/// <summary>Tests for <see cref="JwtTokenGenerator"/>: emits a signed token with the expected claims.</summary>
public class JwtTokenGeneratorTests
{
    private sealed class TestUser : IUser
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Username { get; init; } = "tester";
        public string Role { get; init; } = "Admin";
    }

    private static IConfiguration Config() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "ThisIsASecretKeyForJwtGenerationThatIsAtLeast32BytesLong!!"
            })
            .Build();

    [Fact(DisplayName = "Given a user When generating a token Then it is a JWT carrying the user's claims")]
    public void GenerateToken_ValidUser_ContainsClaims()
    {
        var user = new TestUser();
        var generator = new JwtTokenGenerator(Config());

        var token = generator.GenerateToken(user);

        token.Split('.').Should().HaveCount(3);

        // JwtSecurityTokenHandler serializes with short claim names (nameid/unique_name/role) and
        // ReadJwtToken does not remap them to the ClaimTypes URIs, so assert on the values.
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Select(c => c.Value).Should()
            .Contain(user.Id).And.Contain(user.Username).And.Contain(user.Role);
    }
}
