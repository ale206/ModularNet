using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;

namespace ModularNet.Business;

public static class BusinessServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration parameter is included for consistency and future extensibility
        // (e.g., for conditional registration based on configuration values)
        
        // Register Business managers
        services.AddScoped<IAppSettingsManager, AppSettingsManager>();
        services.AddScoped<IAuditsManager, AuditsManager>();
        services.AddScoped<IAuthManager, AuthManager>();
        services.AddScoped<ICacheManager, CacheManager>();
        services.AddScoped<IEmailServiceManager, EmailServiceManager>();
        services.AddScoped<IEmailVerifierManager, EmailVerifierManager>();
        services.AddScoped<IEncryptManager, EncryptManager>();
        services.AddScoped<IHealthChecksManager, HealthChecksManager>();
        services.AddScoped<ILogsManager, LogsManager>();
        services.AddScoped<ISecretsManager, SecretsManager>();
        services.AddScoped<IUsersManager, UsersManager>();
        services.AddScoped<IUsersSettingsManager, UsersSettingsManager>();
        services.AddScoped<IWebAppConfigsManager, WebAppConfigsManager>();

        return services;
    }
}
