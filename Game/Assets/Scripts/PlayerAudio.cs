using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAudio : MonoBehaviour
{
    private bool hasDied = false;

    public void DieWithSounds()
    {
        if (hasDied) return;
        hasDied = true;

        // Vi struntar i zombieljudet helt. 
        // Vi bara byter scen, vilket gör att musiken rullar på utan hack.
        SceneManager.LoadScene("MainViewScene");
    }
}