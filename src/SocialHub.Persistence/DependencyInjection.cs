using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Persistence.Common;
using SocialHub.Persistence.Context;
using SocialHub.Persistence.Repositories;
 
namespace SocialHub.Persistence;
 
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
 
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
 
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
 
        // Generic repository + Unit of Work now have real EF-backed
        // implementations, overriding Phase 1's Null* defaults because
        // AddPersistence() is called after AddApplication() in Program.cs.
        services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
 
        // Feature-specific repositories.
        services.AddScoped<IHashtagRepository, HashtagRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
 
        // Phase 5 — User Management.
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IUserBlockRepository, UserBlockRepository>();
        services.AddScoped<IUserMuteRepository, UserMuteRepository>();

        // Phase 6 — Posts.
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostRepostRepository, PostRepostRepository>();
        
        return services;
    }
}