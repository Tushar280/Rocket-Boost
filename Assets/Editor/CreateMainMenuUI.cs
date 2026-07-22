#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CreateMainMenuUI : EditorWindow
{
    [MenuItem("Tools/Rocket Boost/Build Clean Main Menu")]
    public static void GenerateCleanMenu()
    {
        Debug.Log("Starting Clean Main Menu Generation...");

        // Ensure active scene is 0MainMenu.unity
        string scenePath = "Assets/Scenes/0MainMenu.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

        // 1. Clean up old elements thoroughly
        CleanUpOldMenu();

        // 2. Import the background image as a Sprite
        Sprite bgSprite = ImportBackgroundSprite();

        // 3. Create Event System
        CreateEventSystem();

        // 4. Create MainMenuManager & SettingsManager objects
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuManager mainMenuManager = managerObj.AddComponent<MainMenuManager>();
        managerObj.AddComponent<SettingsManager>();

        // 5. Create UI Canvas hierarchy
        Canvas canvas = CreateCanvas();
        CreateUIHierarchy(canvas.transform, bgSprite, mainMenuManager);

        // 6. Save Scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("Main Menu successfully created and saved to 0MainMenu.unity!");
    }

    private static void CleanUpOldMenu()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases) DestroyImmediate(c.gameObject);

        var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var es in eventSystems) DestroyImmediate(es.gameObject);

        var managers = Object.FindObjectsByType<MainMenuManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in managers) DestroyImmediate(m.gameObject);
    }

    private static Sprite ImportBackgroundSprite()
    {
        string sourcePath = @"C:\Users\Tushar\Downloads\1123556.png";
        string destFolder = "Assets/Textures";
        string destPath = destFolder + "/MainMenuBackground.png";

        if (System.IO.File.Exists(sourcePath))
        {
            if (!System.IO.Directory.Exists(destFolder))
            {
                System.IO.Directory.CreateDirectory(destFolder);
            }
            try
            {
                System.IO.File.Copy(sourcePath, destPath, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("File copy warning: " + ex.Message);
            }
        }

        if (!System.IO.File.Exists(destPath))
        {
            Debug.LogError("Background texture file does not exist at: " + destPath);
            return null;
        }

        try
        {
            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(destPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
            if (sprite == null)
            {
                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(destPath);
                foreach (var sub in subAssets)
                {
                    if (sub is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            if (sprite == null)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
                if (tex != null)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    Debug.Log("Created Sprite fallback from Texture2D!");
                }
            }

            return sprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to import background image: " + ex.Message);
            return null;
        }
    }

    private static void CreateEventSystem()
    {
        GameObject eventSysObj = new GameObject("EventSystem");
        eventSysObj.AddComponent<EventSystem>();
        eventSysObj.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private static void CreateUIHierarchy(Transform canvasTransform, Sprite bgSprite, MainMenuManager manager)
    {
        // 1. Background Image
        GameObject bgObj = CreateUIElement("Background", canvasTransform);
        Image bgImg = bgObj.AddComponent<Image>();
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }
        else
        {
            bgImg.color = new Color(0.05f, 0.05f, 0.12f, 1f);
        }
        bgImg.raycastTarget = false;
        StretchToFill(bgObj.GetComponent<RectTransform>());

        // 2. Main Container Panel (Reference image centered box)
        GameObject mainPanel = CreateUIElement("MainPanel", canvasTransform);
        RectTransform mainRT = mainPanel.GetComponent<RectTransform>();
        mainRT.anchoredPosition = Vector2.zero;
        mainRT.sizeDelta = new Vector2(480, 640);

        Image panelImg = mainPanel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.08f, 0.72f);
        panelImg.raycastTarget = false;

        // Header: ROCKET BOOST
        CreateText("Title", "ROCKET BOOST", mainPanel.transform, new Vector2(0, 220), 48, FontStyle.Bold, Color.white);

        // Subtitle: GALAXY EXPLORER
        CreateText("Subtitle", "GALAXY EXPLORER", mainPanel.transform, new Vector2(0, 170), 16, FontStyle.Normal, new Color(0.85f, 0.85f, 0.95f, 0.9f));

        // Buttons
        Button playBtn = CreateButton("PlayBtn", "Play", mainPanel.transform, new Vector2(0, 60), true);
        Button controlsBtn = CreateButton("ControlsBtn", "Controls", mainPanel.transform, new Vector2(0, -10), false);
        Button optionsBtn = CreateButton("OptionsBtn", "Options", mainPanel.transform, new Vector2(0, -80), false);
        Button exitBtn = CreateButton("ExitBtn", "Exit", mainPanel.transform, new Vector2(0, -150), false);

        manager.mainPanel = mainPanel;

        // Persistent Button Events
        UnityEventTools.AddPersistentListener(playBtn.onClick, manager.OpenLevelSelect);
        UnityEventTools.AddPersistentListener(controlsBtn.onClick, manager.OpenControls);
        UnityEventTools.AddPersistentListener(optionsBtn.onClick, manager.OpenOptions);
        UnityEventTools.AddPersistentListener(exitBtn.onClick, manager.QuitGame);

        // Sub Panels
        CreateLevelSelectPanel(canvasTransform, manager);
        CreateControlsPanel(canvasTransform, manager);
        CreateOptionsPanel(canvasTransform, manager);
    }

    private static void CreateLevelSelectPanel(Transform canvasTransform, MainMenuManager manager)
    {
        GameObject panelObj = CreateUIElement("LevelSelectPanel", canvasTransform);
        panelObj.SetActive(false);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(650, 600);
        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.9f);
        img.raycastTarget = false;

        CreateText("LevelTitle", "SELECT MISSION", panelObj.transform, new Vector2(0, 210), 38, FontStyle.Bold, Color.white);

        GameObject grid = CreateUIElement("LevelGrid", panelObj.transform);
        RectTransform gridRT = grid.GetComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0, 20);
        gridRT.sizeDelta = new Vector2(560, 280);

        GridLayoutGroup gridLayout = grid.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(160, 65);
        gridLayout.spacing = new Vector2(25, 25);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;

        Button mercuryBtn = CreateButton("1MercuryBtn", "1 Mercury", grid.transform, Vector2.zero, false);
        UnityEventTools.AddPersistentListener(mercuryBtn.onClick, manager.LoadLevel1Mercury);

        Button venusBtn = CreateButton("2VenusBtn", "2 Venus", grid.transform, Vector2.zero, false);
        UnityEventTools.AddPersistentListener(venusBtn.onClick, manager.LoadLevel2Venus);

        Button marsBtn = CreateButton("3MarsBtn", "3 Mars", grid.transform, Vector2.zero, false);
        UnityEventTools.AddPersistentListener(marsBtn.onClick, manager.LoadLevel3Mars);

        Button jupiterBtn = CreateButton("4JupiterBtn", "4 Jupiter", grid.transform, Vector2.zero, false);
        UnityEventTools.AddPersistentListener(jupiterBtn.onClick, manager.LoadLevel4Jupiter);

        Button neptuneBtn = CreateButton("5NeptuneBtn", "5 Neptune", grid.transform, Vector2.zero, false);
        UnityEventTools.AddPersistentListener(neptuneBtn.onClick, manager.LoadLevel5Neptune);

        manager.levelButtons.Clear();
        manager.levelButtons.Add(mercuryBtn);
        manager.levelButtons.Add(venusBtn);
        manager.levelButtons.Add(marsBtn);
        manager.levelButtons.Add(jupiterBtn);
        manager.levelButtons.Add(neptuneBtn);

        Button backBtn = CreateButton("BackLvlBtn", "Back", panelObj.transform, new Vector2(0, -200), false);
        UnityEventTools.AddPersistentListener(backBtn.onClick, manager.ShowMainPanel);

        manager.levelSelectPanel = panelObj;
    }

    private static void CreateControlsPanel(Transform canvasTransform, MainMenuManager manager)
    {
        GameObject panelObj = CreateUIElement("ControlsPanel", canvasTransform);
        panelObj.SetActive(false);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(550, 600);
        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.9f);
        img.raycastTarget = false;

        CreateText("ControlsTitle", "CONTROLS", panelObj.transform, new Vector2(0, 210), 38, FontStyle.Bold, Color.white);

        string controlsText = "THRUST : W / Up Arrow / Space\n\n" +
                              "ROTATE LEFT : A / Left Arrow\n\n" +
                              "ROTATE RIGHT : D / Right Arrow\n\n" +
                              "PAUSE MENU : ESC";

        CreateText("ControlsText", controlsText, panelObj.transform, new Vector2(0, 10), 22, FontStyle.Normal, new Color(0.9f, 0.9f, 1.0f, 0.95f));

        Button backBtn = CreateButton("BackCtrlBtn", "Back", panelObj.transform, new Vector2(0, -200), false);
        UnityEventTools.AddPersistentListener(backBtn.onClick, manager.ShowMainPanel);

        manager.controlsPanel = panelObj;
    }

    private static void CreateOptionsPanel(Transform canvasTransform, MainMenuManager manager)
    {
        GameObject panelObj = CreateUIElement("OptionsPanel", canvasTransform);
        panelObj.SetActive(false);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(550, 600);
        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.9f);
        img.raycastTarget = false;

        CreateText("OptionsTitle", "OPTIONS", panelObj.transform, new Vector2(0, 210), 38, FontStyle.Bold, Color.white);

        Slider masterSlider = CreateSlider("MasterVolume_Container", "Master Volume", panelObj.transform, new Vector2(0, 100));
        Slider musicSlider = CreateSlider("MusicVolume_Container", "Music Volume", panelObj.transform, new Vector2(0, 10));
        Slider sfxSlider = CreateSlider("SFXVolume_Container", "SFX Volume", panelObj.transform, new Vector2(0, -80));

        manager.masterVolumeSlider = masterSlider;
        manager.musicVolumeSlider = musicSlider;
        manager.sfxVolumeSlider = sfxSlider;

        Button backBtn = CreateButton("BackOptBtn", "Back", panelObj.transform, new Vector2(0, -200), false);
        UnityEventTools.AddPersistentListener(backBtn.onClick, manager.ShowMainPanel);

        manager.optionsPanel = panelObj;
    }

    private static Button CreateButton(string name, string text, Transform parent, Vector2 pos, bool isHighlight)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(260, 52);

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

    private static Slider CreateSlider(string name, string label, Transform parent, Vector2 pos)
    {
        GameObject container = CreateUIElement(name, parent);
        container.GetComponent<RectTransform>().anchoredPosition = pos;
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 70);

        CreateText(name + "_Label", label, container.transform, new Vector2(0, 24), 20, FontStyle.Normal, Color.white);

        GameObject sliderObj = CreateUIElement("Slider", container.transform);
        sliderObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -12);
        sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 18);
        Slider slider = sliderObj.AddComponent<Slider>();

        GameObject bgObj = CreateUIElement("Background", sliderObj.transform);
        StretchToFill(bgObj.GetComponent<RectTransform>());
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject fillAreaObj = CreateUIElement("Fill Area", sliderObj.transform);
        StretchToFill(fillAreaObj.GetComponent<RectTransform>());

        GameObject fillObj = CreateUIElement("Fill", fillAreaObj.transform);
        StretchToFill(fillObj.GetComponent<RectTransform>());
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.9f, 1.0f, 1f);

        GameObject handleAreaObj = CreateUIElement("Handle Slide Area", sliderObj.transform);
        StretchToFill(handleAreaObj.GetComponent<RectTransform>());

        GameObject handleObj = CreateUIElement("Handle", handleAreaObj.transform);
        handleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(22, 0);
        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillObj.GetComponent<RectTransform>();
        slider.handleRect = handleObj.GetComponent<RectTransform>();
        slider.value = 0.8f;

        return slider;
    }

    private static void CreateText(string name, string content, Transform parent, Vector2 pos, int fontSize, FontStyle style, Color color)
    {
        GameObject txtObj = CreateUIElement(name, parent);
        txtObj.GetComponent<RectTransform>().anchoredPosition = pos;
        txtObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
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
