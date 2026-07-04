using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Infrastructure.Email;
 
namespace SocialHub.Infrastructure;
 
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
 
        return services;
    }
}