using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
 
namespace SocialHub.Persistence;
 
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // ApplicationDbContext + Npgsql registration land in Phase 2
        // (Database & Persistence). Once IApplicationDbContext has a
        // concrete implementation, uncomment:
        //
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        // services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        // services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
        // services.AddScoped<IUnitOfWork, UnitOfWork>();
 
        return services;
    }
}