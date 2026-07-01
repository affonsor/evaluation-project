using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>End-to-end tests for the Sales endpoints against the real API + PostgreSQL.</summary>
[Collection(SalesApiCollection.Name)]
public class SalesEndpointsTests
{
    private readonly HttpClient _client;

    public SalesEndpointsTests(SalesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object BuildSale(string? customerName = null, int quantity = 5) => new
    {
        saleNumber = $"S-{Guid.NewGuid():N}".Substring(0, 12),
        saleDate = DateTime.UtcNow,
        customerId = Guid.NewGuid(),
        customerName = customerName ?? "Ada Lovelace",
        branchId = Guid.NewGuid(),
        branchName = "Downtown",
        items = new[]
        {
            new { productId = Guid.NewGuid(), productName = "Widget", quantity, unitPrice = 100m }
        }
    };

    private static async Task<JsonElement> DataAsync(HttpResponseMessage response)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.GetProperty("data").Clone();
    }

    [Fact(DisplayName = "Full sale lifecycle: create, get, update, cancel item, cancel sale, delete")]
    public async Task Sale_Lifecycle()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/sales", BuildSale());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await DataAsync(createResponse);
        var saleId = created.GetProperty("id").GetGuid();
        var itemId = created.GetProperty("items")[0].GetProperty("id").GetGuid();
        created.GetProperty("items")[0].GetProperty("discount").GetDecimal().Should().Be(50m); // 5 * 100 * 10%

        // Get
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{saleId}", BuildSale(quantity: 3));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cancel item (re-fetch a current item id after the update replaced the item set)
        var afterUpdate = await DataAsync(await _client.GetAsync($"/api/sales/{saleId}"));
        var currentItemId = afterUpdate.GetProperty("items")[0].GetProperty("id").GetGuid();
        var cancelItemResponse = await _client.PostAsync($"/api/sales/{saleId}/items/{currentItemId}/cancel", null);
        cancelItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cancel sale
        var cancelResponse = await _client.PostAsync($"/api/sales/{saleId}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await DataAsync(cancelResponse)).GetProperty("isCancelled").GetBoolean().Should().BeTrue();

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/sales/{saleId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Gone
        (await _client.GetAsync($"/api/sales/{saleId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "List honours pagination and the case-insensitive wildcard (ILIKE) filter")]
    public async Task List_WithWildcardFilter()
    {
        var unique = $"Zoe-{Guid.NewGuid():N}".Substring(0, 12);
        await _client.PostAsJsonAsync("/api/sales", BuildSale(customerName: unique));

        // lower-case wildcard must still match (ILIKE)
        var response = await _client.GetAsync($"/api/sales?_page=1&_size=10&customerName={unique.ToLowerInvariant()}*");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;
        root.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        root.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact(DisplayName = "Creating a sale with more than 20 identical items returns 400")]
    public async Task Create_RuleViolation_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", BuildSale(quantity: 21));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        doc!.RootElement.TryGetProperty("detail", out _).Should().BeTrue();
    }

    [Fact(DisplayName = "Getting an unknown sale returns 404")]
    public async Task Get_Unknown_Returns404()
    {
        var response = await _client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
