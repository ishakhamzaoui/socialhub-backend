using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SocialHub.API.Extensions;
using SocialHub.API.Middleware;
using SocialHub.Application;
using SocialHub.Identity;
using SocialHub.Identity.Models;
using SocialHub.Infrastructure;
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
 
    // builder.Services.AddSerilogRequestLogging(); // 'IServiceCollection' does not contain a definition for 'AddSerilogRequestLogging'
 
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
 
    // Phase 3: Identity & Authentication. AddIdentityInfrastructure (Identity
    // project) registers Identity core services but stops short of
    // .AddEntityFrameworkStores<>() to avoid a circular project reference —
    // Persistence already references Identity for ApplicationUser/
    // ApplicationRole, so Identity cannot reference Persistence back. The
    // composition root chains the store wiring here instead.
    builder.Services.AddIdentityInfrastructure(builder.Configuration)
        .AddEntityFrameworkStores<ApplicationDbContext>();
 
    // JWT issuance (ITokenService), the real ICurrentUserService,
    // IIdentityService, and IAppUrlProvider.
    builder.Services.AddIdentityAuthServices(builder.Configuration);
 
    // Generic SMTP relay email sender + Redis connection multiplexer.
    builder.Services.AddInfrastructure(builder.Configuration);
 
    // Roadmap 3.13: Redis-backed rate limiting on auth endpoints.
    builder.Services.AddSocialHubRateLimiting(builder.Configuration);
 
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
 
        // Dev-only convenience seeding (roadmap step 2.6, extended in Phase 3
        // to also seed roles + a dev admin). Production data is never seeded
        // automatically.
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await ApplicationDbContextSeeder.SeedAsync(db, roleManager, userManager);
    }
 
    app.UseHttpsRedirection();
 
    app.UseRateLimiter();
 
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