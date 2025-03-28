using System;
using UnityEngine;

namespace Lando.Events
{
    [Serializable]
    public class SerializedListener
    {
        [field: SerializeField] public MonoBehaviour Target {get; private set;}
        [field: SerializeField] public string EventType {get; private set;}

        public SerializedListener(MonoBehaviour target, Type eventType)
        {
            Target = target;
            EventType = eventType.AssemblyQualifiedName;
        }
    }
}