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
        var serverMessage = ServerLogMessage.ConvertFromLogMessage(message, hostId);
        var savedMessage = await _logService.LogAsync([serverMessage]);
        return savedMessage != null ? new JsonResult(savedMessage) : StatusCode(500);
    }
    
    [HttpPost]
    [Route("Batch")]
    public async Task<IActionResult> PostLogBatch([FromBody] List<LogMessage> messages)
    {
        var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (hostId == null) return Unauthorized("User id not found.");
        var serverMessages = messages.
            Select(message => ServerLogMessage.ConvertFromLogMessage(message, hostId))
            .ToList();
        var savedMessage = await _logService.LogAsync(serverMessages);
        return savedMessage != null ? new JsonResult(savedMessage) : StatusCode(500);
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
        if (logs == null) return StatusCode(500);
        return logs is { Count: > 0 } ? new JsonResult(logs) : new JsonResult(new List<LogMessage>());
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
        if (deleted == null) return StatusCode(500);
        return deleted > 0 ? Ok($"Successfully deleted {deleted} logs.") : Ok("No matching logs found");
    }
}
