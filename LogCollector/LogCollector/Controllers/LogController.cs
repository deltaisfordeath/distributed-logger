using Microsoft.AspNetCore.Mvc;
using LogCollector.Models;
using LogCollector.Services;
using Microsoft.AspNetCore.Identity;

namespace DistributedLogger.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogCollectorService _logService;
    private readonly UserManager<IdentityUser> _userManager;

    public LogController(ILogCollectorService logService, UserManager<IdentityUser> userManager)
    {
        _logService = logService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> PostLog([FromBody] LogMessage? message)
    {
        if (message == null || string.IsNullOrEmpty(message.Message))
            return BadRequest("Invalid log message.");

        var response = await _logService.LogAsync(message);
        return response;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _logService.GetLogs();
        return logs is { Count: > 0 } ? new JsonResult(logs) : new JsonResult("No matching logs found");
    }
}
