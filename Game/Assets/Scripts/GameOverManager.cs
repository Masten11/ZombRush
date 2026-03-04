using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene("background"); // Starta om spelet
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainViewScene"); // Gå till startmenyn
    }
}