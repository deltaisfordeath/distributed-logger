using LogCollector.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogCollector.Services;

public interface ILogCollectorService
{
    Task<JsonResult> LogAsync(LogMessage? message);
    Task<List<LogMessage>> GetLogs();
}