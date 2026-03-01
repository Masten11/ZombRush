using UnityEngine;
using UnityEngine.SceneManagement; // Useful if you add a restart/menu button

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject pauseButton; // Optional: to hide the pause button while paused

    private bool isPaused = false;

    void Start()
    {
        // Safety net: Ensures the game doesn't stay frozen if you restart the scene while paused.
        Time.timeScale = 1f; 
    }

    void Update()
    {
        // Optional: Allow the player to pause using the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        
        Time.timeScale = 0f; // Freezes the game
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        
        Time.timeScale = 1f; // Resumes the game
        isPaused = false;
    }

    // Optional helper for a "Main Menu" or "Restart" button
    public void RestartGame()
    {
        Time.timeScale = 1f; // ALWAYS reset time scale before loading a scene!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}