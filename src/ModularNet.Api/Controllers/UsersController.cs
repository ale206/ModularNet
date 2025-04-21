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
public class UsersController : ControllerBase
{
    // The Web API will only accept tokens 1) for users, and 2) having the "ModularNet.Api.Access" scope for this API
    private readonly ILogger<UsersController> _logger;
    private readonly IUsersManager _usersManager;

    public UsersController(ILogger<UsersController> logger, IUsersManager usersManager)
    {
        _logger = logger;
        _usersManager = usersManager;

        _logger.LogDebug($"{nameof(UsersController)} constructed");
    }

    [HttpGet]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        try
        {
            _logger.LogDebug($"{nameof(GetUserByEmail)} endpoint has been reached");

            var result = await _usersManager.GetUserByEmail(email);
            return result != null
                ? Ok(result)
                : NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(GetUserByEmail)}");
            return StatusCode(500, new { ErrorMessage = "Failed to get user by email", ExceptionMessage = ex.Message });
        }
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById([FromRoute] string id)
    {
        try
        {
            _logger.LogDebug($"{nameof(GetUserById)} endpoint has been reached");

            var userId = Guid.Parse(id);
            var result = await _usersManager.GetUserById(userId);
            return result != null
                ? Ok(result)
                : NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(GetUserById)}");
            return StatusCode(500, new { ErrorMessage = "Failed to get user by id", ExceptionMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Save the User info received from the WebApp after being authenticated
    ///     This is used from the Mobile App as the registration in Firebase is made in the app
    /// </summary>
    /// <param name="registerUserRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser(RegisterUserRequest registerUserRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(RegisterUser)} endpoint has been reached");

            // Register or update the user
            var userId = await _usersManager.RegisterUserInModularNet(registerUserRequest);

            //TODO: Create URI
            return Created("", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(RegisterUser)}");
            return StatusCode(500, new { ErrorMessage = "Failed to register user", ExceptionMessage = ex.Message });
        }
    }


    /// <summary>
    ///     Save the User info received from the WebApp after being authenticated
    /// </summary>
    /// <param name="userSignInRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser(UserSignInRequest userSignInRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(LoginUser)} endpoint has been reached");

            await _usersManager.LoginUser(userSignInRequest);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(LoginUser)}");
            return StatusCode(500, new { ErrorMessage = "Failed to login user", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("terms/accept")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetTermsAndConditionsAsAccepted()
    {
        try
        {
            _logger.LogDebug($"{nameof(SetTermsAndConditionsAsAccepted)} endpoint has been reached");

            var userId = Guid.Parse(HttpContext.Items["UserId"].ToString() ?? throw new Exception("UserId is null"));

            await _usersManager.SetTermsAndConditionsAsAccepted(userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(SetTermsAndConditionsAsAccepted)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to set terms and conditions as accepted", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("deactivate-account")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateAccount()
    {
        try
        {
            _logger.LogDebug($"{nameof(DeactivateAccount)} endpoint has been reached");

            var userId = Guid.Parse(HttpContext.Items["UserId"].ToString() ?? throw new Exception("UserId is null"));

            await _usersManager.DeactivateAccount(userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(DeactivateAccount)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to deactivate the account", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("enable")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnableUser()
    {
        try
        {
            _logger.LogDebug($"{nameof(EnableUser)} endpoint has been reached");

            var userId = Guid.Parse(HttpContext.Items["UserId"].ToString() ?? throw new Exception("UserId is null"));

            await _usersManager.EnableUser(userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(EnableUser)}");
            return StatusCode(500, new { ErrorMessage = "Failed to enable the user", ExceptionMessage = ex.Message });
        }
    }
}