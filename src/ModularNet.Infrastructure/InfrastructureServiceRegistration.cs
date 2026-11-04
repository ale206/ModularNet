using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Infrastructure repositories and services
        services.AddSingleton<IAppSettingsRepository, AppSettingsRepository>();
        services.AddScoped<IAuditsRepository, AuditsRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IEmailServiceRepository, EmailServiceRepository>();
        services.AddScoped<IHealthChecksRepository, HealthChecksRepository>();
        services.AddSingleton<IInMemoryCacheRepository, InMemoryCacheRepository>();
        services.AddScoped<ILogsRepository, LogsRepository>();
        services.AddSingleton<ISecretsRepository, SecretsRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUsersSettingsRepository, UsersSettingsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
        services.AddScoped<IRedisRepository, RedisRepository>();

        // A shared contract can expose one or more backends without registration-order selection.
        services.AddKeyedScoped<ICacheRepository, InMemoryCacheAdapter>(CacheType.InMemory);
        services.AddKeyedScoped<ICacheRepository, RedisCacheAdapter>(CacheType.Redis);

        // HttpClient Services with Firebase
        services.AddHttpClient<IJwtProvider, JwtProvider>((sp, httpClient) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            httpClient.BaseAddress = new Uri(config["AppSettings:FirebaseConfig:TokenUri"]);
        });

        return services;
    }
}
