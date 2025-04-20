using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Lando.Events;
using EventLog = Lando.Events.Editor.EventLog;

namespace Lando.Events.Editor
{
    public class EventHistory :
        IEventListener<EventRegistered>,
        IEventListener<EventRaised>,
        IEventListener<EventUnregistered>
    {
        private static readonly List<EventLog> Logs = new();

        public static bool ShowRegister = true;
        public static bool ShowRaise = true;
        public static bool ShowUnregister = true;
        
        private static EventHistory _instance;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _instance = new EventHistory();
            // Logs.Clear();
        }
        
        private EventHistory()
        {
            EventBus.Register<EventRegistered>(listener: this);
            EventBus.Register<EventRaised>(listener: this);
            EventBus.Register<EventUnregistered>(listener: this);
        }
        
        ~EventHistory()
        {
            EventBus.Unregister<EventRegistered>(listener: this);
            EventBus.Unregister<EventRaised>(listener: this);
            EventBus.Unregister<EventUnregistered>(listener: this);
        }

        private static void Add(string message, EventLogType type)
        {
            Logs.Add(new EventLog
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            });
        }

        public static void Clear() => Logs.Clear();

        public static List<EventLog> GetFilteredLogs()
        {
            return Logs.Where(log =>
                (ShowRegister && log.Type == EventLogType.Register) ||
                (ShowRaise && log.Type == EventLogType.Raise) ||
                (ShowUnregister && log.Type == EventLogType.Unregister)
            ).ToList();
        }

        public static List<EventLog> GetLogs() => new(collection: Logs);

        public void OnListenedTo(EventRegistered e)
        {
            if (e.Listener == this) 
                return;
            Add($"{e.Listener.GetType().Name} registered for {e.EventType.Name}", EventLogType.Register);
        }

        public void OnListenedTo(EventRaised e)
        {
            if (e.Event is EventRegistered or EventUnregistered) 
                return;
         
            string[] ignoreNamespaces = { "Lando.Events", "UnityEngine" };
            
            StackTrace stackTrace = new StackTrace();
            string path = "";
            for (int i = stackTrace.FrameCount - 1; i > 0; i--)
            {
                MethodBase method = stackTrace.GetFrame(i).GetMethod();

                if (ignoreNamespaces.Any(Match))
                    continue;
                
                path += $"{method.DeclaringType?.Name}.{method.Name} > ";
                
                continue;

                bool Match(string ns) => method.DeclaringType?.Namespace?.Contains(ns) == true;
            }
            path = path.TrimEnd(' ', '>', ' ');
            Add($"{e.Event.GetType().Name} raised by {path}", EventLogType.Raise);
        }

        public void OnListenedTo(EventUnregistered e)
        {
            if (e.Listener == this) 
                return;
            Add($"{e.Listener.GetType().Name} unregistered for {e.EventType.Name}", EventLogType.Unregister);
        }
    }
}