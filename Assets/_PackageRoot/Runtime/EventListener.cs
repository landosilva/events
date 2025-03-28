using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Lando.Events
{
    public class EventListener : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private List<SerializedListener> listeners = new();

        private const BindingFlags BindingsAttr = BindingFlags.Public | BindingFlags.Static;
        private const string RegisterMethodName = "Register";
        private const string UnregisterMethodName = "Unregister";
        
        private void OnEnable() => RegisterListeners();
        private void OnDisable() => UnregisterListeners();

        private void RegisterListeners()
        {
            foreach (SerializedListener listener in listeners)
            {
                if (listener.Target == null) 
                    continue;
                
                Type eventType = Type.GetType(listener.EventType);
                if (eventType == null) continue;

                MethodInfo registerMethod = typeof(EventBus)
                    .GetMethod(RegisterMethodName, BindingsAttr)?
                    .MakeGenericMethod(eventType);

                registerMethod?.Invoke(null, new object[] { listener.Target });
            }
        }

        private void UnregisterListeners()
        {
            foreach (SerializedListener listener in listeners)
            {
                if (listener.Target == null) 
                    continue;
                
                Type eventType = Type.GetType(listener.EventType);
                if (eventType == null) 
                    continue;

                MethodInfo unregisterMethod = typeof(EventBus)
                    .GetMethod(UnregisterMethodName, BindingsAttr)?
                    .MakeGenericMethod(eventType);

                unregisterMethod?.Invoke(null, new object[] { listener.Target });
            }
        }

        public void UpdateListeners()
        {
            listeners.Clear();
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                IEnumerable<Type> interfaces = component.GetType().GetInterfaces().Where(IsGenericType);

                foreach (Type @interface in interfaces)
                {
                    Type eventType = @interface.GetGenericArguments()[0];
                    if (!typeof(IEvent).IsAssignableFrom(eventType))
                        continue;

                    listeners.Add(new SerializedListener(component, eventType));
                }

                continue;

                bool IsGenericType(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEventListener<>);
            }
        }
    }
}
