using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuSceneCreator
{
    [MenuItem("Rocket Boost/Create & Register Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        string sceneDirPath = "Assets/Scenes";
        string scenePath = sceneDirPath + "/0MainMenu.unity";

        if (!Directory.Exists(sceneDirPath))
        {
            Directory.CreateDirectory(sceneDirPath);
        }

        // Create new empty scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add MainMenuManager component
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuManager manager = managerObj.AddComponent<MainMenuManager>();
        managerObj.AddComponent<SettingsManager>();

        // Build Space UI
        MainMenuUIBuilder.EnsureUI(manager);

        // Save Scene
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log("Successfully created Main Menu Scene at: " + scenePath);

        // Register in Build Settings at index 0
        RegisterSceneInBuildSettings(scenePath);
    }

    public static void RegisterSceneInBuildSettings(string newScenePath)
    {
        EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
        bool alreadyPresent = false;

        foreach (var scene in existingScenes)
        {
            if (scene.path == newScenePath)
            {
                alreadyPresent = true;
                break;
            }
        }

        if (!alreadyPresent)
        {
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[existingScenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(newScenePath, true);

            for (int i = 0; i < existingScenes.Length; i++)
            {
                newScenes[i + 1] = existingScenes[i];
            }

            EditorBuildSettings.scenes = newScenes;
            Debug.Log("Registered " + newScenePath + " as Scene 0 in Build Settings!");
        }
    }
}
