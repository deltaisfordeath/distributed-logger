using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogProducer.Data;

public class LogProducerDbContext(DbContextOptions<LogProducerDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<LogMessage> LogMessages { get; set; }
}