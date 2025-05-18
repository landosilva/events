using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lando.EventWeaver.Editor.Windows
{
    public class EventViewerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        [MenuItem("Tools/Lando/Event Weaver/Event Viewer")]
        public static void ShowWindow()
        {
            EventViewerWindow window = GetWindow<EventViewerWindow>(false);
            window.minSize = new Vector2(300, 200);

            Texture windowIcon = EditorGUIUtility.IconContent("d_ViewToolZoom").image;
            window.titleContent = new GUIContent(text: "Event Viewer", windowIcon);
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Event Viewer only works during Play mode.", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            Type registryType = typeof(Lando.EventWeaver.EventRegistry);
            FieldInfo listenersField = registryType.GetField("Listeners", BindingFlags.NonPublic | BindingFlags.Static);
            IDictionary<Type, List<object>> listenersDictionary = listenersField?.GetValue(null) as IDictionary<Type, List<object>>;

            if (listenersDictionary == null)
            {
                EditorGUILayout.HelpBox("Could not find EventRegistry.Listeners.", MessageType.Error);
            }
            else
            {
                Dictionary<string, List<(Type eventType, object listener)>> grouping = new Dictionary<string, List<(Type, object)>>();
                foreach ((Type eventType, List<object> list) in listenersDictionary)
                {
                    foreach (object listener in list)
                    {
                        Type type = listener.GetType();
                        string baseName = typeof(MonoBehaviour).IsAssignableFrom(type)
                            ? nameof(MonoBehaviour)
                            : typeof(EditorWindow).IsAssignableFrom(type)
                                ? nameof(EditorWindow)
                                : typeof(ScriptableObject).IsAssignableFrom(type)
                                    ? nameof(ScriptableObject)
                                    : type.BaseType?.Name ?? "<No Base>";

                        if (!grouping.ContainsKey(baseName)) grouping[baseName] = new List<(Type, object)>();
                        grouping[baseName].Add((eventType, listener));
                    }
                }

                foreach ((string baseName, List<(Type eventType, object listener)> items) in grouping.OrderBy(p => p.Key))
                {
                    bool baseExpanded = foldoutStates.TryGetValue(baseName, out bool baseState) && baseState;
                    DrawFoldout(baseIndent:0, ref baseExpanded, baseName, "d_Prefab Icon", new Color(0.2f, 0.2f, 0.2f));
                    foldoutStates[baseName] = baseExpanded;
                    if (!baseExpanded) continue;

                    foreach (IGrouping<Type, (Type eventType, object listener)> eventGroup in items.GroupBy(x => x.eventType))
                    {
                        string eventName = eventGroup.Key.Name;
                        bool eventExpanded = foldoutStates.TryGetValue(eventName, out bool eventState) && eventState;
                        DrawFoldout(baseIndent:1, ref eventExpanded, eventName, "d_UnityLogo", new Color(0.15f, 0.15f, 0.15f));
                        foldoutStates[eventName] = eventExpanded;
                        if (!eventExpanded) continue;

                        foreach ((Type _, object listener) in eventGroup)
                        {
                            DrawListenerRow(listener, indentLevel:2);
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFoldout(int baseIndent, ref bool expanded, string label, string iconName, Color backgroundColor)
        {
            float height = EditorGUIUtility.singleLineHeight + 4;
            Rect controlRect = EditorGUILayout.GetControlRect(false, height);
            controlRect.xMin += baseIndent * 15;

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUI.Box(controlRect, GUIContent.none);
            GUI.backgroundColor = originalBg;

            Rect foldRect = controlRect;
            foldRect.xMin += 4;
            Texture icon = EditorGUIUtility.IconContent(iconName).image;
            GUIContent content = new GUIContent(label, icon);
            expanded = EditorGUI.Foldout(foldRect, expanded, content, true);
        }

        private void DrawListenerRow(object listener, int indentLevel)
        {
            float height = EditorGUIUtility.singleLineHeight + 2;
            Rect controlRect = EditorGUILayout.GetControlRect(false, height);
            controlRect.xMin += indentLevel * 16;

            Component component = listener as Component;
            UnityEngine.Object unityObject = component != null ? (UnityEngine.Object)component : listener as UnityEngine.Object;
            string label = component != null
                ? component.gameObject.name + " (" + component.GetType().Name + ")"
                : listener.GetType().Name;

            Texture icon = unityObject != null
                ? EditorGUIUtility.ObjectContent(unityObject, unityObject.GetType()).image
                : EditorGUIUtility.IconContent("d_console.infoicon").image;
            GUIContent content = new GUIContent(label, icon);

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            GUI.Box(controlRect, GUIContent.none);
            GUI.backgroundColor = originalBg;

            if (GUI.Button(controlRect, content, EditorStyles.label) && unityObject != null)
            {
                EditorGUIUtility.PingObject(unityObject);
            }
        }
    }
}