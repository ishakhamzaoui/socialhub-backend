using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SocialHub.Application.Common.Events;
using SocialHub.Persistence.Context;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
/// <summary>
/// Points at a dedicated "socialhub_test" PostgreSQL database (never
/// socialhub_dev). Override via the SOCIALHUB_TEST_DB_CONNECTION environment
/// variable if your setup differs from the one 08-phase2-persistence.sh
/// created.
/// </summary>
public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=socialhub_test;Username=socialhub;Password=changeme_dev";
 
    public ApplicationDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("SOCIALHUB_TEST_DB_CONNECTION")
            ?? DefaultConnectionString;
 
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;
 
        // Domain event dispatch is irrelevant to repository CRUD correctness
        // and pulls in the full MediatR pipeline; a no-op substitute keeps
        // these tests focused on persistence behavior.
        return new ApplicationDbContext(options, Substitute.For<IDomainEventDispatcher>());
    }
 
    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
 
    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
 
[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection : ICollectionFixture<PostgresDatabaseFixture>
{
}