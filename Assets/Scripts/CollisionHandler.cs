using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip landingSound;

    bool isCrashed = false;
    bool isFinished = false;

    [SerializeField] private ParticleSystem vfxCrash;
    [SerializeField] private ParticleSystem vfxLanding;

    [SerializeField] float delay = 1.5f;

    private void Start()
    {
        LevelHUDManager.EnsureInstance();
        SettingsManager.EnsureInstance();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCrashed || isFinished)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("This is a friendly object");
                break;
            case "Finish":
                Debug.Log("This is the finish");
                GameWin();
                break;
            case "Fuel":
                Debug.Log("This is fuel");
                break;
            default:
                Debug.Log("This is an enemy object");
                GameOver();
                break;
        }
    }

    private void ReloadScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private void GameOver()
    {
        float sfxVol = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = sfxVol;
            if (explosionSound != null)
            {
                audioSource.PlayOneShot(explosionSound, sfxVol);
            }
        }

        isCrashed = true;
        if (vfxCrash != null) vfxCrash.Play();
        Invoke("ReloadScene", delay);

        Movement mv = GetComponent<Movement>();
        if (mv != null) mv.enabled = false;
    }

    private void GameWin()
    {
        float sfxVol = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = sfxVol;
            if (landingSound != null)
            {
                audioSource.PlayOneShot(landingSound, sfxVol);
            }
        }

        if (vfxLanding != null) vfxLanding.Play();
        isFinished = true;

        Movement mv = GetComponent<Movement>();
        if (mv != null) mv.enabled = false;

        LevelHUDManager hud = LevelHUDManager.EnsureInstance();
        if (hud != null)
        {
            hud.ShowLevelCompleteMenu();
        }
        else
        {
            Invoke("LoadNextLevel", delay);
        }
    }
}
