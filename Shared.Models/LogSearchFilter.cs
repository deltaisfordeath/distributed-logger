namespace Shared.Models;

public class LogSearchFilter
{
    public int? Id { get; set; }
    public string? HostId { get; set; }
    public string? UserId { get; set; }
    public DateTime? SearchStart { get; set; }
    public DateTime? SearchEnd { get; set; }
    public string? LogLevel { get; set; }
    public string? Application { get; set; }
    public string? SearchText { get; set; }
}