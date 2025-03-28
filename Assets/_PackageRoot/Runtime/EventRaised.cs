namespace Lando.Events
{
    public class EventRaised : IEvent
    {
        public IEvent Event { get; }
        
        public EventRaised(IEvent @event)
        {
            Event = @event;
        }
    }
}