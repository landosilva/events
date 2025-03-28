using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lando.Events.Editor
{
    [CustomEditor(typeof(EventListener))]
    public class EventListenerEditor : UnityEditor.Editor
    {
        private Dictionary<MonoBehaviour, List<Type>> _groupedListeners;
        private readonly Dictionary<MonoBehaviour, bool> _foldoutStates = new();
        
        private Texture _icon;

        private void OnEnable()
        {
            LoadIcon();
            RefreshListeners();
            GroupListeners();
        }

        private void LoadIcon()
        {
            string[] guids = AssetDatabase.FindAssets(filter: "icon-event-listener");
            if (guids.Length <= 0)
            {
                _icon = EditorGUIUtility.FindTexture(name: "console.infoicon");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_groupedListeners == null || !_groupedListeners.Any())
            {
                DisplayNoListenersWarning();
                return;
            }

            DisplayEventListeners();
        }

        private void RefreshListeners()
        {
            EventListener eventListener = (EventListener)target;
            eventListener.UpdateListeners();
            EditorUtility.SetDirty(eventListener);
            GroupListeners();
        }

        private void GroupListeners()
        {
            EventListener eventListener = (EventListener)target;
            MonoBehaviour[] listeners = eventListener.GetComponents<MonoBehaviour>()
                .Where(IsListener)
                .ToArray();

            _groupedListeners = new Dictionary<MonoBehaviour, List<Type>>();

            foreach (MonoBehaviour listener in listeners)
            {
                Type type = listener.GetType();
                Type[] interfaces = type.GetInterfaces()
                    .Where(IsGenericType)
                    .Where(@interface => typeof(IEvent).IsAssignableFrom(@interface.GetGenericArguments()[0]))
                    .ToArray();

                if (!interfaces.Any()) 
                    continue;
                
                if (!_groupedListeners.ContainsKey(listener))
                    _groupedListeners[listener] = new List<Type>();

                foreach (Type @interface in interfaces) 
                    _groupedListeners[listener].Add(@interface.GetGenericArguments()[0]);
            }
        }

        private static bool IsListener(MonoBehaviour component)
        {
            return component.GetType().GetInterfaces().Any(IsGenericType);
        }

        private static bool IsGenericType(Type type)
        {
            return type.IsGenericType && 
                   type.GetGenericTypeDefinition() == typeof(IEventListener<>);
        }

        private static void DisplayNoListenersWarning()
        {
            EditorGUILayout.HelpBox(
                message: "No listeners found on this GameObject.",
                MessageType.Warning
            );
        }

        private void DisplayEventListeners()
        {
            foreach (var pair in _groupedListeners)
            {
                _foldoutStates.TryAdd(pair.Key, true);
                DisplayGroupedEventListener(pair.Key, pair.Value);
            }
        }

        private void DisplayGroupedEventListener(MonoBehaviour listener, List<Type> eventTypes)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(left: 10, right: 10, top: 5, bottom: 5),
                fixedHeight = 24
            };

            EditorGUILayout.BeginHorizontal();

            GUIContent buttonContent = new GUIContent(text: $" {listener.GetType().Name} ({eventTypes.Count})");
            if (GUILayout.Button(buttonContent, buttonStyle))
                _foldoutStates[listener] = !_foldoutStates[listener];

            EditorGUILayout.EndHorizontal();

            if (!_foldoutStates[listener])
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel++;
            foreach (var eventType in eventTypes)
                DisplayEventLabel(eventType.Name);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }

        private void DisplayEventLabel(string eventName)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(left: 0, right: 0, top: 2, bottom: 2)
            };

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(_icon, GUILayout.Width(20), GUILayout.Height(20));

            EditorGUILayout.LabelField($"{eventName}", labelStyle);

            EditorGUILayout.EndHorizontal();
        }
    }
}
