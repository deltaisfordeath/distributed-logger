using Shared.Models;

namespace LogProducer.Services.Interfaces;

public interface IDistributedLogService
{
    Task<LogMessage?> LogAsync(LogMessage message);
    Task<List<LogMessage>?> LogManyAsync(List<LogMessage> messages);
    Task<LogMessage?> AddLogToQueue(LogMessage message);
    Task<List<LogMessage>?> AddManyToQueue(List<LogMessage> messages);
    Task<List<LogMessage>?> SearchLogsAsync(LogSearchFilter filter);
    Task DeleteLogsAsync(LogSearchFilter filter);
}