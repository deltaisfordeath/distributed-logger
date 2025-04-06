using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogCollector.Data;

public class LoggerDbContext(DbContextOptions<LoggerDbContext> options) : IdentityDbContext<IdentityUser>(options);