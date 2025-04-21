using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using ModularNet.Business.Interfaces;

namespace ModularNet.Api.Controllers;

[Authorize]
[RequiredScope("ModularNet.Api.Access")]
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SecretsController : ControllerBase
{
    // The Web API will only accept tokens 1) for users, and 2) having the "ModularNet.Api.Access" scope for this API
    private readonly ILogger<SecretsController> _logger;
    private readonly ISecretsManager _secretsManager;

    public SecretsController(ILogger<SecretsController> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        _logger.LogDebug($"{nameof(SecretsController)} constructed");
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSecret([FromQuery] string key)
    {
        try
        {
            _logger.LogDebug($"{nameof(GetSecret)} endpoint has been reached");

            // Register or update the user
            var result = await _secretsManager.GetSecretAndCacheIt(key);
            return result != null
                ? Ok(result)
                : NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(GetSecret)}");
            return StatusCode(500, new { ErrorMessage = "Failed to get secret", ExceptionMessage = ex.Message });
        }
    }
}