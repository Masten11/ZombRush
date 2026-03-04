using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAudio : MonoBehaviour
{
    private bool hasDied = false;

    public void DieWithSounds()
{
    if (hasDied) return;
    hasDied = true;
    
    // Här skickar vi spelaren till den nya scenen
    SceneManager.LoadScene("GameOverScene");
}
}