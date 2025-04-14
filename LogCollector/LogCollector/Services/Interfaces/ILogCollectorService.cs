using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace LogCollector.Services;

public interface ILogCollectorService
{
    Task<List<LogMessage>> LogAsync(List<LogMessage>? messages);
    Task<List<LogMessage>> GetLogs(LogSearchFilter filter);
    Task<int> DeleteLogs(LogSearchFilter filter);
}