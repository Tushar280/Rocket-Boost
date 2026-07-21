using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject soundOptionsPanel;

    [Header("Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "0MainMenu";
    [SerializeField] private int mainMenuBuildIndex = 0;
    [SerializeField] private bool loadByBuildIndex = false;

    [Header("Audio Sliders (Optional UI Wiring)")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    public bool IsPaused { get; private set; } = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitPauseMenu()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "0MainMenu" || activeScene.buildIndex == 0)
        {
            return;
        }

        if (Instance == null && FindObjectOfType<PauseMenuManager>() == null)
        {
            GameObject pauseObj = new GameObject("PauseMenuManager");
            pauseObj.AddComponent<PauseMenuManager>();
        }
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

        if (pauseMenuPanel == null)
        {
            PauseMenuUIBuilder.EnsureUI(this);
        }
    }

    private void Start()
    {
        ResumeGame(); // Ensure game runs on start
    }

    private void Update()
    {
        // Toggle Pause menu when ESC is pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        // Don't pause if on main menu
        if (SceneManager.GetActiveScene().name == "0MainMenu" || SceneManager.GetActiveScene().buildIndex == 0)
        {
            return;
        }

        if (IsPaused)
        {
            if (soundOptionsPanel != null && soundOptionsPanel.activeSelf)
            {
                CloseSoundOptions();
            }
            else
            {
                ResumeGame();
            }
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0.0f;

        if (pauseMenuPanel == null)
        {
            PauseMenuUIBuilder.EnsureUI(this);
        }

        Transform parentTransform = pauseMenuPanel != null ? pauseMenuPanel.transform.parent : null;
        if (parentTransform != null) parentTransform.gameObject.SetActive(true);

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (soundOptionsPanel != null) soundOptionsPanel.SetActive(false);

        InitializeAudioSliders();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1.0f;

        if (pauseMenuPanel != null)
        {
            Transform parentTransform = pauseMenuPanel.transform.parent;
            if (parentTransform != null && parentTransform.name.Contains("Backdrop"))
            {
                parentTransform.gameObject.SetActive(false);
            }
            pauseMenuPanel.SetActive(false);
        }

        if (soundOptionsPanel != null) soundOptionsPanel.SetActive(false);
    }

    public void OpenSoundOptions()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (soundOptionsPanel != null) soundOptionsPanel.SetActive(true);

        InitializeAudioSliders();
    }

    public void CloseSoundOptions()
    {
        if (soundOptionsPanel != null) soundOptionsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1.0f; // Restore normal time scale before scene change
        IsPaused = false;

        if (loadByBuildIndex)
        {
            SceneManager.LoadScene(mainMenuBuildIndex);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void ExitToDesktop()
    {
        Debug.Log("Exiting Rocket Boost to Desktop...");
        Application.Quit();
    }

    private void InitializeAudioSliders()
    {
        if (SettingsManager.Instance != null)
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = SettingsManager.Instance.MasterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = SettingsManager.Instance.MusicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = SettingsManager.Instance.SFXVolume;
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMasterVolume(volume);
        }
        else
        {
            AudioListener.volume = volume;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMusicVolume(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSFXVolume(volume);
        }
    }
}
