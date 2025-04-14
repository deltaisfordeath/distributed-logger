using Shared.Models;

namespace LogCollector.Models
{
    public class ServerLogMessage: LogMessage
    {
        public string HostId { get; set; }
    }
}
