using System;

namespace Lando.Events.Editor
{
    public class EventLog
    {
        public string Message { get; set; }
        public EventLogType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}