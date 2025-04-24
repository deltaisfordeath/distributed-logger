using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class LogMessage
{
    [Key]
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Application { get; set; }
    public string Level { get; set; } // DEBUG, INFO, WARNING, ERROR, FATAL
    public string Message { get; set; }
    public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
}