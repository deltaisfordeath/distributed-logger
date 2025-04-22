using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogProducer.Data;

public class LogProducerDbContext(DbContextOptions<LogProducerDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<LogMessage> TempLogMessages { get; set; }
    
    public int ClearTable<T>() where T : class
    {
        var tableName = Model.FindEntityType(typeof(T)).GetTableName();
        return Database.ExecuteSqlRaw($"DELETE FROM \"{tableName}\"");
    }

public DbSet<Shared.Models.LogSearchFilter> LogSearchFilter { get; set; } = default!;
}