using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.IntegrationTests.Tests;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

// Register Services
builder.Services
    .AddScoped<IAppSettingsManager, AppSettingsManager>()
    .AddScoped<IAuditsManager, AuditsManager>()
    .AddScoped<IAuditsRepository, AuditsRepository>()
    .AddScoped<IAuthManager, AuthManager>()
    .AddScoped<IAuthenticationService, AuthenticationService>()
    .AddScoped<ICacheManager, CacheManager>()
    .AddScoped<ICacheRepository, CacheRepository>()
    .AddScoped<IDbConnectionFactory, DbConnectionFactory>()
    .AddScoped<IEmailServiceManager, EmailServiceManager>()
    .AddScoped<IEmailServiceRepository, EmailServiceRepository>()
    .AddScoped<IEmailVerifierManager, EmailVerifierManager>()
    .AddScoped<IEncryptManager, EncryptManager>()
    .AddScoped<IHealthChecksManager, HealthChecksManager>()
    .AddScoped<IHealthChecksRepository, HealthChecksRepository>()
    .AddSingleton<IInMemoryCacheRepository, InMemoryCacheRepository>()
    .AddScoped<ILogsManager, LogsManager>()
    .AddScoped<ILogsRepository, LogsRepository>()
    .AddScoped<ISecretsManager, SecretsManager>()
    .AddScoped<ISecretsRepository, SecretsRepository>()
    .AddScoped<IUsersManager, UsersManager>()
    .AddScoped<IUsersRepository, UsersRepository>()
    .AddScoped<IUsersSettingsManager, UsersSettingsManager>()
    .AddScoped<IUsersSettingsRepository, UsersSettingsRepository>()
    .AddScoped<IWebAppConfigsManager, WebAppConfigsManager>()
    .AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>()
    .AddScoped<IRedisRepository, RedisRepository>()
    .AddSingleton(configuration)
    .AddLogging();

FirebaseApp.Create(
    new AppOptions
    {
        Credential = GoogleCredential.FromFile("modularNet-firebase-adminsdk-dev.json")
    }
);

builder.Services.AddHttpClient<IJwtProvider, JwtProvider>((sp, httpClient) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    httpClient.BaseAddress = new Uri(configuration["AppSettings:FirebaseConfig:TokenUri"]);
});

var serviceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();

var configurationTests = new ConfigurationTests(
    serviceProvider.GetService<IConfiguration>() ?? throw new Exception("Error Getting Service"));

var emailServiceTests = new EmailServiceTests(
    serviceProvider.GetService<IEmailServiceManager>() ?? throw new Exception("Error Getting Service"));

var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
if (appSettings == null) throw new Exception("Error Getting AppSettings");

var encryptManagerTests = new EncryptManagerTests(
    serviceProvider.GetService<IEncryptManager>() ?? throw new Exception("Error Getting Service"));

var appSettingsManagerTests = new AppSettingsManagerTests(
    serviceProvider.GetService<IAppSettingsManager>() ?? throw new Exception("Error Getting Service"),
    serviceProvider.GetService<ISecretsManager>() ?? throw new Exception("Error Getting Service"));

var cacheManagerTests =
    new CacheManagerTests(serviceProvider.GetService<ICacheManager>() ?? throw new Exception("Error Getting Service"));


configurationTests.RunAllTests();
await emailServiceTests.RunAllTests();
await encryptManagerTests.RunAllTests();
await appSettingsManagerTests.RunAllTests();
await cacheManagerTests.RunAllTests();