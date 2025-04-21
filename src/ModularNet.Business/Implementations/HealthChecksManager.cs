using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class HealthChecksManager : IHealthChecksManager
{
    private readonly IHealthChecksRepository _healthChecksRepository;
    private readonly ILogger<HealthChecksManager> _logger;

    public HealthChecksManager(IHealthChecksRepository healthChecksRepository, ILogger<HealthChecksManager> logger)
    {
        _healthChecksRepository = healthChecksRepository;
        _logger = logger;

        _logger.LogDebug($"{nameof(HealthChecksManager)} constructed");
    }

    public async Task CheckDbConnection()
    {
        _logger.LogDebug($"Start method {nameof(CheckDbConnection)}");

        await _healthChecksRepository.CheckDbConnection();
    }

    public async Task<string> GetLocalIp()
    {
        _logger.LogDebug($"Start method {nameof(GetLocalIp)}");

        var host = await Dns.GetHostEntryAsync(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}