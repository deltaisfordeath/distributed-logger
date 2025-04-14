using LogCollector.Models;
using Shared.Models;


namespace LogCollector.Services.Interfaces;

public interface ILogCollectorService
{
    Task<List<ServerLogMessage>> LogAsync(List<ServerLogMessage>? messages);
    Task<List<ServerLogMessage>> GetLogs(LogSearchFilter filter);
    Task<int> DeleteLogs(LogSearchFilter filter);
}