using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogProducer.Data;

public class LogProducerDbContext(DbContextOptions<LogProducerDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<LogMessage> TempLogMessages { get; set; }

    public async Task<List<LogMessage>> SearchLogsAsync(LogSearchFilter filter)
    {
        var query = BuildQuery(filter);
        var logs = await query.ToListAsync();
        return logs;
    }

private IQueryable<LogMessage> BuildQuery(LogSearchFilter filter)
{
    IQueryable<LogMessage> query = TempLogMessages;

    if (filter.Id != null)
    {
        query = query.Where(log => log.Id == filter.Id);
        return query;
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