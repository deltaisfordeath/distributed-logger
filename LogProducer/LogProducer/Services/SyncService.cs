using LogProducer.Data;
using LogProducer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public SyncService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncWithRemoteServerAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Sync service encountered an exception: {0}", e);
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task SyncWithRemoteServerAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<IDistributedLogService>();
        var context = scope.ServiceProvider.GetRequiredService<LogProducerDbContext>();
        var messages = await context.TempLogMessages.ToListAsync();
        if (messages.Count == 0) return;

        var result = await logService.LogManyAsync(messages);
        if (result != null)
        {
             context.TempLogMessages.RemoveRange(messages);
             await context.SaveChangesAsync();
        }
    }
}