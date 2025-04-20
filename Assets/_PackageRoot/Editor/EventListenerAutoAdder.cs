using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lando.Events.Editor
{
    [InitializeOnLoad]
    public static class EventListenerAutoAdder
    {
        static EventListenerAutoAdder()
        {
            EditorApplication.delayCall += OnScriptsReloaded;
        }

        private static void OnScriptsReloaded()
        {
            if(EditorApplication.isPlaying == true || EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                EditorApplication.delayCall += OnScriptsReloaded;
                return;
            }
            AddListenersInScenes();
            AddListenersInPrefabs();
            EditorSceneManager.MarkAllScenesDirty();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
        }

        private static void AddListenersInScenes()
        {
            int sceneCount = EditorSceneManager.sceneCount;
            for(int i = 0; i < sceneCount; i = i + 1)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                if(scene.isLoaded == false)
                {
                    continue;
                }
                foreach(GameObject root in scene.GetRootGameObjects())
                {
                    MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
                    for(int j = 0; j < components.Length; j = j + 1)
                    {
                        MonoBehaviour comp = components[j];
                        if(comp == null)
                        {
                            continue;
                        }
                        Type[] interfaces = comp.GetType().GetInterfaces();
                        bool hasListenerInterface = interfaces.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEventListener<>));
                        if(hasListenerInterface == false)
                        {
                            continue;
                        }
                        if(comp.gameObject.GetComponent<EventListener>() == null)
                        {
                            comp.gameObject.AddComponent<EventListener>();
                        }
                    }
                }
            }
        }

        private static void AddListenersInPrefabs()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            for(int i = 0; i < prefabGuids.Length; i = i + 1)
            {
                string guid = prefabGuids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                bool modified = false;
                MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
                for(int j = 0; j < components.Length; j = j + 1)
                {
                    MonoBehaviour comp = components[j];
                    if(comp == null)
                    {
                        continue;
                    }
                    Type[] interfaces = comp.GetType().GetInterfaces();
                    bool hasListenerInterface = interfaces.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEventListener<>));
                    if(hasListenerInterface == false)
                    {
                        continue;
                    }
                    if(comp.gameObject.GetComponent<EventListener>() == null)
                    {
                        comp.gameObject.AddComponent<EventListener>();
                        modified = true;
                    }
                }
                if(modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}