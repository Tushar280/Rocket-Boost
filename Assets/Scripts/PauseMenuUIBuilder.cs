using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class PauseMenuUIBuilder : MonoBehaviour
{
    private static Font cachedFont;

    public static Font GetSafeFont()
    {
        if (cachedFont != null) return cachedFont;

        try
        {
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            cachedFont = null;
        }

        if (cachedFont == null)
        {
            cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 18);
        }

        return cachedFont;
    }

    public static void EnsureUI(PauseMenuManager manager)
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
            canvasObj = new GameObject("PauseMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

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

        // Backdrop Dark Overlay
        GameObject backdropObj = CreateUIElement("PauseBackdrop", canvasObj.transform);
        RectTransform bdRect = backdropObj.GetComponent<RectTransform>();
        StretchToFill(bdRect);
        Image bdImg = backdropObj.AddComponent<Image>();

        Sprite galaxySprite = MainMenuUIBuilder.GetGalaxySprite();
        if (galaxySprite != null)
        {
            bdImg.sprite = galaxySprite;
            bdImg.color = new Color(0.55f, 0.45f, 0.65f, 0.92f);
            bdImg.type = Image.Type.Simple;
            bdImg.preserveAspect = false;
        }
        else
        {
            bdImg.color = new Color(0.06f, 0.02f, 0.12f, 0.88f);
        }

        // Pause Main Card
        GameObject pauseCardObj = CreateUIElement("PauseMainCard", backdropObj.transform);
        RectTransform cardRect = pauseCardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(460, 480);

        Image cardImg = pauseCardObj.AddComponent<Image>();
        cardImg.color = new Color(0.04f, 0.02f, 0.1f, 0.85f);

        VerticalLayoutGroup layout = pauseCardObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Title
        GameObject titleObj = CreateUIElement("PauseTitle", pauseCardObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "GAME PAUSED";
        titleText.font = GetSafeFont();
        titleText.fontSize = 32;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;
        titleObj.AddComponent<Outline>().effectColor = Color.black;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 45);

        // Buttons
        Button resumeBtn = CreateRefButton("ResumeBtn", "Return to Game", pauseCardObj.transform, manager != null ? manager.ResumeGame : null);
        Button soundBtn = CreateRefButton("SoundBtn", "Sound Options", pauseCardObj.transform, manager != null ? manager.OpenSoundOptions : null);
        Button mainBtn = CreateRefButton("MainMenuBtn", "Exit to Main Menu", pauseCardObj.transform, manager != null ? manager.ExitToMainMenu : null);
        Button exitBtn = CreateRefButton("ExitDesktopBtn", "Exit to Desktop", pauseCardObj.transform, manager != null ? manager.ExitToDesktop : null);

        // Sound Options Panel
        GameObject soundCardObj = CreateUIElement("PauseSoundCard", backdropObj.transform);
        RectTransform soundRect = soundCardObj.GetComponent<RectTransform>();
        soundRect.anchorMin = new Vector2(0.5f, 0.5f);
        soundRect.anchorMax = new Vector2(0.5f, 0.5f);
        soundRect.pivot = new Vector2(0.5f, 0.5f);
        soundRect.sizeDelta = new Vector2(500, 480);

        Image soundImg = soundCardObj.AddComponent<Image>();
        soundImg.color = new Color(0.04f, 0.02f, 0.1f, 0.85f);

        VerticalLayoutGroup soundLayout = soundCardObj.AddComponent<VerticalLayoutGroup>();
        soundLayout.padding = new RectOffset(30, 30, 30, 30);
        soundLayout.spacing = 16;
        soundLayout.childAlignment = TextAnchor.UpperCenter;
        soundLayout.childControlWidth = true;
        soundLayout.childControlHeight = false;

        GameObject soundTitle = CreateUIElement("SoundTitle", soundCardObj.transform);
        Text soundTitleText = soundTitle.AddComponent<Text>();
        soundTitleText.text = "AUDIO OPTIONS";
        soundTitleText.font = GetSafeFont();
        soundTitleText.fontSize = 28;
        soundTitleText.alignment = TextAnchor.MiddleCenter;
        soundTitleText.color = Color.white;
        soundTitleText.fontStyle = FontStyle.Bold;
        soundTitle.AddComponent<Outline>().effectColor = Color.black;
        soundTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 45);

        Slider masterSlider = CreatePristineVolumeSlider("Master Volume", manager != null ? manager.SetMasterVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MasterVolume : 1.0f);
        Slider musicSlider = CreatePristineVolumeSlider("Music Volume", manager != null ? manager.SetMusicVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.8f);
        Slider sfxSlider = CreatePristineVolumeSlider("SFX Volume", manager != null ? manager.SetSFXVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f);

        Button backSoundBtn = CreateRefButton("BackSoundBtn", "Back to Pause Menu", soundCardObj.transform, manager != null ? manager.CloseSoundOptions : null);

        if (manager != null)
        {
            SetPrivateField(manager, "pauseMenuPanel", pauseCardObj);
            SetPrivateField(manager, "soundOptionsPanel", soundCardObj);
            SetPrivateField(manager, "masterVolumeSlider", masterSlider);
            SetPrivateField(manager, "musicVolumeSlider", musicSlider);
            SetPrivateField(manager, "sfxVolumeSlider", sfxSlider);
        }

        backdropObj.SetActive(false);
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
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 50);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.1f, 0.35f, 0.45f);

        Button btn = btnObj.AddComponent<Button>();

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.1f, 0.35f, 0.45f);
        colors.highlightedColor = new Color(0.5f, 0.25f, 0.75f, 0.8f);
        colors.pressedColor = new Color(0.85f, 0.45f, 1.0f, 0.95f);
        btn.colors = colors;

        GameObject txtObj = CreateUIElement(name + "_Text", btnObj.transform);
        Text btnText = txtObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = GetSafeFont();
        btnText.fontSize = 22;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        txtObj.AddComponent<Outline>().effectColor = Color.black;

        StretchToFill(txtObj.GetComponent<RectTransform>());

        if (action != null)
        {
            btn.onClick.AddListener(action);
        }

        return btn;
    }

    private static Slider CreatePristineVolumeSlider(string labelText, UnityEngine.Events.UnityAction<float> callback, Transform parent, float initialVal)
    {
        GameObject container = CreateUIElement(labelText + "_Container", parent);
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 42);

        // Label
        GameObject labelObj = CreateUIElement("Label", container.transform);
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0f, 0f);
        lblRect.anchorMax = new Vector2(0.42f, 1f);
        lblRect.offsetMin = new Vector2(10, 0);
        lblRect.offsetMax = Vector2.zero;

        Text labelTextComp = labelObj.AddComponent<Text>();
        labelTextComp.text = labelText;
        labelTextComp.font = GetSafeFont();
        labelTextComp.fontSize = 17;
        labelTextComp.color = Color.white;
        labelTextComp.alignment = TextAnchor.MiddleLeft;
        labelObj.AddComponent<Outline>().effectColor = Color.black;

        // Slider Object
        GameObject sliderObj = CreateUIElement(labelText + "_Slider", container.transform);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.44f, 0.2f);
        sliderRect.anchorMax = new Vector2(1f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = new Vector2(-10, 0);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialVal;

        // Background Bar
        GameObject bg = CreateUIElement("Background", sliderObj.transform);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.35f);
        bgRect.anchorMax = new Vector2(1, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.08f, 0.25f, 0.8f);

        // Fill Area
        GameObject fillArea = CreateUIElement("Fill Area", sliderObj.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1, 0.65f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.45f, 1.0f, 1.0f);

        // Handle Slide Area
        GameObject handleArea = CreateUIElement("Handle Slide Area", sliderObj.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        // Handle (Knob)
        GameObject handle = CreateUIElement("Handle", handleArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(0, 1);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(18, 0);

        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

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
