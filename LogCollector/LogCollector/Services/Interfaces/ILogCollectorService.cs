using LogCollector.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogCollector.Services;

public interface ILogCollectorService
{
    Task<List<LogMessage>> LogAsync(List<LogMessage>? messages);
    Task<List<LogMessage>> GetLogs(LogSearchFilter filter);
}