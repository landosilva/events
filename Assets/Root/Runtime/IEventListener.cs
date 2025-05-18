namespace Lando.EventWeaver
{
    public interface IEventListener<in T> where T : IEvent
    { 
        void OnListenedTo(T e);
    }
}