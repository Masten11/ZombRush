using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerAudio : MonoBehaviour
{
    private bool hasDied = false;

    public bool HasDied => hasDied;

    public void MarkDead()
    {
        if (hasDied) return;
        hasDied = true;
    }

    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    public IEnumerator LoadGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameOverScene");
    }
}