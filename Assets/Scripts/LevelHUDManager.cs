using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LevelHUDManager : MonoBehaviour
{
    private static LevelHUDManager _instance;
    public static LevelHUDManager Instance => _instance;

    [Header("UI Panels")]
    [SerializeField] public GameObject pauseMenuPanel;
    [SerializeField] public GameObject levelCompletePanel;
    [SerializeField] public GameObject gameOverPanel;

    private bool isPaused = false;

    private void Awake()
    {
        _instance = this;
    }

    public static LevelHUDManager EnsureInstance()
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<LevelHUDManager>();
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("LevelHUDCanvas");
                if (prefab != null)
                {
                    Instantiate(prefab);
                    _instance = FindAnyObjectByType<LevelHUDManager>();
                }
            }
        }
        return _instance;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void ShowLevelCompleteMenu()
    {
        // Unlock next level in progress
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        MainMenuManager.UnlockNextLevel(currentBuildIndex);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
        else
        {
            Invoke("LoadNextLevel", 1.5f);
        }
    }

    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        }
        else
        {
            Invoke("RestartLevel", 1.5f);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextIndex = 0; // Return to Main Menu if completed all levels
        }
        SceneManager.LoadScene(nextIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("0MainMenu");
    }
}
