using Shared.Models;

namespace LogProducer.Services.Interfaces;

public interface IDistributedLogService
{
    Task<LogMessage> LogAsync(LogMessage message);
    Task<List<LogMessage>> GetLogsAsync(LogSearchFilter filter);
    Task DeleteLogsAsync(LogSearchFilter filter);
}