using Serilog;
using SocialHub.API.Extensions;
using SocialHub.API.Middleware;
using SocialHub.Application;
 
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
 
    builder.Services.AddControllers();
 
    builder.Services.AddSocialHubApiVersioning();
    builder.Services.AddSocialHubSwagger();
    builder.Services.AddSocialHubAuthentication(builder.Configuration);
    builder.Services.AddSocialHubHealthChecks(builder.Configuration);
 
    // Phase 1: Core Architecture (Result pattern, CQRS, MediatR pipeline).
    builder.Services.AddApplication();
 
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