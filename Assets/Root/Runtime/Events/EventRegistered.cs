using System;

namespace Lando.EventWeaver.Events
{
    public record EventRegistered(object Listener, Type EventType) : IEvent
    {
        public object Listener { get; } = Listener;
        public Type EventType { get; } = EventType;
    }
}