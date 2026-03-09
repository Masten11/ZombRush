using UnityEngine;
using UnityEngine.SceneManagement; 

// Manages pausing, resuming, and restarting the game using Unity's TimeScale
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel; // The large pause menu UI
    public GameObject pauseButton;    // The small on-screen pause button (optional)

    private bool isPaused = false;

    void Start()
    {
        // Always ensure time is running normally when a scene first loads.
        // If you restart while paused, the game will stay permanently frozen without this!
        Time.timeScale = 1f; 
    }

    void Update()
    {
        // Allow the player to toggle the pause menu using the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // Called by the on-screen Pause Button or the Escape key
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        
        // Time.timeScale = 0 freezes all physics, movement, and animations
        Time.timeScale = 0f; 
        isPaused = true;
    }

    // Called by the "Resume" button on the pause menu or the Escape key
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        
        // Return time to normal speed
        Time.timeScale = 1f; 
        isPaused = false;
    }

    // Called by a "Restart" or "Try Again" UI button
    public void RestartGame()
    {
        // Crucial: You must unfreeze time before loading a scene, 
        // otherwise the new scene will load completely frozen!
        Time.timeScale = 1f; 
        
        // Reloads the level you are currently playing
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}