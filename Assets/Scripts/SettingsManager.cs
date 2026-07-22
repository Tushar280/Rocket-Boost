using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager _instance;
    public static SettingsManager Instance => _instance;

    public float MasterVolume { get; private set; } = 1.0f;
    public float MusicVolume { get; private set; } = 0.8f;
    public float SFXVolume { get; private set; } = 0.8f;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    public static SettingsManager EnsureInstance()
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<SettingsManager>();
            if (_instance == null)
            {
                GameObject obj = new GameObject("SettingsManager");
                _instance = obj.AddComponent<SettingsManager>();
            }
        }
        return _instance;
    }

    public void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1.0f);
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 0.8f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.8f);
        ApplyAudioSettings();
    }

    public void SetMasterVolume(float val)
    {
        MasterVolume = Mathf.Clamp01(val);
        PlayerPrefs.SetFloat(MASTER_KEY, MasterVolume);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float val)
    {
        MusicVolume = Mathf.Clamp01(val);
        PlayerPrefs.SetFloat(MUSIC_KEY, MusicVolume);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float val)
    {
        SFXVolume = Mathf.Clamp01(val);
        PlayerPrefs.SetFloat(SFX_KEY, SFXVolume);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        AudioListener.volume = MasterVolume;
    }
}
