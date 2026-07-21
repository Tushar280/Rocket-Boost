using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class MainMenuUIBuilder : MonoBehaviour
{
    private static Font cachedFont;

    public static Font GetSafeFont()
    {
        if (cachedFont != null) return cachedFont;

        cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 18);

        return cachedFont;
    }

    public static void EnsureUI(MainMenuManager manager)
    {
        // 1. Ensure EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        // 2. Ensure Canvas
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

        // 3. Atmospheric Space Background (matching user reference image)
        GameObject bgObj = CreateUIElement("SpaceBackground", canvasObj.transform);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        StretchToFill(bgRect);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.05f, 0.09f, 1.0f); // Deep space dark teal

        // Ethereal light gradient curve (matching reference image atmosphere)
        GameObject gradObj = CreateUIElement("AtmosphereGradient", bgObj.transform);
        RectTransform gradRect = gradObj.GetComponent<RectTransform>();
        StretchToFill(gradRect);
        Image gradImg = gradObj.AddComponent<Image>();
        gradImg.color = new Color(0.18f, 0.38f, 0.48f, 0.35f);

        // 4. Main Menu Card
        GameObject menuCardObj = CreateUIElement("MainPanel", canvasObj.transform);
        RectTransform cardRect = menuCardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(500, 600);

        VerticalLayoutGroup cardLayout = menuCardObj.AddComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(20, 20, 20, 20);
        cardLayout.spacing = 20;
        cardLayout.childAlignment = TextAnchor.MiddleCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = false;

        // Title Text
        GameObject titleObj = CreateUIElement("TitleText", menuCardObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ROCKET BOOST";
        titleText.font = GetSafeFont();
        titleText.fontSize = 44;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.75f, 0.9f, 1.0f, 1.0f);
        titleText.fontStyle = FontStyle.Bold;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 60);

        // Subtitle Text
        GameObject subObj = CreateUIElement("SubtitleText", menuCardObj.transform);
        Text subText = subObj.AddComponent<Text>();
        subText.text = "SPACE EXPLORER";
        subText.font = GetSafeFont();
        subText.fontSize = 16;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.45f, 0.7f, 0.85f, 0.8f);
        subObj.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 30);

        // Minimalist Glowing Metallic Buttons (matching reference image)
        Button playBtn = CreateRefButton("PlayBtn", "Play", menuCardObj.transform, manager != null ? manager.PlayGame : null);
        Button controlsBtn = CreateRefButton("ControlsBtn", "Controls", menuCardObj.transform, manager != null ? manager.OpenControls : null);
        Button optionsBtn = CreateRefButton("OptionsBtn", "Options", menuCardObj.transform, manager != null ? manager.OpenOptions : null);
        Button exitBtn = CreateRefButton("ExitBtn", "Exit", menuCardObj.transform, manager != null ? manager.QuitGame : null);

        // 5. Controls Panel
        GameObject controlsCardObj = CreateUIElement("ControlsPanel", canvasObj.transform);
        RectTransform controlsRect = controlsCardObj.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0.5f);
        controlsRect.anchorMax = new Vector2(0.5f, 0.5f);
        controlsRect.pivot = new Vector2(0.5f, 0.5f);
        controlsRect.sizeDelta = new Vector2(550, 580);

        Image controlsImg = controlsCardObj.AddComponent<Image>();
        controlsImg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

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
        ctrlTitleText.color = new Color(0.75f, 0.9f, 1.0f);
        ctrlTitleText.fontStyle = FontStyle.Bold;
        ctrlTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 50);

        CreateControlRow("Rocket Thrust", "SPACE / W", controlsCardObj.transform);
        CreateControlRow("Steer Left", "A / LEFT ARROW", controlsCardObj.transform);
        CreateControlRow("Steer Right", "D / RIGHT ARROW", controlsCardObj.transform);
        CreateControlRow("Pause Game", "ESCAPE", controlsCardObj.transform);

        Button backCtrlBtn = CreateRefButton("BackCtrlBtn", "Back to Menu", controlsCardObj.transform, manager != null ? manager.ShowMainPanel : null);

        // 6. Options Panel
        GameObject optionsCardObj = CreateUIElement("OptionsPanel", canvasObj.transform);
        RectTransform optionsRect = optionsCardObj.GetComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.5f, 0.5f);
        optionsRect.anchorMax = new Vector2(0.5f, 0.5f);
        optionsRect.pivot = new Vector2(0.5f, 0.5f);
        optionsRect.sizeDelta = new Vector2(550, 580);

        Image optionsImg = optionsCardObj.AddComponent<Image>();
        optionsImg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

        VerticalLayoutGroup optionsLayout = optionsCardObj.AddComponent<VerticalLayoutGroup>();
        optionsLayout.padding = new RectOffset(30, 30, 30, 30);
        optionsLayout.spacing = 16;
        optionsLayout.childAlignment = TextAnchor.UpperCenter;
        optionsLayout.childControlWidth = true;
        optionsLayout.childControlHeight = false;

        GameObject optTitle = CreateUIElement("OptTitle", optionsCardObj.transform);
        Text optTitleText = optTitle.AddComponent<Text>();
        optTitleText.text = "SETTINGS";
        optTitleText.font = GetSafeFont();
        optTitleText.fontSize = 28;
        optTitleText.alignment = TextAnchor.MiddleCenter;
        optTitleText.color = new Color(0.75f, 0.9f, 1.0f);
        optTitleText.fontStyle = FontStyle.Bold;
        optTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 50);

        Slider masterSlider = CreateVolumeSlider("Master Volume", manager != null ? manager.SetMasterVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MasterVolume : 1.0f);
        Slider musicSlider = CreateVolumeSlider("Music Volume", manager != null ? manager.SetMusicVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.8f);
        Slider sfxSlider = CreateVolumeSlider("SFX Volume", manager != null ? manager.SetSFXVolume : null, optionsCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f);

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
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 54);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.24f, 0.35f, 0.45f);

        Button btn = btnObj.AddComponent<Button>();

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.12f, 0.24f, 0.35f, 0.45f);
        colors.highlightedColor = new Color(0.25f, 0.55f, 0.75f, 0.85f);
        colors.pressedColor = new Color(0.0f, 0.85f, 1.0f, 1.0f);
        btn.colors = colors;

        GameObject txtObj = CreateUIElement(name + "_Text", btnObj.transform);
        Text btnText = txtObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = GetSafeFont();
        btnText.fontSize = 26;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.85f, 0.94f, 1.0f);

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
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 46);

        Image rowImg = row.AddComponent<Image>();
        rowImg.color = new Color(0.1f, 0.2f, 0.3f, 0.4f);

        GameObject labelObj = CreateUIElement("Label", row.transform);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = GetSafeFont();
        labelText.fontSize = 18;
        labelText.color = new Color(0.85f, 0.95f, 1.0f);
        labelText.alignment = TextAnchor.MiddleLeft;
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0.05f, 0f);
        lblRect.anchorMax = new Vector2(0.45f, 1f);

        GameObject keyObj = CreateUIElement("Key", row.transform);
        Text keyText = keyObj.AddComponent<Text>();
        keyText.text = key;
        keyText.font = GetSafeFont();
        keyText.fontSize = 16;
        keyText.fontStyle = FontStyle.Bold;
        keyText.color = new Color(0.3f, 0.85f, 1.0f);
        keyText.alignment = TextAnchor.MiddleRight;
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0f);
        keyRect.anchorMax = new Vector2(0.95f, 1f);
    }

    private static Slider CreateVolumeSlider(string label, UnityEngine.Events.UnityAction<float> callback, Transform parent, float initialVal)
    {
        GameObject container = CreateUIElement(label + "_Container", parent);
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 48);

        GameObject labelObj = CreateUIElement("Label", container.transform);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = GetSafeFont();
        labelText.fontSize = 18;
        labelText.color = new Color(0.85f, 0.95f, 1.0f);
        labelText.alignment = TextAnchor.MiddleLeft;
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0.02f, 0);
        lblRect.anchorMax = new Vector2(0.45f, 1);

        GameObject sliderObj = CreateUIElement(label + "_Slider", container.transform);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.48f, 0.25f);
        sliderRect.anchorMax = new Vector2(0.98f, 0.75f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialVal;

        GameObject bg = CreateUIElement("Background", sliderObj.transform);
        StretchToFill(bg.GetComponent<RectTransform>());
        bg.AddComponent<Image>().color = new Color(0.1f, 0.2f, 0.35f, 0.8f);

        GameObject fillArea = CreateUIElement("Fill Area", sliderObj.transform);
        StretchToFill(fillArea.GetComponent<RectTransform>());

        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        StretchToFill(fill.GetComponent<RectTransform>());
        fill.AddComponent<Image>().color = new Color(0.2f, 0.8f, 1.0f, 1.0f);

        slider.targetGraphic = fill.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();

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
