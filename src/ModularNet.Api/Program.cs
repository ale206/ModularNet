using System.Globalization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using ModularNet.Api;
using ModularNet.Api.Middlewares;
using ModularNet.Business;
using ModularNet.Infrastructure;
using Serilog;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console()              
        // Additional configurations and sinks
        .CreateBootstrapLogger();

    var builder = WebApplication.CreateBuilder(args);

    ConfigureLogging(builder);
    ConfigureAppSettings(builder);
    await ConfigureServices(builder, builder.Environment);

    var app = builder.Build();

    ConfigureMiddleware(app);

    app.MapControllers();

    Log.Information("Starting the application");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, $"ERROR Starting the Application: {ex.Message}");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

void ConfigureLogging(WebApplicationBuilder builder)
{
    var loggingConfiguration = builder.Configuration.GetSection("AppSettings");

    var minimumLevel = loggingConfiguration["AzureLogAnalyticsConfig:MinimumLevel"];

    var logEventLevel = string.IsNullOrWhiteSpace(minimumLevel)
        ? LogEventLevel.Information
        : (LogEventLevel)Enum.Parse(typeof(LogEventLevel), minimumLevel);

    builder.Host.UseSerilog((hostContext, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(builder.Configuration)
            .MinimumLevel.Is(logEventLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .WriteTo.Console();
    });

    builder.Logging.ClearProviders().AddConsole();
}

void ConfigureAppSettings(WebApplicationBuilder builder)
{
    var defaultCulture = new CultureInfo("en-GB");
    CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
    CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
}

async Task ConfigureServices(WebApplicationBuilder builder, IWebHostEnvironment env)
{
    // This section can be refactored further as it grows.
    builder.Services.AddCors(options =>
    {
        // Only include origins that are used by your frontends
        options.AddPolicy("AllowSpecificOrigins", corsBuilder =>
        {
            corsBuilder
                // .WithOrigins(
                //     "https://localhost:7230",
                .AllowAnyOrigin() // Allow any origin to allow mobile app calls
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    IdentityModelEventSource.ShowPII = true;

    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "ModularNet API", Version = "v1" });
    });

    // Initialize Firebase App

    if (env.IsDevelopment())
        FirebaseApp.Create(
            new AppOptions
            {
                Credential = GoogleCredential.FromFile("modularnet-firebase-adminsdk-dev.json")
            });
    else
        try
        {
            // Configure FIREBASE_ADMIN_SDK in an Environment Variables Section (Azure App Service, for instance)
            var firebaseAdminSdkJson = Environment.GetEnvironmentVariable("FIREBASE_ADMIN_SDK");

            if (string.IsNullOrEmpty(firebaseAdminSdkJson))
                throw new Exception("Error getting Firebase Admin SDK credentials from environment variable");

            FirebaseApp.Create(
                new AppOptions
                {
                    Credential = GoogleCredential.FromJson(firebaseAdminSdkJson)
                });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"ERROR starting Firebase config: {ex.Message}");
        }

    builder.Services.AddAuthentication()
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
            jwtOptions =>
            {
                jwtOptions.Authority = builder.Configuration["AppSettings:FirebaseConfig:ValidIssuer"];
                jwtOptions.Audience = builder.Configuration["AppSettings:FirebaseConfig:Audience"];
                jwtOptions.TokenValidationParameters.ValidIssuer =
                    builder.Configuration["AppSettings:FirebaseConfig:ValidIssuer"];
            });

    // Application Insights
    builder.Services.AddApplicationInsightsTelemetry();

    RegisterDomainServices(builder.Services, builder.Configuration);
}

void ConfigureMiddleware(WebApplication app)
{
    // Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "ModularNet API V1"); });
    }

    // Request localization
    var supportedCultures = new[] { new CultureInfo("en-GB") };
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("en-GB"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures
    });

    app.UseHttpsRedirection();

    app.UseMiddleware<FirebaseAuthenticationMiddleware>();
    app.UseMiddleware<UserIdExtractionMiddleware>();

    // Important: UseAuthentication must go before UseRouting
    app.UseAuthentication();
    app.UseRouting();
    app.UseCors("AllowSpecificOrigins");

    app.UseAuthorization();

    app.UseSerilogRequestLogging();
}

void RegisterDomainServices(IServiceCollection services, IConfiguration configuration)
{
    // Register services from each layer
    services.AddInfrastructureServices(configuration);
    services.AddBusinessServices(configuration);
    services.AddApiServices(configuration);
}