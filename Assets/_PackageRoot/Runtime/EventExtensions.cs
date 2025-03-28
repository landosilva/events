using UnityEngine;

namespace Lando.Events
{
    public static class EventExtensions
    {
        public static void Raise<T>(this T e) where T : IEvent => EventBus.Raise(e);

        public static void Register<T>(this MonoBehaviour listener) where T : IEvent
        {
            IEventListener<T> listenerComponent = (IEventListener<T>)listener;
            listenerComponent.Register();
        }
        
        public static void Unregister<T>(this MonoBehaviour listener) where T : IEvent
        {
            IEventListener<T> listenerComponent = (IEventListener<T>)listener;
            listenerComponent.Unregister();
        }
    }
}