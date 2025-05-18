using System.Collections.Generic;
using System.Linq;
using Lando.EventWeaver.Events;
using UnityEditor;
using UnityEngine;

namespace Lando.EventWeaver.Editor.History
{
    public class EventHistoryWindow : EditorWindow,
        IEventListener<EventRaised>
    {
        private Vector2 _scrollPosition;
        private GUIStyle _evenRowStyle;
        private GUIStyle _oddRowStyle;
        private GUIStyle _iconStyle;

        private readonly Color _evenRowColor = new(0.247f, 0.247f, 0.247f);
        private readonly Color _oddRowColor = new(0.219f, 0.219f, 0.219f);
        
        private static Texture2D _evenRowTexture;
        private static Texture2D _oddRowTexture;

        [MenuItem("Tools/Lando/Event Weaver/Event History")]
        public static void ShowWindow()
        {
            EventHistoryWindow window = GetWindow<EventHistoryWindow>();
            Texture windowIcon = EditorGUIUtility.IconContent(name: "console.infoicon.sml").image;
            window.titleContent = new GUIContent(text: "Event History", windowIcon);
            
            EventRegistry.Register(window);
        }

        private void OnEnable()
        {
            InitializeTextures();
        }
        
        private void OnDisable() => EventRegistry.Unregister(listener: this);

        private void OnGUI()
        {
            InitializeStyles();
            DisplayToolbar();
            DisplayLogMessages();
        }
        
        private void InitializeTextures()
        {
            if (_evenRowTexture == null)
                _evenRowTexture = MakeTex(1, 1, _evenRowColor);

            if (_oddRowTexture == null)
                _oddRowTexture = MakeTex(1, 1, _oddRowColor);
        }

        private void InitializeStyles()
        {
            if(EditorStyles.label == null)
                return;
            
            _evenRowStyle ??= new GUIStyle(EditorStyles.label)
            {
                normal = { background = MakeTex(1, 1, new Color(0.18f, 0.18f, 0.18f)) },
                padding = new RectOffset(5, 5, 2, 2),
                fontSize = 12,
                wordWrap = true
            };

            _oddRowStyle ??= new GUIStyle(EditorStyles.label)
            {
                normal = { background = MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f)) },
                padding = new RectOffset(5, 5, 2, 2),
                fontSize = 12,
                wordWrap = true
            };

            _iconStyle ??= new GUIStyle(EditorStyles.label)
            {
                fixedWidth = 20,
                fixedHeight = 20,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void DisplayToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                EventHistory.Clear();

            GUILayout.FlexibleSpace();

            List<EventLog> logs = EventHistory.GetLogs();
            int registerCount = logs.Count(log => log.Type == EventLogType.Register);
            int raiseCount = logs.Count(log => log.Type == EventLogType.Raise);
            int unregisterCount = logs.Count(log => log.Type == EventLogType.Unregister);

            GUILayoutOption buttonWidth = GUILayout.Width(40);

            EventHistory.ShowRegister = GUILayout.Toggle(
                EventHistory.ShowRegister,
                new GUIContent(registerCount.ToString(), GetIcon(EventLogType.Register)),
                EditorStyles.toolbarButton,
                buttonWidth
            );

            EventHistory.ShowRaise = GUILayout.Toggle(
                EventHistory.ShowRaise,
                new GUIContent(raiseCount.ToString(), GetIcon(EventLogType.Raise)),
                EditorStyles.toolbarButton,
                buttonWidth
            );

            EventHistory.ShowUnregister = GUILayout.Toggle(
                EventHistory.ShowUnregister,
                new GUIContent(unregisterCount.ToString(), GetIcon(EventLogType.Unregister)),
                EditorStyles.toolbarButton,
                buttonWidth
            );

            GUILayout.EndHorizontal();
        }

        private void DisplayLogMessages()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            List<EventLog> filteredLogs = EventHistory.GetFilteredLogs();
            for (int i = 0; i < filteredLogs.Count; i++)
            {
                Color rowColor = filteredLogs.Count < 1 || i % 2 > 0 ? _evenRowColor : _oddRowColor;

                GUIStyle rowStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { background = MakeTex(1, 1, rowColor) },
                    padding = new RectOffset(5, 5, 2, 2),
                    fontSize = 12,
                    wordWrap = true
                };

                DisplayLogEntry(filteredLogs[i], rowStyle);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DisplayLogEntry(EventLog entry, GUIStyle style)
        {
            GUILayout.BeginHorizontal(style);

            GUIContent iconContent = new GUIContent(GetIcon(entry.Type));
            GUILayout.Label(iconContent, _iconStyle);

            GUILayout.Label($"[{entry.Timestamp:HH:mm:ss}] {entry.Message}");

            GUILayout.EndHorizontal();
        }

        private Texture2D GetIcon(EventLogType type)
        {
            return type switch
            {
                EventLogType.Register => EditorGUIUtility.IconContent("d_Toolbar Plus").image as Texture2D,
                EventLogType.Raise => EditorGUIUtility.IconContent("d_EventSystem Icon").image as Texture2D,
                EventLogType.Unregister => EditorGUIUtility.IconContent("d_Toolbar Minus").image as Texture2D,
                _ => EditorGUIUtility.IconContent("d_console.infoicon").image as Texture2D
            };
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public void OnListenedTo(EventRaised e) => Repaint();
    }
}
