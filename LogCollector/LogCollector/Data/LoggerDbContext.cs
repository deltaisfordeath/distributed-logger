using LogCollector.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogCollector.Data;

public class LoggerDbContext(DbContextOptions<LoggerDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<LogMessage> LogMessages { get; set; }
}