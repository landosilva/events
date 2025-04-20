using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lando.Events
{
    public class EventListener : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private List<SerializedListener> listeners = new();

        private const BindingFlags BindingsFlag = BindingFlags.Public | BindingFlags.Static;
        private const string RegisterMethodName = "Register";
        private const string UnregisterMethodName = "Unregister";

        private void OnEnable()
        {
            UpdateListeners();
            RegisterListeners();
        }

        private void OnDisable()
        {
            UnregisterListeners();
        }

        private void RegisterListeners()
        {
            for (int i = 0; i < listeners.Count; i = i + 1)
            {
                SerializedListener entry = listeners[i];
                if (entry.Target == null)
                {
                    continue;
                }

                Type eventType = Type.GetType(entry.EventType);
                if (eventType == null)
                {
                    continue;
                }

                MethodInfo method = typeof(EventBus)
                    .GetMethod(RegisterMethodName, BindingsFlag)
                    .MakeGenericMethod(eventType);

                method.Invoke(null, new object[] { entry.Target });
            }
        }

        private void UnregisterListeners()
        {
            for (int i = 0; i < listeners.Count; i = i + 1)
            {
                SerializedListener entry = listeners[i];
                if (entry.Target == null)
                {
                    continue;
                }

                Type eventType = Type.GetType(entry.EventType);
                if (eventType == null)
                {
                    continue;
                }

                MethodInfo method = typeof(EventBus)
                    .GetMethod(UnregisterMethodName, BindingsFlag)
                    .MakeGenericMethod(eventType);

                method.Invoke(null, new object[] { entry.Target });
            }
        }

        public void UpdateListeners()
        {
            listeners.Clear();
            MonoBehaviour[] componentArray = GetComponents<MonoBehaviour>();
            for (int componentIndex = 0; componentIndex < componentArray.Length; componentIndex = componentIndex + 1)
            {
                MonoBehaviour componentEntry = componentArray[componentIndex];
                Type componentType = componentEntry.GetType();
                Type[] interfaceArray = componentType.GetInterfaces();
                for (int interfaceIndex = 0; interfaceIndex < interfaceArray.Length; interfaceIndex = interfaceIndex + 1)
                {
                    Type interfaceType = interfaceArray[interfaceIndex];
                    if (interfaceType.IsGenericType == false)
                    {
                        continue;
                    }

                    Type genericDefinition = interfaceType.GetGenericTypeDefinition();
                    if (genericDefinition != typeof(IEventListener<>))
                    {
                        continue;
                    }

                    Type[] genericArguments = interfaceType.GetGenericArguments();
                    Type eventType = genericArguments[0];
                    if (typeof(IEvent).IsAssignableFrom(eventType) == false)
                    {
                        continue;
                    }

                    SerializedListener newEntry = new SerializedListener(componentEntry, eventType);
                    listeners.Add(newEntry);
                }
            }
        }
    }
}
