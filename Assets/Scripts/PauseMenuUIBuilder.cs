using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class PauseMenuUIBuilder : MonoBehaviour
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

    public static void EnsureUI(PauseMenuManager manager)
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
        bdImg.color = new Color(0.02f, 0.05f, 0.09f, 0.85f);

        // Pause Main Menu Card
        GameObject pauseCardObj = CreateUIElement("PauseMainCard", backdropObj.transform);
        RectTransform cardRect = pauseCardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(500, 520);

        Image cardImg = pauseCardObj.AddComponent<Image>();
        cardImg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

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
        titleText.color = new Color(0.75f, 0.9f, 1.0f);
        titleText.fontStyle = FontStyle.Bold;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 50);

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
        soundImg.color = new Color(0.03f, 0.08f, 0.14f, 0.95f);

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
        soundTitleText.color = new Color(0.75f, 0.9f, 1.0f);
        soundTitleText.fontStyle = FontStyle.Bold;
        soundTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 50);

        Slider masterSlider = CreateVolumeSlider("Master Volume", manager != null ? manager.SetMasterVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MasterVolume : 1.0f);
        Slider musicSlider = CreateVolumeSlider("Music Volume", manager != null ? manager.SetMusicVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.8f);
        Slider sfxSlider = CreateVolumeSlider("SFX Volume", manager != null ? manager.SetSFXVolume : null, soundCardObj.transform, SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f);

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
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 50);

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
        btnText.fontSize = 22;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.85f, 0.94f, 1.0f);

        StretchToFill(txtObj.GetComponent<RectTransform>());

        if (action != null)
        {
            btn.onClick.AddListener(action);
        }

        return btn;
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
