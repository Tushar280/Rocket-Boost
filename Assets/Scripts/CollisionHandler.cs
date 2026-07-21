using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isCrashed || isFinished)
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

    // Update logic moved to PauseMenuManager for in-game pause menu control


    private void ReloadScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }

    private void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if(currentIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            nextIndex = 0;
        }

        SceneManager.LoadScene(nextIndex);
    }

    private void GameOver()
    {
        audioSource.Stop();
        isCrashed = true;
        vfxCrash.Play();
        audioSource.PlayOneShot(explosionSound);
        Invoke("ReloadScene",delay);
        GetComponent<Movement>().enabled = false;
        
    }

    private void GameWin()
    {
        audioSource.Stop();
        vfxLanding.Play();
        isFinished = true;
        audioSource.PlayOneShot(landingSound);
        GetComponent<Movement>().enabled = false;

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
