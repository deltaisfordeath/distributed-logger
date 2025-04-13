using LogCollector.Data;
using LogCollector.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogCollector.Services;

public class LogCollectorService : ILogCollectorService

{
    private readonly LoggerDbContext _context;

    public LogCollectorService(LoggerDbContext context)
    {
        _context = context;
    }
    public async Task<List<LogMessage>> LogAsync(List<LogMessage>? messages)
    {
        if (messages == null) return [];
        await _context.LogMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();
        return messages;
    }

    public async Task<List<LogMessage>> GetLogs(LogSearchFilter filter)
    {
        {
            IQueryable<LogMessage> query = _context.LogMessages;
            
            if (!string.IsNullOrEmpty(filter.HostId))
            {
                query = query.Where(log => log.HostId == filter.HostId);
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                query = query.Where(log => log.UserId == filter.UserId);
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
                query = query.Where(log => log.Level == filter.LogLevel);
            }

            if (!string.IsNullOrEmpty(filter.Application))
            {
                query = query.Where(log => log.Application == filter.Application);
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query = query.Where(log => log.Message.Contains(filter.SearchText));
            }

            return await query.ToListAsync();
        }
    }
}