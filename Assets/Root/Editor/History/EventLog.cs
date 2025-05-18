using System;

namespace Lando.EventWeaver.Editor.History
{
    public class EventLog
    {
        public string Message { get; set; }
        public EventLogType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}