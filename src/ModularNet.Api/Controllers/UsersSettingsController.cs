using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.Api.Controllers;

[Authorize]
[RequiredScope("ModularNet.Api.Access")]
[ApiController]
[Route("[controller]")]
public class UsersSettingsController : ControllerBase
{
    // The Web API will only accept tokens 1) for users, and 2) having the "ModularNet.Api.Access" scope for this API
    private readonly ILogger<UsersSettingsController> _logger;
    private readonly IUsersSettingsManager _usersSettingsManager;

    public UsersSettingsController(ILogger<UsersSettingsController> logger, IUsersSettingsManager usersSettingsManager)
    {
        _logger = logger;
        _usersSettingsManager = usersSettingsManager;

        _logger.LogDebug($"{nameof(UsersController)} constructed");
    }

    [HttpPost]
    [Route("get-by-user")]
    [ProducesResponseType(typeof(IEnumerable<UserSetting>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserSettingsByUserId()
    {
        try
        {
            _logger.LogDebug($"{nameof(GetUserSettingsByUserId)} endpoint has been reached");

            var userId = Guid.Parse(HttpContext.Items["UserId"].ToString() ?? throw new Exception("UserId is null"));

            var userSettings = await _usersSettingsManager.GetUserSettingsByUserId(userId);
            return userSettings.Any()
                ? Ok(userSettings)
                : NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(GetUserSettingsByUserId)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to get user's settings", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("create-or-update")]
    [ProducesResponseType(typeof(IEnumerable<UserSetting>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrUpdateUserSettings(
        CreateOrUpdateUserSettingsRequest createOrUpdateUserSettingsRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(CreateOrUpdateUserSettings)} endpoint has been reached");

            if (!createOrUpdateUserSettingsRequest.UserSettings.Any())
                return BadRequest("No settings to be created or updated");

            // Register or update the user's settings
            var userSettings =
                await _usersSettingsManager.CreateOrUpdateUserSettings(createOrUpdateUserSettingsRequest);

            return Ok(userSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(CreateOrUpdateUserSettings)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to set user's settings", ExceptionMessage = ex.Message });
        }
    }
}