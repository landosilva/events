using System;

namespace Lando.Events
{
    public class EventUnregistered : IEvent
    {
        public object Listener { get; }
        public Type EventType { get; }
        
        public EventUnregistered(object listener, Type eventType)
        {
            Listener = listener;
            EventType = eventType;
        }
    }
}