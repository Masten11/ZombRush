using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip gameOverClip; 
    private bool hasDied = false;

    public void DieWithSounds()
{
    if (hasDied) return;
    hasDied = true;

    // 1. HITTA OCH RADERA MUSIKSPELAREN HELT
    // Vi letar efter MusicPlayer-skriptet som vi ser i din Hierarchy
    MusicPlayer musicObj = Object.FindAnyObjectByType<MusicPlayer>();
    if (musicObj != null)
    {
        // Förstör hela objektet så musiken tystnar och försvinner
        Destroy(musicObj.gameObject);
    }

    // 2. SKAPA GAME OVER-LJUDET (som tidigare)
    GameObject tempAudio = new GameObject("GameOverSound");
    AudioSource source = tempAudio.AddComponent<AudioSource>();
    source.clip = gameOverClip;
    source.volume = 1.0f; 
    DontDestroyOnLoad(tempAudio);
    source.Play();
    Destroy(tempAudio, 5f);

    // 3. LADDA MENYN DIREKT
    SceneManager.LoadScene("MainViewScene");
}
}
