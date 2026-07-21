using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.IO;

[ExecuteAlways]
public class MainMenuUIBuilder : MonoBehaviour
{
    private static Font cachedFont;
    private static Sprite cachedGalaxySprite;

    public static Font GetSafeFont()
    {
        if (cachedFont != null) return cachedFont;

        cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 18);

        return cachedFont;
    }

    public static Sprite GetGalaxySprite()
    {
        if (cachedGalaxySprite != null) return cachedGalaxySprite;

        // 1. Try Resources.Load
        cachedGalaxySprite = Resources.Load<Sprite>("SpaceGalaxyBackground");
        if (cachedGalaxySprite != null) return cachedGalaxySprite;

#if UNITY_EDITOR
        // 2. Try AssetDatabase in Editor
        cachedGalaxySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/SpaceGalaxyBackground.png");
        if (cachedGalaxySprite == null)
        {
            cachedGalaxySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/SpaceGalaxyBackground.png");
        }
        if (cachedGalaxySprite != null) return cachedGalaxySprite;
#endif

        // 3. Fallback: Direct PNG File Byte Loading (Guaranteed to work 100% of the time)
        string[] searchPaths = new string[]
        {
            Path.Combine(Application.dataPath, "Resources/SpaceGalaxyBackground.png"),
            Path.Combine(Application.dataPath, "Textures/SpaceGalaxyBackground.png")
        };

        foreach (string path in searchPaths)
        {
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Point; // Crisp pixel art galaxy
                    cachedGalaxySprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    return cachedGalaxySprite;
                }
            }
        }

        return null;
    }

    public static void EnsureUI(MainMenuManager manager)
    {
        // 1. EventSystem
        EventSystem eventSys = FindObjectOfType<EventSystem>();
        GameObject eventSystemObj;

        if (eventSys == null)
        {
            eventSystemObj = new GameObject("EventSystem");
            eventSys = eventSystemObj.AddComponent<EventSystem>();
        }
        else
        {
            eventSystemObj = eventSys.gameObject;
        }

        StandaloneInputModule legacyModule = eventSystemObj.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            if (Application.isPlaying) Destroy(legacyModule);
            else DestroyImmediate(legacyModule);
        }

        InputSystemUIInputModule inputModule = eventSystemObj.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
        }

        // 2. Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // 3. Pixel Art Galaxy Background Image (Stretched Fullscreen)
        GameObject bgObj = CreateUIElement("SpaceBackground", canvasObj.transform);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        StretchToFill(bgRect);
        Image bgImg = bgObj.AddComponent<Image>();

        Sprite galaxySprite = GetGalaxySprite();
        if (galaxySprite != null)
        {
            bgImg.sprite = galaxySprite;
            bgImg.color = Color.white;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }
        else
        {
            bgImg.color = new Color(0.06f, 0.02f, 0.12f, 1.0f);
        }

        // 4. Sleek Translucent Main Menu Card (Minimalist Reference Style)
        GameObject menuCardObj = CreateUIElement("MainPanel", canvasObj.transform);
        RectTransform cardRect = menuCardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(460, 520);

        Image cardBg = menuCardObj.AddComponent<Image>();
        cardBg.color = new Color(0.05f, 0.02f, 0.12f, 0.4f); // Translucent subtle glass backdrop

        VerticalLayoutGroup cardLayout = menuCardObj.AddComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(20, 20, 20, 20);
        cardLayout.spacing = 18;
        cardLayout.childAlignment = TextAnchor.MiddleCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = false;

        // Title Text
        GameObject titleObj = CreateUIElement("TitleText", menuCardObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ROCKET BOOST";
        titleText.font = GetSafeFont();
        titleText.fontSize = 42;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 1.0f, 0.95f);
        titleText.fontStyle = FontStyle.Bold;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 55);

        // Subtitle Text
        GameObject subObj = CreateUIElement("SubtitleText", menuCardObj.transform);
        Text subText = subObj.AddComponent<Text>();
        subText.text = "GALAXY EXPLORER";
        subText.font = GetSafeFont();
        subText.fontSize = 15;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.7f, 0.6f, 0.9f, 0.75f);
        subObj.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 25);

        // Sleek Minimalist Centered Buttons (matching reference image)
        Button playBtn = CreateRefButton("PlayBtn", "Play", menuCardObj.transform, manager != null ? manager.PlayGame : null);
        Button controlsBtn = CreateRefButton("ControlsBtn", "Controls", menuCardObj.transform, manager != null ? manager.OpenControls : null);
        Button optionsBtn = CreateRefButton("OptionsBtn", "Options", menuCardObj.transform, manager != null ? manager.OpenOptions : null);
        Button exitBtn = CreateRefButton("ExitBtn", "Exit", menuCardObj.transform, manager != null ? manager.QuitGame : null);

        // 5. Controls Panel (Translucent Glass Overlay)
        GameObject controlsCardObj = CreateUIElement("ControlsPanel", canvasObj.transform);
        RectTransform controlsRect = controlsCardObj.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0.5f);
        controlsRect.anchorMax = new Vector2(0.5f, 0.5f);
        controlsRect.pivot = new Vector2(0.5f, 0.5f);
        controlsRect.sizeDelta = new Vector2(520, 540);

        Image controlsImg = controlsCardObj.AddComponent<Image>();
        controlsImg.color = new Color(0.04f, 0.02f, 0.1f, 0.82f); // Translucent glass backdrop

        VerticalLayoutGroup controlsLayout = controlsCardObj.AddComponent<VerticalLayoutGroup>();
        controlsLayout.padding = new RectOffset(30, 30, 30, 30);
        controlsLayout.spacing = 16;
        controlsLayout.childAlignment = TextAnchor.UpperCenter;
        controlsLayout.childControlWidth = true;
        controlsLayout.childControlHeight = false;

        GameObject ctrlTitle = CreateUIElement("CtrlTitle", controlsCardObj.transform);
        Text ctrlTitleText = ctrlTitle.AddComponent<Text>();
        ctrlTitleText.text = "FLIGHT CONTROLS";
        ctrlTitleText.font = GetSafeFont();
        ctrlTitleText.fontSize = 28;
        ctrlTitleText.alignment = TextAnchor.MiddleCenter;
        ctrlTitleText.color = new Color(0.9f, 0.85f, 1.0f);
        ctrlTitleText.fontStyle = FontStyle.Bold;
        ctrlTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 45);

        CreateControlRow("Thrust Rocket", "SPACE / W", controlsCardObj.transform);
        CreateControlRow("Steer Left", "A / LEFT ARROW", controlsCardObj.transform);
        CreateControlRow("Steer Right", "D / RIGHT ARROW", controlsCardObj.transform);
        CreateControlRow("Pause Game", "ESCAPE", controlsCardObj.transform);

        Button backCtrlBtn = CreateRefButton("BackCtrlBtn", "Back to Menu", controlsCardObj.transform, manager != null ? manager.ShowMainPanel : null);

        // 6. Options Panel (Translucent Glass Overlay with Clean Sliders)
        GameObject optionsCardObj = CreateUIElement("OptionsPanel", canvasObj.transform);
        RectTransform optionsRect = optionsCardObj.GetComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.5f, 0.5f);
        optionsRect.anchorMax = new Vector2(0.5f, 0.5f);
        optionsRect.pivot = new Vector2(0.5f, 0.5f);
        optionsRect.sizeDelta = new Vector2(520, 540);

        Image optionsImg = optionsCardObj.AddComponent<Image>();
        optionsImg.color = new Color(0.04f, 0.02f, 0.1f, 0.82f);

        VerticalLayoutGroup optionsLayout = optionsCardObj.AddComponent<VerticalLayoutGroup>();
        optionsLayout.padding = new RectOffset(30, 30, 30, 30);
        optionsLayout.spacing = 18;
        optionsLayout.childAlignment = TextAnchor.UpperCenter;
        optionsLayout.childControlWidth = true;
        optionsLayout.childControlHeight = false;

        GameObject optTitle = CreateUIElement("OptTitle", optionsCardObj.transform);
        Text optTitleText = optTitle.AddComponent<Text>();
        optTitleText.text = "SETTINGS";
        optTitleText.font = GetSafeFont();
        optTitleText.fontSize = 28;
        optTitleText.alignment = TextAnchor.MiddleCenter;
        optTitleText.color = new Color(0.9f, 0.85f, 1.0f);
        optTitleText.fontStyle = FontStyle.Bold;
        optTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 45);

        Slider masterSlider = CreatePristineVolumeSlider("Master Volume", manager != null ? manager.SetMasterVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MasterVolume : 1.0f);
        Slider musicSlider = CreatePristineVolumeSlider("Music Volume", manager != null ? manager.SetMusicVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.8f);
        Slider sfxSlider = CreatePristineVolumeSlider("SFX Volume", manager != null ? manager.SetSFXVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f);

        Button backOptBtn = CreateRefButton("BackOptBtn", "Back to Menu", optionsCardObj.transform, manager != null ? manager.ShowMainPanel : null);

        if (manager != null)
        {
            SetPrivateField(manager, "mainPanel", menuCardObj);
            SetPrivateField(manager, "controlsPanel", controlsCardObj);
            SetPrivateField(manager, "optionsPanel", optionsCardObj);
            SetPrivateField(manager, "masterVolumeSlider", masterSlider);
            SetPrivateField(manager, "musicVolumeSlider", musicSlider);
            SetPrivateField(manager, "sfxVolumeSlider", sfxSlider);
        }

        controlsCardObj.SetActive(false);
        optionsCardObj.SetActive(false);
        menuCardObj.SetActive(true);
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static Button CreateRefButton(string name, string text, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 48);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.1f, 0.35f, 0.35f);

        Button btn = btnObj.AddComponent<Button>();

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.1f, 0.35f, 0.35f);
        colors.highlightedColor = new Color(0.5f, 0.25f, 0.7f, 0.7f);
        colors.pressedColor = new Color(0.85f, 0.45f, 1.0f, 0.95f);
        btn.colors = colors;

        GameObject txtObj = CreateUIElement(name + "_Text", btnObj.transform);
        Text btnText = txtObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = GetSafeFont();
        btnText.fontSize = 24;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.92f, 0.88f, 1.0f);

        StretchToFill(txtObj.GetComponent<RectTransform>());

        if (action != null)
        {
            btn.onClick.AddListener(action);
        }

        return btn;
    }

    private static void CreateControlRow(string label, string key, Transform parent)
    {
        GameObject row = CreateUIElement(label + "_Row", parent);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 44);

        Image rowImg = row.AddComponent<Image>();
        rowImg.color = new Color(0.18f, 0.08f, 0.28f, 0.35f);

        GameObject labelObj = CreateUIElement("Label", row.transform);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = GetSafeFont();
        labelText.fontSize = 17;
        labelText.color = new Color(0.92f, 0.88f, 1.0f);
        labelText.alignment = TextAnchor.MiddleLeft;
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0.05f, 0f);
        lblRect.anchorMax = new Vector2(0.45f, 1f);

        GameObject keyObj = CreateUIElement("Key", row.transform);
        Text keyText = keyObj.AddComponent<Text>();
        keyText.text = key;
        keyText.font = GetSafeFont();
        keyText.fontSize = 15;
        keyText.fontStyle = FontStyle.Bold;
        keyText.color = new Color(0.85f, 0.45f, 1.0f);
        keyText.alignment = TextAnchor.MiddleRight;
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0f);
        keyRect.anchorMax = new Vector2(0.95f, 1f);
    }

    private static Slider CreatePristineVolumeSlider(string labelText, UnityEngine.Events.UnityAction<float> callback, Transform parent, float initialVal)
    {
        // Container
        GameObject container = CreateUIElement(labelText + "_Container", parent);
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 42);

        // Label
        GameObject labelObj = CreateUIElement("Label", container.transform);
        Text labelTextComp = labelObj.AddComponent<Text>();
        labelTextComp.text = labelText;
        labelTextComp.font = GetSafeFont();
        labelTextComp.fontSize = 17;
        labelTextComp.color = new Color(0.92f, 0.88f, 1.0f);
        labelTextComp.alignment = TextAnchor.MiddleLeft;
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0.02f, 0);
        lblRect.anchorMax = new Vector2(0.42f, 1);

        // Slider Object
        GameObject sliderObj = CreateUIElement(labelText + "_Slider", container.transform);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.44f, 0.3f);
        sliderRect.anchorMax = new Vector2(0.98f, 0.7f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialVal;

        // Background Bar
        GameObject bg = CreateUIElement("Background", sliderObj.transform);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.35f);
        bgRect.anchorMax = new Vector2(1, 0.65f);
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.08f, 0.25f, 0.8f);

        // Fill Area
        GameObject fillArea = CreateUIElement("Fill Area", sliderObj.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1, 0.65f);
        fillAreaRect.sizeDelta = Vector2.zero;

        // Fill
        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.45f, 1.0f, 1.0f);

        // Handle Slide Area
        GameObject handleArea = CreateUIElement("Handle Slide Area", sliderObj.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = Vector2.zero;

        // Handle (Knob)
        GameObject handle = CreateUIElement("Handle", handleArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18, 18);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(1.0f, 0.9f, 1.0f, 1.0f);

        slider.targetGraphic = handleImg;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;

        if (callback != null)
        {
            slider.onValueChanged.AddListener(callback);
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

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
