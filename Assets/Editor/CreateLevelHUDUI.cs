#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CreateLevelHUDUI : EditorWindow
{
    [MenuItem("Tools/Rocket Boost/Build In-Game HUD UI for All Levels")]
    public static void GenerateHUDForAllLevels()
    {
        Debug.Log("Starting In-Game HUD UI Generation for all levels...");

        string[] levelScenes = new string[]
        {
            "Assets/Scenes/1Mercury.unity",
            "Assets/Scenes/2Venus.unity",
            "Assets/Scenes/3Mars.unity",
            "Assets/Scenes/4jupiter.unity",
            "Assets/Scenes/5Neptune.unity"
        };

        foreach (string scenePath in levelScenes)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning("Scene not found: " + scenePath);
                continue;
            }

            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            Debug.Log("Building HUD for scene: " + scene.name);

            // 1. Clean old HUD elements
            CleanUpOldHUD();

            // 2. Create Event System
            CreateEventSystem();

            // 3. Create Manager & Canvas
            GameObject managerObj = new GameObject("LevelHUDManager");
            LevelHUDManager hudManager = managerObj.AddComponent<LevelHUDManager>();

            Canvas canvas = CreateCanvas();
            CreateHUDHierarchy(canvas.transform, hudManager, scene.name);

            // 4. Save Scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("In-Game HUD UI successfully generated across all levels!");
    }

    private static void CleanUpOldHUD()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases) DestroyImmediate(c.gameObject);

        var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var es in eventSystems) DestroyImmediate(es.gameObject);

        var managers = Object.FindObjectsByType<LevelHUDManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in managers) DestroyImmediate(m.gameObject);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSysObj = new GameObject("EventSystem");
        eventSysObj.AddComponent<EventSystem>();
        eventSysObj.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private static void CreateHUDHierarchy(Transform canvasTransform, LevelHUDManager manager, string sceneName)
    {
        // 1. Top HUD Bar
        GameObject topBar = CreateUIElement("TopHUDBar", canvasTransform);
        RectTransform topRT = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 1);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.pivot = new Vector2(0.5f, 1);
        topRT.anchoredPosition = new Vector2(0, 0);
        topRT.sizeDelta = new Vector2(0, 80);

        // Level Title Text (Top Left)
        CreateText("LevelIndicator", sceneName.ToUpper(), topBar.transform, new Vector2(140, -40), 26, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

        // Pause Button (Top Right)
        GameObject pauseBtnObj = CreateUIElement("PauseBtn", topBar.transform);
        RectTransform pauseBtnRT = pauseBtnObj.GetComponent<RectTransform>();
        pauseBtnRT.anchorMin = new Vector2(1, 0.5f);
        pauseBtnRT.anchorMax = new Vector2(1, 0.5f);
        pauseBtnRT.pivot = new Vector2(1, 0.5f);
        pauseBtnRT.anchoredPosition = new Vector2(-40, 0);
        pauseBtnRT.sizeDelta = new Vector2(160, 48);

        Image pauseImg = pauseBtnObj.AddComponent<Image>();
        pauseImg.color = new Color(1f, 1f, 1f, 0.15f);
        pauseImg.raycastTarget = true;

        Button pauseBtn = pauseBtnObj.AddComponent<Button>();
        ColorBlock colors = pauseBtn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.15f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.3f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.5f);
        pauseBtn.colors = colors;

        GameObject pauseTxtObj = CreateUIElement("PauseText", pauseBtnObj.transform);
        StretchToFill(pauseTxtObj.GetComponent<RectTransform>());
        Text pauseTxt = pauseTxtObj.AddComponent<Text>();
        pauseTxt.text = "PAUSE";
        pauseTxt.fontSize = 20;
        pauseTxt.fontStyle = FontStyle.Bold;
        pauseTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pauseTxt.alignment = TextAnchor.MiddleCenter;
        pauseTxt.color = Color.white;
        pauseTxt.raycastTarget = false;

        UnityEventTools.AddPersistentListener(pauseBtn.onClick, manager.PauseGame);

        // 2. Pause Panel Overlay
        GameObject pausePanel = CreateOverlayPanel("PauseMenuPanel", canvasTransform, "GAME PAUSED", "Press ESC or Resume to continue");
        Button resumeBtn = CreateButton("ResumeBtn", "Resume", pausePanel.transform, new Vector2(0, 40), true);
        Button restartPauseBtn = CreateButton("RestartPauseBtn", "Restart Level", pausePanel.transform, new Vector2(0, -30), false);
        Button menuPauseBtn = CreateButton("MenuPauseBtn", "Main Menu", pausePanel.transform, new Vector2(0, -100), false);

        UnityEventTools.AddPersistentListener(resumeBtn.onClick, manager.ResumeGame);
        UnityEventTools.AddPersistentListener(restartPauseBtn.onClick, manager.RestartLevel);
        UnityEventTools.AddPersistentListener(menuPauseBtn.onClick, manager.ReturnToMainMenu);
        manager.pauseMenuPanel = pausePanel;

        // 3. Level Complete Panel Overlay
        GameObject completePanel = CreateOverlayPanel("LevelCompletePanel", canvasTransform, "MISSION ACCOMPLISHED!", "Level Complete!");
        Button nextLvlBtn = CreateButton("NextLevelBtn", "Next Level", completePanel.transform, new Vector2(0, 40), true);
        Button replayBtn = CreateButton("ReplayBtn", "Replay Level", completePanel.transform, new Vector2(0, -30), false);
        Button menuCompleteBtn = CreateButton("MenuCompleteBtn", "Main Menu", completePanel.transform, new Vector2(0, -100), false);

        UnityEventTools.AddPersistentListener(nextLvlBtn.onClick, manager.LoadNextLevel);
        UnityEventTools.AddPersistentListener(replayBtn.onClick, manager.RestartLevel);
        UnityEventTools.AddPersistentListener(menuCompleteBtn.onClick, manager.ReturnToMainMenu);
        manager.levelCompletePanel = completePanel;

        // 4. Game Over Panel Overlay
        GameObject gameOverPanel = CreateOverlayPanel("GameOverPanel", canvasTransform, "CRASHED!", "Rocket Destroyed");
        Button retryBtn = CreateButton("RetryBtn", "Retry", gameOverPanel.transform, new Vector2(0, 30), true);
        Button menuGameOverBtn = CreateButton("MenuGameOverBtn", "Main Menu", gameOverPanel.transform, new Vector2(0, -40), false);

        UnityEventTools.AddPersistentListener(retryBtn.onClick, manager.RestartLevel);
        UnityEventTools.AddPersistentListener(menuGameOverBtn.onClick, manager.ReturnToMainMenu);
        manager.gameOverPanel = gameOverPanel;
    }

    private static GameObject CreateOverlayPanel(string name, Transform parent, string titleText, string subtitleText)
    {
        GameObject panelObj = CreateUIElement(name, parent);
        panelObj.SetActive(false);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(500, 520);

        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);
        img.raycastTarget = false;

        CreateText("Title", titleText, panelObj.transform, new Vector2(0, 180), 38, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        CreateText("Subtitle", subtitleText, panelObj.transform, new Vector2(0, 135), 18, FontStyle.Normal, new Color(0.85f, 0.85f, 0.95f, 0.9f), TextAnchor.MiddleCenter);

        return panelObj;
    }

    private static Button CreateButton(string name, string text, Transform parent, Vector2 pos, bool isHighlight)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(250, 50);

        Image img = btnObj.AddComponent<Image>();
        img.color = isHighlight ? new Color(1f, 1f, 1f, 0.25f) : new Color(1f, 1f, 1f, 0.08f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = isHighlight ? new Color(1f, 1f, 1f, 0.25f) : new Color(1f, 1f, 1f, 0.08f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.35f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.5f);
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;

        GameObject txtObj = CreateUIElement(name + "_Text", btnObj.transform);
        StretchToFill(txtObj.GetComponent<RectTransform>());

        Text txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = 22;
        txt.fontStyle = FontStyle.Normal;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.raycastTarget = false;

        return btn;
    }

    private static void CreateText(string name, string content, Transform parent, Vector2 pos, int fontSize, FontStyle style, Color color, TextAnchor alignment)
    {
        GameObject txtObj = CreateUIElement(name, parent);
        txtObj.GetComponent<RectTransform>().anchoredPosition = pos;
        txtObj.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 80);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = alignment;
        txt.color = color;
        txt.raycastTarget = false;
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
#endif
