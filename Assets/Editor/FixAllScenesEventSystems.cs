using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.IO;

[InitializeOnLoad]
public class FixAllScenesEventSystems
{
    static FixAllScenesEventSystems()
    {
        EditorApplication.delayCall += BatchFixAllScenes;
    }

    [MenuItem("Rocket Boost/Fix All Scenes EventSystems (Remove Legacy Input Errors)")]
    public static void BatchFixAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            FixSceneEventSystem(scenePath);
        }

        Debug.Log("Batch fixed EventSystems across all scenes!");
    }

    private static void FixSceneEventSystem(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath)) return;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>(true);

        bool modified = false;

        foreach (var es in eventSystems)
        {
            if (es == null) continue;

            StandaloneInputModule[] legacyModules = es.GetComponents<StandaloneInputModule>();
            foreach (var legacy in legacyModules)
            {
                if (legacy != null)
                {
                    Object.DestroyImmediate(legacy);
                    modified = true;
                }
            }

            InputSystemUIInputModule newModule = es.GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
                modified = true;
            }

            EventSystemFixer fixer = es.GetComponent<EventSystemFixer>();
            if (fixer == null)
            {
                es.gameObject.AddComponent<EventSystemFixer>();
                modified = true;
            }
        }

        if (modified)
        {
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Fixed and saved EventSystem in: " + scenePath);
        }
    }
}
