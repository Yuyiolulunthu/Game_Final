using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    public static void RestartBGM()
    {
        if (instance != null && instance.audioSource != null)
        {
            instance.audioSource.Stop();
            instance.audioSource.Play();
        }
    }
}