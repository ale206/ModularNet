using Microsoft.AspNetCore.Mvc;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;

namespace ModularNet.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthChecksController : ControllerBase
{
    private readonly ICacheManager _cacheManager;
    private readonly IHealthChecksManager _healthChecksManager;
    private readonly ILogger<HealthChecksController> _logger;

    public HealthChecksController(ILogger<HealthChecksController> logger, IHealthChecksManager healthChecksManager,
        ICacheManager cacheManager)
    {
        _healthChecksManager = healthChecksManager;
        _cacheManager = cacheManager;
        _logger = logger;

        _logger.LogDebug($"{nameof(HealthChecksController)} constructed");
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Check()
    {
        try
        {
            _logger.LogDebug($"{nameof(Check)} endpoint has been reached");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(Check)}");
            return StatusCode(500, new { ErrorMessage = "Failed to check", ExceptionMessage = ex.Message });
        }
    }

    [HttpGet]
    [Route("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckDbConnection()
    {
        try
        {
            _logger.LogDebug($"{nameof(CheckDbConnection)} endpoint has been reached");

            await _healthChecksManager.CheckDbConnection();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(CheckDbConnection)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to check db connection", ExceptionMessage = ex.Message });
        }
    }

    [HttpGet]
    [Route("memory-cache")]
    public async Task<IActionResult> CheckInMemoryCache([FromQuery] bool external)
    {
        try
        {
            _logger.LogDebug($"{nameof(CheckInMemoryCache)} endpoint has been reached");

            const string cacheItemName = "cachedItem";
            const CacheType cacheType = CacheType.InMemory;

            var cachedItem = await _cacheManager.GetFromCache<DateTime>(cacheItemName, cacheType);

            if (cachedItem == default)
            {
                // Save for one month
                await _cacheManager.SaveInCache(cacheItemName, DateTime.UtcNow, cacheType, 2592000);
                cachedItem = await _cacheManager.GetFromCache<DateTime>(cacheItemName, cacheType);
            }

            return Ok(cachedItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(CheckInMemoryCache)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to get item from cache", ExceptionMessage = ex.Message });
        }
    }

    [HttpGet]
    [Route("redis-cache")]
    public async Task<IActionResult> CheckRedisCache([FromQuery] bool external)
    {
        try
        {
            _logger.LogDebug($"{nameof(CheckRedisCache)} endpoint has been reached");

            const string cacheItemName = "cachedItem";
            const CacheType cacheType = CacheType.Redis;

            var cachedItem = await _cacheManager.GetFromCache<DateTime>(cacheItemName, cacheType);

            if (cachedItem == default)
            {
                // Save for one month
                await _cacheManager.SaveInCache(cacheItemName, DateTime.UtcNow, cacheType, 2592000);
                cachedItem = await _cacheManager.GetFromCache<DateTime>(cacheItemName, cacheType);
            }

            return Ok(cachedItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(CheckRedisCache)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to get item from cache", ExceptionMessage = ex.Message });
        }
    }
}