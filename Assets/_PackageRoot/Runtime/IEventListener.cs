namespace Lando.Events
{
    public interface IEventListener<in T> where T : IEvent
    {
        void Register() => EventBus.Register(listener: this);
        void Unregister() => EventBus.Unregister(listener: this);

        void OnListenedTo(T e);
    }
}