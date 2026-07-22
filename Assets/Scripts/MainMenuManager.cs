using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] public GameObject mainPanel;
    [SerializeField] public GameObject levelSelectPanel;
    [SerializeField] public GameObject controlsPanel;
    [SerializeField] public GameObject optionsPanel;

    [Header("Options UI Sliders")]
    [SerializeField] public Slider masterVolumeSlider;
    [SerializeField] public Slider musicVolumeSlider;
    [SerializeField] public Slider sfxVolumeSlider;

    [Header("Level Buttons")]
    [SerializeField] public List<Button> levelButtons = new List<Button>();

    public const string UNLOCKED_LEVEL_KEY = "UnlockedLevelIndex";

    private void Start()
    {
        SettingsManager.EnsureInstance();
        ShowMainPanel();
        InitOptionsSliders();
        RefreshLevelButtons();
    }

    public static int GetUnlockedLevelIndex()
    {
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    public static void UnlockNextLevel(int completedLevelIndex)
    {
        int currentUnlocked = GetUnlockedLevelIndex();
        if (completedLevelIndex + 1 > currentUnlocked)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, completedLevelIndex + 1);
            PlayerPrefs.Save();
        }
    }

    public void RefreshLevelButtons()
    {
        int unlockedLevel = GetUnlockedLevelIndex();

        for (int i = 0; i < levelButtons.Count; i++)
        {
            Button btn = levelButtons[i];
            if (btn == null) continue;

            int levelNum = i + 1; // Level 1, 2, 3...
            bool isUnlocked = levelNum <= unlockedLevel;

            btn.interactable = isUnlocked;

            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                if (!isUnlocked)
                {
                    btnText.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                }
                else
                {
                    btnText.color = Color.white;
                }
            }
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        OpenLevelSelect();
    }

    public void StartFirstLevel()
    {
        LoadLevel("1Mercury");
    }

    public void OpenLevelSelect()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        RefreshLevelButtons();
    }

    public void OpenControls()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);

        InitOptionsSliders();
    }

    private void InitOptionsSliders()
    {
        SettingsManager settings = SettingsManager.EnsureInstance();
        if (settings == null) return;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = settings.MasterVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = settings.MusicVolume;
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = settings.SFXVolume;
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    public void OnMasterVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetMasterVolume(val);
    }

    public void OnMusicVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetMusicVolume(val);
    }

    public void OnSFXVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetSFXVolume(val);
    }

    public void LoadLevel1Mercury() => LoadLevel("1Mercury");
    public void LoadLevel2Venus() => LoadLevel("2Venus");
    public void LoadLevel3Mars() => LoadLevel("3Mars");
    public void LoadLevel4Jupiter() => LoadLevel("4jupiter");
    public void LoadLevel5Neptune() => LoadLevel("5Neptune");

    public void LoadLevel(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
        PlayerPrefs.Save();
        RefreshLevelButtons();
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Rocket Boost...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
