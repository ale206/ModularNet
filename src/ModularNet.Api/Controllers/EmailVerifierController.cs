using Microsoft.AspNetCore.Mvc;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Requests;

namespace ModularNet.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailVerifierController : ControllerBase
{
    private readonly IEmailVerifierManager _emailVerifierManager;
    private readonly ILogger<EmailVerifierController> _logger;

    public EmailVerifierController(ILogger<EmailVerifierController> logger, IEmailVerifierManager emailVerifierManager)
    {
        _logger = logger;
        _emailVerifierManager = emailVerifierManager;

        _logger.LogDebug($"{nameof(EmailVerifierController)} constructed");
    }

    [HttpPost]
    [Route("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest verifyEmailRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(VerifyEmail)} endpoint has been reached");

            var isEmailVerified = await _emailVerifierManager.VerifyEmail(verifyEmailRequest.EmailVerificationCode);

            return Ok(isEmailVerified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(VerifyEmail)}");
            return StatusCode(500, new { ErrorMessage = "Failed to verify email", ExceptionMessage = ex.Message });
        }
    }
}