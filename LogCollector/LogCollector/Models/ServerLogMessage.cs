using Shared.Models;

namespace LogCollector.Models
{
    public class ServerLogMessage: LogMessage
    {
        public string HostId { get; set; }

        public static ServerLogMessage ConvertFromLogMessage(LogMessage message, string hostId = "")
        {
            return new ServerLogMessage
            {
                Application = message.Application,
                HostId = hostId,
                Level = message.Level,
                Message = message.Message,
                Timestamp = message.Timestamp,
                UserId = message.UserId ?? hostId
            };
        }
    }
}
