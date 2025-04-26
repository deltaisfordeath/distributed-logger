using LogCollector.Data;
using LogCollector.Models;
using LogCollector.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogCollector.Services;

public class LogCollectorService : ILogCollectorService

{
    private readonly LoggerDbContext _context;

    public LogCollectorService(LoggerDbContext context)
    {
        _context = context;
    }
    public async Task<List<ServerLogMessage>?> LogAsync(List<ServerLogMessage>? messages)
    {
        if (messages == null) return [];
        try
        {
            await _context.LogMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();
            return messages;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<List<ServerLogMessage>?> GetLogs(LogSearchFilter filter)
    {
        IQueryable<ServerLogMessage> query = BuildQuery(filter);
        try
        {
            var messages = await query.ToListAsync();
            return messages;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    public async Task<int?> DeleteLogs(LogSearchFilter filter)
    {
        IQueryable<ServerLogMessage> query = BuildQuery(filter);
        try
        {
            var deleted = await query.ExecuteDeleteAsync();
            return deleted;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private IQueryable<ServerLogMessage> BuildQuery(LogSearchFilter filter)
    {
        IQueryable<ServerLogMessage> query =_context.LogMessages;

        if (filter.Id != null)
        {
            query = query.Where(log => log.Id == filter.Id);
            return query;
        }
        
        if (!string.IsNullOrEmpty(filter.HostId))
        {
            query = query.Where(log => log.HostId == filter.HostId);
        }

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            query = query.Where(log => log.UserId.ToLower() == filter.UserId.ToLower());
        }

        if (filter.SearchStart.HasValue)
        {
            query = query.Where(log => log.Timestamp >= filter.SearchStart.Value);
        }

        if (filter.SearchEnd.HasValue)
        {
            query = query.Where(log => log.Timestamp <= filter.SearchEnd.Value);
        }

        if (!string.IsNullOrEmpty(filter.LogLevel))
        {
            query = query.Where(log => log.Level.ToLower() == filter.LogLevel.ToLower());
        }

        if (!string.IsNullOrEmpty(filter.Application))
        {
            query = query.Where(log => log.Application.ToLower() == filter.Application);
        }

        if (!string.IsNullOrEmpty(filter.SearchText))
        {
            query = query.Where(log => log.Message.ToLower().Contains(filter.SearchText.ToLower()));
        }

        return query;
    }
}