using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class LevelHUDManager : MonoBehaviour
{
    public static LevelHUDManager Instance { get; private set; }

    private GameObject hudCanvasObj;
    private GameObject levelTitleObj;
    private Text levelTitleText;
    
    private GameObject levelCompletePanel;
    private Text winTitleText;
    private Text winSubText;
    private Button nextLevelBtnComp;
    private Button mainMenuBtnComp;
    private Button exitDesktopBtnComp;

    private float displayDuration = 2.5f;
    private Coroutine hideTitleCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitHUD()
    {
        EnsureInstance();
    }

    public static LevelHUDManager EnsureInstance()
    {
        if (Instance != null) return Instance;

        LevelHUDManager existing = FindObjectOfType<LevelHUDManager>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject hudObj = new GameObject("LevelHUDManager");
        Instance = hudObj.AddComponent<LevelHUDManager>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        EnsureUIBuilt();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        TriggerLevelTitleForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1.0f; // Unpause time on scene load
        EnsureUIBuilt();
        TriggerLevelTitleForCurrentScene();
    }

    public static string GetCleanLevelName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "Space Orbit";

        // Remove numeric prefixes (e.g. 1Mercury -> Mercury, 4jupiter -> jupiter)
        string cleaned = System.Text.RegularExpressions.Regex.Replace(rawName, @"^\d+", "").Trim();
        if (string.IsNullOrEmpty(cleaned)) cleaned = rawName;

        // Capitalize first letter
        return char.ToUpper(cleaned[0]) + (cleaned.Length > 1 ? cleaned.Substring(1) : "");
    }

    public static string FormatLevelName(string rawName)
    {
        return $"\"{GetCleanLevelName(rawName)}\"";
    }

    public void TriggerLevelTitleForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        // If in Main Menu, hide HUD canvas
        if (activeScene.name == "0MainMenu" || activeScene.buildIndex == 0)
        {
            if (hudCanvasObj != null) hudCanvasObj.SetActive(false);
            return;
        }

        if (hudCanvasObj != null) hudCanvasObj.SetActive(true);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        if (levelTitleText != null && levelTitleObj != null)
        {
            string formattedName = FormatLevelName(activeScene.name);
            levelTitleText.text = formattedName;
            levelTitleObj.SetActive(true);

            if (hideTitleCoroutine != null)
            {
                StopCoroutine(hideTitleCoroutine);
            }
            hideTitleCoroutine = StartCoroutine(HideTitleRoutine());
        }
    }

    private IEnumerator HideTitleRoutine()
    {
        yield return new WaitForSecondsRealtime(displayDuration);

        if (levelTitleObj != null)
        {
            levelTitleObj.SetActive(false);
        }
    }

    private bool IsLastLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        int currentIndex = activeScene.buildIndex;
        bool isNeptune = activeScene.name.ToLower().Contains("neptune");
        bool isLastBuildIndex = (currentIndex >= SceneManager.sceneCountInBuildSettings - 1) && SceneManager.sceneCountInBuildSettings > 1;

        return isNeptune || isLastBuildIndex;
    }

    public void ShowLevelCompleteMenu()
    {
        EnsureUIBuilt();
        if (hudCanvasObj != null) hudCanvasObj.SetActive(true);
        if (levelTitleObj != null) levelTitleObj.SetActive(false);

        if (levelCompletePanel != null)
        {
            string cleanName = GetCleanLevelName(SceneManager.GetActiveScene().name);

            if (IsLastLevel())
            {
                // Final Level (Neptune) Finish Screen
                if (winTitleText != null) winTitleText.text = "GALAXY CONQUERED!";
                if (winSubText != null) winSubText.text = $"\"{cleanName}\" COMPLETED - ALL LEVELS CLEARED!";

                if (nextLevelBtnComp != null) nextLevelBtnComp.gameObject.SetActive(false);
                if (mainMenuBtnComp != null) mainMenuBtnComp.gameObject.SetActive(true);
                if (exitDesktopBtnComp != null) exitDesktopBtnComp.gameObject.SetActive(true);
            }
            else
            {
                // Regular Levels (1-4) Finish Screen
                if (winTitleText != null) winTitleText.text = "LEVEL COMPLETE!";
                if (winSubText != null) winSubText.text = $"\"{cleanName}\" MISSION ACCOMPLISHED";

                if (nextLevelBtnComp != null) nextLevelBtnComp.gameObject.SetActive(true);
                if (mainMenuBtnComp != null) mainMenuBtnComp.gameObject.SetActive(true);
                if (exitDesktopBtnComp != null) exitDesktopBtnComp.gameObject.SetActive(false);
            }

            levelCompletePanel.SetActive(true);
            Time.timeScale = 0.0f; // Pause game physics on win menu
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1.0f;
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextIndex = 0; // Loop to Main Menu
        }

        SceneManager.LoadScene(nextIndex);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0); // 0MainMenu
    }

    public void ExitToDesktop()
    {
        Time.timeScale = 1.0f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureUIBuilt()
    {
        // 1. Ensure EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
        }

        // 2. Ensure Canvas
        if (hudCanvasObj == null)
        {
            hudCanvasObj = new GameObject("HUDCanvas");
            hudCanvasObj.transform.SetParent(transform, false);

            Canvas canvas = hudCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            CanvasScaler scaler = hudCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            hudCanvasObj.AddComponent<GraphicRaycaster>();
        }

        // 3. Center Screen Level Title Text
        if (levelTitleObj == null)
        {
            levelTitleObj = CreateUIElement("LevelTitleCenter", hudCanvasObj.transform);
            RectTransform titleRect = levelTitleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(900, 140);

            levelTitleText = levelTitleObj.AddComponent<Text>();
            levelTitleText.font = MainMenuUIBuilder.GetSafeFont();
            levelTitleText.fontSize = 64;
            levelTitleText.alignment = TextAnchor.MiddleCenter;
            levelTitleText.color = Color.white;
            levelTitleText.fontStyle = FontStyle.Bold;

            Outline outline = levelTitleObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.0f, 0.25f, 0.95f);
            outline.effectDistance = new Vector2(3, -3);
        }

        // 4. Level Complete Win Panel
        if (levelCompletePanel == null)
        {
            levelCompletePanel = CreateUIElement("LevelCompletePanel", hudCanvasObj.transform);
            RectTransform cardRect = levelCompletePanel.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(520, 380);

            Image cardBg = levelCompletePanel.AddComponent<Image>();
            cardBg.color = new Color(0.05f, 0.02f, 0.12f, 0.92f);

            VerticalLayoutGroup layout = levelCompletePanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            // Win Header
            GameObject winTitleObj = CreateUIElement("WinTitleText", levelCompletePanel.transform);
            winTitleText = winTitleObj.AddComponent<Text>();
            winTitleText.text = "LEVEL COMPLETE!";
            winTitleText.font = MainMenuUIBuilder.GetSafeFont();
            winTitleText.fontSize = 32;
            winTitleText.alignment = TextAnchor.MiddleCenter;
            winTitleText.color = new Color(0.85f, 0.45f, 1.0f);
            winTitleText.fontStyle = FontStyle.Bold;
            winTitleObj.AddComponent<Outline>().effectColor = Color.black;
            winTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 50);

            // Subtitle
            GameObject winSubObj = CreateUIElement("WinSubText", levelCompletePanel.transform);
            winSubText = winSubObj.AddComponent<Text>();
            winSubText.text = "MISSION ACCOMPLISHED";
            winSubText.font = MainMenuUIBuilder.GetSafeFont();
            winSubText.fontSize = 16;
            winSubText.alignment = TextAnchor.MiddleCenter;
            winSubText.color = new Color(0.9f, 0.9f, 1.0f);
            winSubObj.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 25);

            // Buttons
            nextLevelBtnComp = CreateSpaceButton("NextLevelBtn", "Go to Next Level ▶", levelCompletePanel.transform, LoadNextLevel);
            mainMenuBtnComp = CreateSpaceButton("MainMenuBtn", "Exit to Main Menu 🏠", levelCompletePanel.transform, ExitToMainMenu);
            exitDesktopBtnComp = CreateSpaceButton("ExitDesktopBtn", "Exit to Desktop 🚪", levelCompletePanel.transform, ExitToDesktop);

            levelCompletePanel.SetActive(false);
        }
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static Button CreateSpaceButton(string name, string text, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 50);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.12f, 0.4f, 0.75f);

        Button btn = btnObj.AddComponent<Button>();

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.25f, 0.12f, 0.4f, 0.75f);
        colors.highlightedColor = new Color(0.55f, 0.25f, 0.85f, 0.9f);
        colors.pressedColor = new Color(0.85f, 0.45f, 1.0f, 1.0f);
        btn.colors = colors;

        GameObject txtObj = CreateUIElement(name + "_Text", btnObj.transform);
        Text btnText = txtObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = MainMenuUIBuilder.GetSafeFont();
        btnText.fontSize = 22;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        txtObj.AddComponent<Outline>().effectColor = Color.black;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        if (action != null)
        {
            btn.onClick.AddListener(action);
        }

        return btn;
    }
}
