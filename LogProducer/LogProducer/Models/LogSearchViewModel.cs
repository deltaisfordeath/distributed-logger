using Shared.Models;

namespace LogProducer.Models
{
    public class LogSearchViewModel
    {
        public LogSearchFilter Filter { get; set; }
        public List<LogMessage> Results {  get; set; }
    }
}
