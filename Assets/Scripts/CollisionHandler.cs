using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip landingSound;

    [SerializeField] float delay = 1.5f;

    private void OnCollisionEnter(Collision collision)
    {
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
        audioSource.PlayOneShot(explosionSound);
        Invoke("ReloadScene",delay);
        GetComponent<Movement>().enabled = false;
        
    }

    private void GameWin()
    {
        audioSource.PlayOneShot(landingSound);
        Invoke("LoadNextLevel",delay);
        GetComponent<Movement>().enabled = false;
        
    }
}
