using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private AudioClip gameOverClip;

    private bool hasDied = false;

    private void Awake()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
    }

    public void DieWithSounds()
    {
        if (hasDied) return;
        hasDied = true;

        sfxSource.PlayOneShot(crashClip, 2.0f);
        Invoke(nameof(PlayGameOver), 0.4f);
        StartCoroutine(LoadSceneAfterDelay());
    }

    private void PlayGameOver()
    {
        sfxSource.PlayOneShot(gameOverClip, 1.8f);
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(2.7f);
        SceneManager.LoadScene("MainViewScene");
    }

    private void Update()
{
    if (Input.GetKeyDown(KeyCode.P))
    {
        Debug.Log("P tryckt - spelar crashClip");
        sfxSource.PlayOneShot(crashClip);
    }
}
}