using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LogCollector.Models;
using Shared.Models;
using LogCollector.Services.Interfaces;

namespace LogCollector.Controllers;

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
        var serverMessage = new ServerLogMessage
        {
            HostId = hostId,
            Application = message.Application,
            Level = message.Level,
            Timestamp = message.Timestamp,
            Message = message.Message,
            UserId = message.UserId
        };
        message.UserId ??= hostId;
        var savedMessage = await _logService.LogAsync([serverMessage]);
        return new JsonResult(savedMessage);
    }
    
    [HttpPost]
    [Route("Batch")]
    public async Task<IActionResult> PostLogBatch([FromBody] List<ServerLogMessage> messages)
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
        return logs is { Count: > 0 } ? new JsonResult(logs) : Ok("No matching logs found");
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
        return deleted > 0 ? Ok($"Successfully deleted {deleted} logs.") : Ok("No matching logs found");
    }
}
