using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Api.Helpers;

namespace ModularNet.Api;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register API-specific services
        services.AddScoped<ITokenHelper, TokenHelper>();

        return services;
    }
}
