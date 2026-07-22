using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer (Optional)")]
    [SerializeField] private AudioMixer mainAudioMixer;

    // Keys for PlayerPrefs
    private const string MASTER_VOL_KEY = "MasterVolume";
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    private const string QUALITY_KEY = "GraphicsQuality";
    private const string FULLSCREEN_KEY = "IsFullscreen";

    public float MasterVolume { get; private set; } = 1.0f;
    public float MusicVolume { get; private set; } = 0.8f;
    public float SFXVolume { get; private set; } = 0.8f;
    public int QualityLevel { get; private set; } = 2; // Default High
    public bool IsFullscreen { get; private set; } = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static SettingsManager EnsureInstance()
    {
        if (Instance != null) return Instance;

        SettingsManager existing = FindObjectOfType<SettingsManager>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject obj = new GameObject("SettingsManager");
        Instance = obj.AddComponent<SettingsManager>();
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

        LoadSettings();
    }

    public void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1.0f);
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.8f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.8f);
        QualityLevel = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.names.Length > 0 ? QualitySettings.names.Length - 1 : 2);
        IsFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;

        ApplySettings();
    }

    public void ApplySettings()
    {
        // Apply Global Audio Listener Volume
        AudioListener.volume = MasterVolume;

        // If using AudioMixer
        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("MasterVol", ConvertToDecibels(MasterVolume));
            mainAudioMixer.SetFloat("MusicVol", ConvertToDecibels(MusicVolume));
            mainAudioMixer.SetFloat("SFXVol", ConvertToDecibels(SFXVolume));
        }

        // Apply Quality Settings
        if (QualityLevel >= 0 && QualityLevel < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(QualityLevel, true);
        }

        // Apply Fullscreen
        Screen.fullScreen = IsFullscreen;
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, MasterVolume);
        AudioListener.volume = MasterVolume;

        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("MasterVol", ConvertToDecibels(MasterVolume));
        }
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, MusicVolume);

        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("MusicVol", ConvertToDecibels(MusicVolume));
        }
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, SFXVolume);

        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("SFXVol", ConvertToDecibels(SFXVolume));
        }
        PlayerPrefs.Save();
    }

    public void SetQualityLevel(int index)
    {
        QualityLevel = index;
        PlayerPrefs.SetInt(QUALITY_KEY, QualityLevel);
        QualitySettings.SetQualityLevel(QualityLevel, true);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        IsFullscreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, IsFullscreen ? 1 : 0);
        Screen.fullScreen = IsFullscreen;
        PlayerPrefs.Save();
    }

    public void SetResolution(int width, int height, bool fullscreen)
    {
        Screen.SetResolution(width, height, fullscreen);
    }

    private float ConvertToDecibels(float linearVolume)
    {
        return linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
    }
}
