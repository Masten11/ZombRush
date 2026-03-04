using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    void Awake()
    {
        // Om det redan finns en spelare, ta bort den här nya direkt
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // Detta är nyckeln: Musiken rörs aldrig vid scenbyten
        DontDestroyOnLoad(gameObject);
    }
}