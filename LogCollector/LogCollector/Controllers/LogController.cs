using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LogCollector.Models;
using LogCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DistributedLogger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (hostId == null) return Unauthorized("User id not found.");
        message.HostId = hostId;
        message.UserId ??= hostId;
        var savedMessage = await _logService.LogAsync([message]);
        return new JsonResult(savedMessage);
    }
    
    [HttpPost]
    [Route("Batch")]
    public async Task<IActionResult> PostLogBatch([FromBody] List<LogMessage> messages)
    {
        var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (hostId == null) return Unauthorized("User id not found.");
        foreach (var message in messages)
        {
            message.HostId = hostId;
            message.UserId ??= hostId;
        }
        var savedMessage = await _logService.LogAsync(messages);
        return new JsonResult(savedMessage);
    }

    [HttpPost]
    [Route("Search")]
    public async Task<IActionResult> SearchLogs([FromBody]LogSearchFilter? filter)
    {
        filter ??= new LogSearchFilter();
        if (!User.IsInRole("Admin"))
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            filter.HostId = hostId;
        }
        var logs = await _logService.GetLogs(filter);
        return logs is { Count: > 0 } ? new JsonResult(logs) : new JsonResult("No matching logs found");
    }
    
    [HttpPost]
    [Route("Delete")]
    public async Task<IActionResult> DeleteLogs([FromBody]LogSearchFilter? filter)
    {
        filter ??= new LogSearchFilter();
        if (!User.IsInRole("Admin"))
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            filter.HostId = hostId;
        }
        var deleted = await _logService.DeleteLogs(filter);
        return Ok($"Successfully deleted {deleted} logs.");
    }
}
