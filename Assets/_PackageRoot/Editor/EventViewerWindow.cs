using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// This window lists events grouped by subscriber base class with foldouts and icons
public class EventViewerWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
    private GUIStyle _headerStyle;

    [MenuItem("Tools/Lando/Events/Event Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<EventViewerWindow>("Event Viewer");
        window.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        // Lazy header style initialization
        if (_headerStyle == null)
        {
            GUIStyle baseStyle = EditorStyles.label ?? new GUIStyle(GUI.skin.label);
            _headerStyle = new GUIStyle(baseStyle)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal = { textColor = Color.cyan }
            };
        }

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Event Viewer only works during Play mode.", MessageType.Warning);
            return;
        }

        GUILayout.Label("Registered Events by Subscriber Base Class", _headerStyle);
        EditorGUILayout.Space();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        // Reflect into EventBus Listeners
        var busType = typeof(Lando.Events.EventBus);
        var field = busType.GetField("Listeners", BindingFlags.NonPublic | BindingFlags.Static);
        var dict = field?.GetValue(null) as IDictionary<Type, List<object>>;

        if (dict == null)
        {
            EditorGUILayout.HelpBox("Could not find or parse EventBus.Listeners.", MessageType.Error);
        }
        else
        {
            // Group subscribers by base-most known Unity type
            var grouping = new Dictionary<string, List<(Type eventType, object listener)>>();

            foreach (var kvp in dict)
            {
                var eventType = kvp.Key;
                foreach (var listener in kvp.Value)
                {
                    var listenerType = listener.GetType();
                    string baseName;

                    if (typeof(MonoBehaviour).IsAssignableFrom(listenerType))
                        baseName = nameof(MonoBehaviour);
                    else if (typeof(EditorWindow).IsAssignableFrom(listenerType))
                        baseName = nameof(EditorWindow);
                    else if (typeof(ScriptableObject).IsAssignableFrom(listenerType))
                        baseName = nameof(ScriptableObject);
                    else
                        baseName = listenerType.BaseType?.Name ?? "<No Base>";

                    if (!grouping.ContainsKey(baseName))
                        grouping[baseName] = new List<(Type, object)>();

                    grouping[baseName].Add((eventType, listener));
                }
            }

            // Render each base class group
            foreach (var group in grouping.OrderBy(g => g.Key))
            {
                string baseName = group.Key;
                bool groupExpanded = _foldoutStates.TryGetValue(baseName, out bool gVal) && gVal;
                groupExpanded = EditorGUILayout.Foldout(groupExpanded, baseName, true);
                _foldoutStates[baseName] = groupExpanded;

                if (!groupExpanded)
                    continue;

                EditorGUI.indentLevel++;

                // Within group, events by type
                var events = group.Value.GroupBy(x => x.eventType);
                foreach (var evGroup in events)
                {
                    string evName = evGroup.Key.Name;
                    bool evExpanded = _foldoutStates.TryGetValue(evName, out bool eVal) && eVal;
                    evExpanded = EditorGUILayout.Foldout(evExpanded, evName, true);
                    _foldoutStates[evName] = evExpanded;

                    if (!evExpanded)
                        continue;

                    EditorGUI.indentLevel++;

                    // List each listener
                    foreach (var (_, listener) in evGroup)
                    {
                        var comp = listener as Component;
                        var unityObj = comp != null ? comp : listener as UnityEngine.Object;
                        string label = comp != null
                            ? $"{comp.gameObject.name} ({comp.GetType().Name})"
                            : listener.GetType().Name;

                        GUIContent content = unityObj != null
                            ? EditorGUIUtility.ObjectContent(unityObj, unityObj.GetType())
                            : new GUIContent(label);

                        var style = new GUIStyle(EditorStyles.label) { richText = true };
                        float lineHeight = EditorGUIUtility.singleLineHeight;
                        Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
                        rect = EditorGUI.IndentedRect(rect);

                        if (GUI.Button(rect, content, style) && unityObj != null)
                            EditorGUIUtility.PingObject(unityObj);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
