using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip landingSound;

    bool isCrashed = false;
    bool isFinished = false;

    [SerializeField] private ParticleSystem vfxCrash;
    [SerializeField] private ParticleSystem vfxLanding;

    [SerializeField] float delay = 1.2f;

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

    private void TriggerGameOverUI()
    {
        LevelHUDManager hud = LevelHUDManager.EnsureInstance();
        if (hud != null)
        {
            hud.ShowGameOverMenu();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void TriggerLevelCompleteUI()
    {
        LevelHUDManager hud = LevelHUDManager.EnsureInstance();
        if (hud != null)
        {
            hud.ShowLevelCompleteMenu();
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                nextSceneIndex = 0;
            }
            SceneManager.LoadScene(nextSceneIndex);
        }
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
        Invoke("TriggerGameOverUI", delay);

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

        Invoke("TriggerLevelCompleteUI", delay);
    }
}
