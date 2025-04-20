using Shared.Models;

namespace LogProducer.Services.Interfaces;

public interface IDistributedLogService
{
    Task<LogMessage?> LogAsync(LogMessage message);
    Task<List<LogMessage>?> SearchLogsAsync(LogSearchFilter filter);
    Task DeleteLogsAsync(LogSearchFilter filter);
    Task<List<LogMessage>?> LogManyAsync(List<LogMessage> messages);
}