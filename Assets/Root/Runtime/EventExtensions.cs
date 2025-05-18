using UnityEngine;

namespace Lando.EventWeaver
{
    public static class EventExtensions
    {
        public static void Raise<T>(this T e) where T : IEvent => EventRegistry.Raise(e);
    }
}