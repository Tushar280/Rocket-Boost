using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject soundSettingsSubPanel;
    [SerializeField] private GameObject graphicsSettingsSubPanel;

    [Header("Level Loading")]
    [SerializeField] private string firstLevelName = "1Mercury";
    [SerializeField] private int firstLevelBuildIndex = 1;
    [SerializeField] private bool loadByBuildIndex = false;

    [Header("Sound Controls (Optional UI Wiring)")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics Controls (Optional UI Wiring)")]
    [SerializeField] private Toggle fullscreenToggle;

    private void Awake()
    {
        if (mainPanel == null)
        {
            MainMenuUIBuilder.EnsureUI(this);
        }
    }

    private void Start()
    {
        // Show Main Panel by default
        ShowMainPanel();

        // Initialize UI controls with saved settings
        InitializeUIValues();
    }

    private void InitializeUIValues()
    {
        if (SettingsManager.Instance != null)
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = SettingsManager.Instance.MasterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = SettingsManager.Instance.MusicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = SettingsManager.Instance.SFXVolume;
            if (fullscreenToggle != null) fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen;
        }
    }

    #region Navigation Methods

    public void PlayGame()
    {
        Time.timeScale = 1.0f;
        if (loadByBuildIndex)
        {
            SceneManager.LoadScene(firstLevelBuildIndex);
        }
        else
        {
            SceneManager.LoadScene(firstLevelName);
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void CloseControls()
    {
        ShowMainPanel();
    }

    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);

        ShowSoundSettingsTab(); // Default to sound tab in options
    }

    public void CloseOptions()
    {
        ShowMainPanel();
    }

    public void ShowSoundSettingsTab()
    {
        if (soundSettingsSubPanel != null) soundSettingsSubPanel.SetActive(true);
        if (graphicsSettingsSubPanel != null) graphicsSettingsSubPanel.SetActive(false);
    }

    public void ShowGraphicsSettingsTab()
    {
        if (soundSettingsSubPanel != null) soundSettingsSubPanel.SetActive(false);
        if (graphicsSettingsSubPanel != null) graphicsSettingsSubPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Space Rocket Boost...");
        Application.Quit();
    }

    #endregion

    #region Sound Settings Callbacks

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

    #endregion

    #region Graphics Settings Callbacks

    public void SetQualityLevel(int index)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetQualityLevel(index);
        }
        else
        {
            QualitySettings.SetQualityLevel(index, true);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetFullscreen(isFullscreen);
        }
        else
        {
            Screen.fullScreen = isFullscreen;
        }
    }

    #endregion
}
