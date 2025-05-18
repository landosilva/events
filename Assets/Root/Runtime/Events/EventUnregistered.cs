using System;

namespace Lando.EventWeaver.Events
{
    public record EventUnregistered(object Listener, Type EventType) : IEvent
    {
        public object Listener { get; } = Listener;
        public Type EventType { get; } = EventType;
    }
}