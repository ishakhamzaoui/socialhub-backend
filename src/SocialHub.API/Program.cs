using Microsoft.EntityFrameworkCore;
using Serilog;
using SocialHub.API.Extensions;
using SocialHub.API.Middleware;
using SocialHub.Application;
using SocialHub.Persistence;
using SocialHub.Persistence.Context;
using SocialHub.Persistence.Seed;
 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/socialhub-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateBootstrapLogger();
 
try
{
    Log.Information("Starting SocialHub API");
 
    var builder = WebApplication.CreateBuilder(args);
 
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/socialhub-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14));

    builder.Services.AddSerilogRequestLogging();
 
    builder.Services.AddControllers();
 
    builder.Services.AddSocialHubApiVersioning();
    builder.Services.AddSocialHubSwagger();
    builder.Services.AddSocialHubAuthentication(builder.Configuration);
    builder.Services.AddSocialHubHealthChecks(builder.Configuration);
 
    // Phase 1: Core Architecture (Result pattern, CQRS, MediatR pipeline).
    builder.Services.AddApplication();

    // Phase 2: Database & Persistence. Registered after AddApplication() so
    // its real IUnitOfWork/IRepository<,> implementations take precedence
    // over Phase 1's Null* defaults.
    builder.Services.AddPersistence(builder.Configuration);
 
    // Phase 1.9: global exception handling -> RFC 7807 ProblemDetails.
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
 
    var app = builder.Build();
 
    app.UseExceptionHandler();
 
    app.UseSerilogRequestLogging();
 
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SocialHub API v1");
        });

        // Dev-only convenience seeding (roadmap step 2.6).
        // Production data is never seeded automatically.
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await ApplicationDbContextSeeder.SeedAsync(db);
    }
 
    app.UseHttpsRedirection();
 
    app.UseAuthentication();
    app.UseAuthorization();
 
    app.MapControllers();
    app.MapSocialHubHealthChecks();
 
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SocialHub API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}