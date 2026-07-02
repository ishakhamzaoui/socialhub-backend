namespace SocialHub.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddSocialHubHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();
 
        var postgres = configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrWhiteSpace(postgres))
        {
            builder.AddNpgSql(postgres, name: "postgres", tags: new[] { "ready" });
        }
 
        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            builder.AddRedis(redis, name: "redis", tags: new[] { "ready" });
        }
 
        return services;
    }
 
    public static WebApplication MapSocialHubHealthChecks(this WebApplication app)
    {
        // Liveness: process is up. No dependency checks.
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });
 
        // Readiness: only checks tagged "ready" (DB, Redis).
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
 
        // General/aggregate status.
        app.MapHealthChecks("/health");
 
        return app;
    }
}
