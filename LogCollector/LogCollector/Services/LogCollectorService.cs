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
    public async Task<JsonResult> LogAsync(LogMessage? message)
    {
        if (message == null) return new JsonResult("Please provide a valid log message.");
        var messageResponse = await _context.LogMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        return new JsonResult(messageResponse);
    }

    public async Task<List<LogMessage>> GetLogs()
    {
        var logs = await _context.LogMessages
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
        return logs;
    }
}