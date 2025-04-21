using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly IEncryptManager _encryptManager;
    private readonly ILogger<AuthController> _logger;
    private readonly IUsersManager _usersManager;

    public AuthController(ILogger<AuthController> logger, IAuthManager authManager,
        IUsersManager usersManager, IEncryptManager encryptManager)
    {
        _logger = logger;
        _authManager = authManager;
        _usersManager = usersManager;
        _encryptManager = encryptManager;

        _logger.LogDebug($"{nameof(AuthController)} constructed");
    }

    [HttpPost]
    [Route("sign-up")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUpUser(SignUpUserRequest signUpUserRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(SignUpUser)} endpoint has been reached");

            if (!signUpUserRequest.TermsAccepted)
                throw new Exception("Terms and Conditions must be accepted");

            var userIdFirebase =
                await _authManager.RegisterUserInFirebase(signUpUserRequest.Email, signUpUserRequest.Password);

            // Check if the user is already registered
            var user = await _usersManager.GetUserByEmail(signUpUserRequest.Email);

            // If the user is already registered, just update the UserOid
            if (user != null)
            {
                await _usersManager.UpdateUserOid(user.Id, userIdFirebase);
                return Created("", "User was already registered");
            }

            // If not, continue with the registration
            var newUser = new RegisterUserRequest
            {
                UserOid = userIdFirebase,
                Email = signUpUserRequest.Email,
                FirstName = signUpUserRequest.FirstName,
                LastName = signUpUserRequest.LastName,
                TermsAndConditionsAcceptedOn = DateTime.UtcNow
            };

            // Register the user in the system
            var userId = await _usersManager.RegisterUserInModularNet(newUser);

            //TODO: Create URI
            return Created("", userId);
        }
        catch (FirebaseAuthException fex)
        {
            // Return the error message from Firebase with status code 
            return StatusCode(400, new { ErrorMessage = fex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(SignUpUser)}");
            return StatusCode(500, new { ErrorMessage = "Failed to sign up the user", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("sign-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignInUser(SignInUserRequest signInUserRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(SignInUser)} endpoint has been reached");

            var accessToken =
                await _authManager.LoginUserInFirebase(signInUserRequest.Email, signInUserRequest.Password);

            if (string.IsNullOrEmpty(accessToken))
                return StatusCode(401, new { ErrorMessage = "Wrong username or password" });

            var user = await _usersManager.GetUserByEmail(signInUserRequest.Email);

            if (user == null)
                throw new Exception("Error retrieving User information");

            var userLoginRequest = new UserSignInRequest
            {
                AuthenticatedOn = DateTime.UtcNow,
                Id = user.Id
            };

            await _usersManager.LoginUser(userLoginRequest);

            return Ok(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(SignInUser)}");
            return StatusCode(500, new { ErrorMessage = "Failed to sign in the user", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("verify-authentication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyAuthentication(VerifyAuthenticationRequest verifyAuthenticationRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(VerifyAuthentication)} endpoint has been reached");

            var isValidToken =
                await _authManager.VerifyAuthenticationInFirebase(verifyAuthenticationRequest.AccessToken);

            if (!isValidToken)
                return Unauthorized();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(VerifyAuthentication)}");
            return StatusCode(500, new { ErrorMessage = "Failed to validate token", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(LogoutRequest logoutRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(Logout)} endpoint has been reached");

            await _authManager.LogoutFromFirebase(logoutRequest.Uid);

            var userId = HttpContext.Items["UserId"].ToString() ?? throw new Exception("UserId is null");

            await _authManager.ClearCache(userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(Logout)}");
            return StatusCode(500, new { ErrorMessage = "Failed to logout user", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    [Route("external-provider-signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignInWithExternalProvider(
        SignInWithExternalProviderRequest signInWithExternalProviderRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(SignInWithExternalProvider)} endpoint has been reached");

            // At this point, the user is already authenticated in Firebase

            var user = await _usersManager.GetUserByEmail(signInWithExternalProviderRequest.Email);

            if (user == null)
            {
                // Register

                // Divide the name into first and last name
                var names = signInWithExternalProviderRequest.DisplayName.Split(" ");

                // The first one is the first name
                var firstName = names[0];

                // Take all the rest for lastname
                var lastName = string.Join(" ", names.Skip(1));

                // Register the user in the system
                //TODO: We receive the providerId and the DB should be refactored to save this, plus the related userOid for that provider, as a user could use multiple providers to sign in. 
                var newUser = new RegisterUserRequest
                {
                    UserOid = signInWithExternalProviderRequest.Uid,
                    Email = signInWithExternalProviderRequest.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    TermsAndConditionsAcceptedOn = DateTime.UtcNow,
                    IsEmailVerified = signInWithExternalProviderRequest.IsEmailVerified,
                    IsEnabled = true
                };

                // Register the user in the system
                var userId = await _usersManager.RegisterUserInModularNet(newUser);
                user = new User
                {
                    Id = userId
                };
            }

            // Login (Always)

            // Enable the user and set the email as verified
            await _usersManager.EnableUser(user.Id);
            await _usersManager.SetUserEmailAsVerified(user.Id);

            // Login the user
            var userLoginRequest = new UserSignInRequest
            {
                AuthenticatedOn = DateTime.UtcNow,
                Id = user.Id
            };

            await _usersManager.LoginUser(userLoginRequest);

            // Return the token
            return Ok(signInWithExternalProviderRequest.IdToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(SignInWithExternalProvider)}");
            return StatusCode(500,
                new { ErrorMessage = "Failed to sign in with Google", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("get-user-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserToken(GetUserTokenRequest getUserTokenRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(GetUserToken)} endpoint has been reached");

            // Check if the access code is valid, as this endpoint doesn't use the Firebase token
            var isAccessCodeValid = await _encryptManager.IsAccessCodeValid(getUserTokenRequest.OneAccessCode);

            if (!isAccessCodeValid)
                return BadRequest("Invalid access code");

            var token =
                await _authManager.GetUserToken(getUserTokenRequest.Email);

            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(GetUserToken)}");
            return StatusCode(500, new { ErrorMessage = "Failed to get user token", ExceptionMessage = ex.Message });
        }
    }

    [HttpPost]
    [Route("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(ResetPassword)} endpoint has been reached");

            // Check if the access code is valid, as this endpoint doesn't use the Firebase token
            var isAccessCodeValid = await _encryptManager.IsAccessCodeValid(resetPasswordRequest.OneAccessCode);

            if (!isAccessCodeValid)
                return BadRequest("Invalid access code");

            await _authManager.ResetPasswordInFirebase(resetPasswordRequest.Email);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(ResetPassword)}");
            return StatusCode(500, new { ErrorMessage = "Failed to reset password", ExceptionMessage = ex.Message });
        }
    }
}