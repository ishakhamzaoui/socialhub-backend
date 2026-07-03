using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Defaults;
using SocialHub.Application.Common.Events;
using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Application;
 
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
 
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
 
        // Order matters: first registered = outermost. This mirrors
        // SocialHub-Backend-Specification.md §14 exactly.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
 
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
 
        // Safe no-op defaults. Persistence (Phase 2), Identity (Phase 3), and
        // Infrastructure's Redis cache (Phase 4/17) register their real
        // implementations *after* AddApplication() is called in Program.cs,
        // which takes precedence for constructor injection.
        services.AddScoped<ICurrentUserService, NullCurrentUserService>();
        services.AddScoped<ICacheService, NullCacheService>();
        services.AddScoped<IAuditService, NullAuditService>();
        services.AddScoped<IUnitOfWork, NullUnitOfWork>();
 
        return services;
    }
}