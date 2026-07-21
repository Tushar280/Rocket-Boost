using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

        // Clean existing Canvas & EventSystem to avoid duplicate overlays
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

        // Create Canvas & UI Hierarchy
        BuildSpaceUIInScene(manager);

        // Save Scene
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
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

    private static void BuildSpaceUIInScene(MainMenuManager manager)
    {
        // 1. Create EventSystem
        GameObject eventSysObj = new GameObject("EventSystem");
        eventSysObj.AddComponent<EventSystem>();
        eventSysObj.AddComponent<StandaloneInputModule>();

        // 2. Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Create Space Atmosphere Background (Teal/Cyan curve gradient matching reference image)
        GameObject bgObj = CreateRectElement("SpaceBackground", canvasObj.transform);
        StretchToFill(bgObj.GetComponent<RectTransform>());
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.06f, 0.1f, 1.0f); // Deep space dark teal atmosphere

        // Subtle gradient overlay panel
        GameObject gradObj = CreateRectElement("AtmosphereGradient", bgObj.transform);
        StretchToFill(gradObj.GetComponent<RectTransform>());
        Image gradImg = gradObj.AddComponent<Image>();
        gradImg.color = new Color(0.15f, 0.35f, 0.45f, 0.35f); // Ethereal teal light curve

        // 4. Main Menu Card (Center container)
        GameObject mainPanel = CreateRectElement("MainPanel", canvasObj.transform);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.5f, 0.5f);
        mainRect.anchorMax = new Vector2(0.5f, 0.5f);
        mainRect.pivot = new Vector2(0.5f, 0.5f);
        mainRect.sizeDelta = new Vector2(500, 600);

        VerticalLayoutGroup mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
        mainLayout.padding = new RectOffset(20, 20, 30, 30);
        mainLayout.spacing = 20;
        mainLayout.childAlignment = TextAnchor.MiddleCenter;
        mainLayout.childControlWidth = true;
        mainLayout.childControlHeight = false;

        // Title
        GameObject titleObj = CreateRectElement("TitleText", mainPanel.transform);
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 70);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ROCKET BOOST";
        titleText.fontSize = 42;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.7f, 0.9f, 1.0f, 1.0f);
        titleText.fontStyle = FontStyle.Bold;

        // Subtitle
        GameObject subObj = CreateRectElement("SubtitleText", mainPanel.transform);
        subObj.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 30);
        Text subText = subObj.AddComponent<Text>();
        subText.text = "SPACE EXPLORER";
        subText.fontSize = 16;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.4f, 0.7f, 0.85f, 0.8f);

        // Buttons matching sleek reference style (Play, Controls, Options, Exit)
        Button playBtn = CreateSpaceButton("PlayBtn", "Play", mainPanel.transform, manager.PlayGame);
        Button controlsBtn = CreateSpaceButton("ControlsBtn", "Controls", mainPanel.transform, manager.OpenControls);
        Button optionsBtn = CreateSpaceButton("OptionsBtn", "Options", mainPanel.transform, manager.OpenOptions);
        Button exitBtn = CreateSpaceButton("ExitBtn", "Exit", mainPanel.transform, manager.QuitGame);

        // 5. Controls Panel
        GameObject controlsPanel = CreateRectElement("ControlsPanel", canvasObj.transform);
        RectTransform ctrlRect = controlsPanel.GetComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(0.5f, 0.5f);
        ctrlRect.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRect.pivot = new Vector2(0.5f, 0.5f);
        ctrlRect.sizeDelta = new Vector2(550, 580);

        Image ctrlBg = controlsPanel.AddComponent<Image>();
        ctrlBg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

        VerticalLayoutGroup ctrlLayout = controlsPanel.AddComponent<VerticalLayoutGroup>();
        ctrlLayout.padding = new RectOffset(30, 30, 30, 30);
        ctrlLayout.spacing = 18;
        ctrlLayout.childAlignment = TextAnchor.UpperCenter;
        ctrlLayout.childControlWidth = true;
        ctrlLayout.childControlHeight = false;

        GameObject ctrlTitle = CreateRectElement("CtrlTitle", controlsPanel.transform);
        ctrlTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 50);
        Text ctrlTitleText = ctrlTitle.AddComponent<Text>();
        ctrlTitleText.text = "FLIGHT CONTROLS";
        ctrlTitleText.fontSize = 28;
        ctrlTitleText.alignment = TextAnchor.MiddleCenter;
        ctrlTitleText.color = new Color(0.7f, 0.9f, 1.0f);
        ctrlTitleText.fontStyle = FontStyle.Bold;

        CreateControlRow("Thrust Rocket", "SPACE / W", controlsPanel.transform);
        CreateControlRow("Steer Left", "A / LEFT ARROW", controlsPanel.transform);
        CreateControlRow("Steer Right", "D / RIGHT ARROW", controlsPanel.transform);
        CreateControlRow("Pause Game", "ESCAPE", controlsPanel.transform);

        Button backCtrlBtn = CreateSpaceButton("BackCtrlBtn", "Back to Menu", controlsPanel.transform, manager.ShowMainPanel);

        // 6. Options Panel
        GameObject optionsPanel = CreateRectElement("OptionsPanel", canvasObj.transform);
        RectTransform optRect = optionsPanel.GetComponent<RectTransform>();
        optRect.anchorMin = new Vector2(0.5f, 0.5f);
        optRect.anchorMax = new Vector2(0.5f, 0.5f);
        optRect.pivot = new Vector2(0.5f, 0.5f);
        optRect.sizeDelta = new Vector2(550, 580);

        Image optBg = optionsPanel.AddComponent<Image>();
        optBg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

        VerticalLayoutGroup optLayout = optionsPanel.AddComponent<VerticalLayoutGroup>();
        optLayout.padding = new RectOffset(30, 30, 30, 30);
        optLayout.spacing = 18;
        optLayout.childAlignment = TextAnchor.UpperCenter;
        optLayout.childControlWidth = true;
        optLayout.childControlHeight = false;

        GameObject optTitle = CreateRectElement("OptTitle", optionsPanel.transform);
        optTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 50);
        Text optTitleText = optTitle.AddComponent<Text>();
        optTitleText.text = "SETTINGS";
        optTitleText.fontSize = 28;
        optTitleText.alignment = TextAnchor.MiddleCenter;
        optTitleText.color = new Color(0.7f, 0.9f, 1.0f);
        optTitleText.fontStyle = FontStyle.Bold;

        Slider masterSlider = CreateSliderRow("Master Volume", optionsPanel.transform, manager.SetMasterVolume);
        Slider musicSlider = CreateSliderRow("Music Volume", optionsPanel.transform, manager.SetMusicVolume);
        Slider sfxSlider = CreateSliderRow("SFX Volume", optionsPanel.transform, manager.SetSFXVolume);

        Button backOptBtn = CreateSpaceButton("BackOptBtn", "Back to Menu", optionsPanel.transform, manager.ShowMainPanel);

        // Assign to manager serialized private fields
        SetPrivateField(manager, "mainPanel", mainPanel);
        SetPrivateField(manager, "controlsPanel", controlsPanel);
        SetPrivateField(manager, "optionsPanel", optionsPanel);
        SetPrivateField(manager, "masterVolumeSlider", masterSlider);
        SetPrivateField(manager, "musicVolumeSlider", musicSlider);
        SetPrivateField(manager, "sfxVolumeSlider", sfxSlider);

        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);

        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(canvasObj);
    }

    private static GameObject CreateRectElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static Button CreateSpaceButton(string name, string labelText, Transform parent, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateRectElement(name, parent);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 52);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.24f, 0.35f, 0.5f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.12f, 0.24f, 0.35f, 0.5f);
        cb.highlightedColor = new Color(0.2f, 0.5f, 0.7f, 0.9f);
        cb.pressedColor = new Color(0.0f, 0.8f, 1.0f, 1.0f);
        btn.colors = cb;

        GameObject txtObj = CreateRectElement("Text", btnObj.transform);
        StretchToFill(txtObj.GetComponent<RectTransform>());
        Text txt = txtObj.AddComponent<Text>();
        txt.text = labelText;
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.85f, 0.94f, 1.0f);
        txt.fontStyle = FontStyle.Normal;

        if (onClick != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, onClick);
        }

        return btn;
    }

    private static void CreateControlRow(string action, string keys, Transform parent)
    {
        GameObject row = CreateRectElement(action + "_Row", parent);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 44);

        Image rowImg = row.AddComponent<Image>();
        rowImg.color = new Color(0.1f, 0.2f, 0.3f, 0.4f);

        GameObject actObj = CreateRectElement("Action", row.transform);
        Text actTxt = actObj.AddComponent<Text>();
        actTxt.text = action;
        actTxt.fontSize = 18;
        actTxt.color = new Color(0.85f, 0.95f, 1.0f);
        actTxt.alignment = TextAnchor.MiddleLeft;
        RectTransform actRect = actObj.GetComponent<RectTransform>();
        actRect.anchorMin = new Vector2(0.05f, 0);
        actRect.anchorMax = new Vector2(0.45f, 1);

        GameObject keyObj = CreateRectElement("Keys", row.transform);
        Text keyTxt = keyObj.AddComponent<Text>();
        keyTxt.text = keys;
        keyTxt.fontSize = 16;
        keyTxt.fontStyle = FontStyle.Bold;
        keyTxt.color = new Color(0.3f, 0.85f, 1.0f);
        keyTxt.alignment = TextAnchor.MiddleRight;
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0);
        keyRect.anchorMax = new Vector2(0.95f, 1);
    }

    private static Slider CreateSliderRow(string labelText, Transform parent, UnityEngine.Events.UnityAction<float> callback)
    {
        GameObject container = CreateRectElement(labelText + "_Container", parent);
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 48);

        GameObject lblObj = CreateRectElement("Label", container.transform);
        Text txt = lblObj.AddComponent<Text>();
        txt.text = labelText;
        txt.fontSize = 18;
        txt.color = new Color(0.85f, 0.95f, 1.0f);
        txt.alignment = TextAnchor.MiddleLeft;
        RectTransform lblRect = lblObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0.02f, 0);
        lblRect.anchorMax = new Vector2(0.45f, 1);

        GameObject sliderObj = CreateRectElement("Slider", container.transform);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.48f, 0.25f);
        sliderRect.anchorMax = new Vector2(0.98f, 0.75f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        GameObject bg = CreateRectElement("Background", sliderObj.transform);
        StretchToFill(bg.GetComponent<RectTransform>());
        bg.AddComponent<Image>().color = new Color(0.1f, 0.2f, 0.35f, 0.8f);

        GameObject fillArea = CreateRectElement("Fill Area", sliderObj.transform);
        StretchToFill(fillArea.GetComponent<RectTransform>());

        GameObject fill = CreateRectElement("Fill", fillArea.transform);
        StretchToFill(fill.GetComponent<RectTransform>());
        fill.AddComponent<Image>().color = new Color(0.2f, 0.8f, 1.0f, 1.0f);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fill.GetComponent<Image>();

        if (callback != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(slider.onValueChanged, callback);
        }

        return slider;
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
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

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
