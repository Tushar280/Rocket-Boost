using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.IO;

[InitializeOnLoad]
public class MainMenuBuilder
{
    static MainMenuBuilder()
    {
        EditorApplication.delayCall += EnsureMainMenuBuilt;
    }

    [MenuItem("Rocket Boost/Rebuild Main Menu Scene & Canvas")]
    public static void RebuildMainMenuScene()
    {
        string scenePath = "Assets/Scenes/0MainMenu.unity";
        
        // Open or Create 0MainMenu scene
        Scene scene;
        if (File.Exists(scenePath))
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        // Clean existing Canvas & EventSystem
        Canvas existingCanvas = Object.FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Object.DestroyImmediate(existingCanvas.gameObject);
        }

        EventSystem existingEventSystem = Object.FindObjectOfType<EventSystem>();
        if (existingEventSystem != null)
        {
            Object.DestroyImmediate(existingEventSystem.gameObject);
        }

        // Ensure MainMenuManager GameObject
        MainMenuManager manager = Object.FindObjectOfType<MainMenuManager>();
        if (manager == null)
        {
            GameObject mgrObj = new GameObject("MainMenuManager");
            manager = mgrObj.AddComponent<MainMenuManager>();
            mgrObj.AddComponent<SettingsManager>();
        }

        // Build Space UI using MainMenuUIBuilder
        MainMenuUIBuilder.EnsureUI(manager);

        // Save Scene
        EditorSceneManager.SaveScene(scene, scenePath);
        RegisterInBuildSettings(scenePath);

        Debug.Log("Successfully built Space Main Menu Canvas into " + scenePath);
    }

    private static void EnsureMainMenuBuilt()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "0MainMenu")
        {
            if (Object.FindObjectOfType<Canvas>() == null)
            {
                RebuildMainMenuScene();
            }
        }
    }

    private static void RegisterInBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        bool exists = false;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) { exists = true; break; }
        }

        if (!exists)
        {
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < scenes.Length; i++)
            {
                newScenes[i + 1] = scenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
