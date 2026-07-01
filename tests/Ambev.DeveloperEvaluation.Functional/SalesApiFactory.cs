using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Boots the real Web API in-memory (TestServer) against a disposable PostgreSQL container, so the
/// functional tests exercise the full stack: controllers, middleware, MediatR, EF Core repositories
/// and the PostgreSQL-specific queries (e.g. the ILIKE wildcard filter used by ListSales).
/// </summary>
public class SalesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("developer_evaluation")
        .WithUsername("developer")
        .WithPassword("ev@luAt10n")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.GetConnectionString()
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>Shares a single booted app + database container across all functional test classes.</summary>
[CollectionDefinition(Name)]
public class SalesApiCollection : ICollectionFixture<SalesApiFactory>
{
    public const string Name = "sales-api";
}
