using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Interfaces;
using MySqlConnector;

namespace ModularNet.Infrastructure.Implementations;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<DbConnectionFactory> _logger;
    private readonly ISecretsRepository _secretsRepository;

    public DbConnectionFactory(IConfiguration configuration, IHostingEnvironment hostingEnvironment,
        ISecretsRepository secretsRepository, ILogger<DbConnectionFactory> logger)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        _secretsRepository = secretsRepository;
        _logger = logger;

        _logger.LogDebug($"{nameof(DbConnectionFactory)} constructed");
    }

    public DbConnection GetDbConnection(string connectionString)
    {
        _logger.LogDebug($"Start repository method {nameof(GetDbConnection)}");

        // This is to return proper DateTime from MySql instead of 01-01-0001
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return new MySqlConnection(connectionString);
    }

    public async Task<string> GetDbConnectionString()
    {
        _logger.LogDebug($"Start repository method {nameof(GetDbConnectionString)}");

        if (_hostingEnvironment.IsDevelopment())
            return _configuration.GetConnectionString("ModularNetConnectionString") ??
                   throw new Exception("Error getting Db Connection String");

        //TODO: Create If for each environment when ready
        //else if(_hostingEnvironment.EnvironmentName == "STG") 

        return await _secretsRepository.GetSecret("connection-string") ??
               throw new Exception("Error Getting Db Connection String from Secrets");
    }
}