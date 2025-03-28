using System;

namespace Lando.Events
{
    public class EventRegistered : IEvent
    {
        public object Listener { get; }
        public Type EventType { get; }
        
        public EventRegistered(object listener, Type eventType)
        {
            Listener = listener;
            EventType = eventType;
        }
    }
}