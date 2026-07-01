using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>End-to-end tests for the Users and Auth endpoints (and the /health check).</summary>
[Collection(SalesApiCollection.Name)]
public class AuthAndUsersEndpointsTests
{
    private readonly HttpClient _client;

    public AuthAndUsersEndpointsTests(SalesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object NewUser(string email) => new
    {
        username = $"user{Guid.NewGuid():N}".Substring(0, 12),
        password = "Passw0rd!",
        email,
        phone = "+5511999999999",
        status = 1, // Active
        role = 1    // Customer
    };

    [Fact(DisplayName = "Health endpoint returns 200")]
    public async Task Health_Returns200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "User can be created, fetched and deleted")]
    public async Task User_CreateGetDelete()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var createResponse = await _client.PostAsJsonAsync("/api/users", NewUser(email));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
        var id = created!.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync($"/api/users/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Login returns a token for valid credentials and 401 for a wrong password")]
    public async Task Login_ValidAndInvalid()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/users", NewUser(email));

        var ok = await _client.PostAsJsonAsync("/api/auth", new { email, password = "Passw0rd!" });
        ok.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ok.Content.ReadFromJsonAsync<JsonDocument>();
        body!.RootElement.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();

        var wrong = await _client.PostAsJsonAsync("/api/auth", new { email, password = "nope" });
        wrong.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
