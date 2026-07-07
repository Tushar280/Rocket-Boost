using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("This is a friendly object");
                break;
            case "Finish":
                Debug.Log("This is the finish");
                LoadNextLevel();
                break;
            case "Fuel":
                Debug.Log("This is fuel");
                break;
            default:
                Debug.Log("This is an enemy object");
                ReloadScene();
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
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextIndex);
    }   
}
