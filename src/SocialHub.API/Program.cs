using Serilog;
using SocialHub.API.Extensions;

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
 
    var app = builder.Build();
 
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
