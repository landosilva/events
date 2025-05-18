namespace Lando.EventWeaver.Events
{
    public record EventRaised(IEvent Event) : IEvent
    {
        public IEvent Event { get; } = Event;
    }
}