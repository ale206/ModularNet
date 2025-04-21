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
public class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;
    private readonly ILogsManager _logsManager;

    public LogsController(ILogger<LogsController> logger, ILogsManager logsManager)
    {
        _logger = logger;
        _logsManager = logsManager;

        _logger.LogDebug($"{nameof(LogsController)} constructed");
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> WriteLog([FromBody] WriteLogRequest writeLogRequest)
    {
        try
        {
            _logger.LogDebug($"{nameof(WriteLog)} endpoint has been reached");

            var modularNetLog = new ModularNetLog
            {
                LogException = writeLogRequest.LogException,
                LogLevel = writeLogRequest.LogLevel,
                LogMessage = writeLogRequest.LogMessage,
                LogProperties = writeLogRequest.LogProperties,
                LogTimeStamp = writeLogRequest.LogTimeStamp
            };

            await _logsManager.WriteLogToDb(modularNetLog);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in {nameof(WriteLog)}");
            return StatusCode(500, new { ErrorMessage = "Failed to write log", ExceptionMessage = ex.Message });
        }
    }
}