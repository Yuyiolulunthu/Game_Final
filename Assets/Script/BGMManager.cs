using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    [SerializeField] private List<SceneBGM> sceneBGMList = new List<SceneBGM>();
    private Dictionary<string, AudioClip> bgmDict;

    void Awake()
    {
        Debug.Log($"[BGM] Awake called in scene: {SceneManager.GetActiveScene().name}, instance exists: {instance != null}");
        
        if (instance != null)
        {
            Debug.Log($"[BGM] Merging {sceneBGMList.Count} settings from this scene");
            instance.MergeSettings(sceneBGMList);
            instance.PlayBGMForScene(SceneManager.GetActiveScene().name);
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var item in sceneBGMList)
        {
            Debug.Log($"[BGM] Adding to dict: {item.sceneName} -> {item.bgmClip?.name}");
            bgmDict[item.sceneName] = item.bgmClip;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayBGMForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[BGM] OnSceneLoaded: {scene.name}");
        PlayBGMForScene(scene.name);
    }

    private void MergeSettings(List<SceneBGM> otherList)
    {
        foreach (var item in otherList)
        {
            if (!bgmDict.ContainsKey(item.sceneName))
            {
                Debug.Log($"[BGM] Merged: {item.sceneName} -> {item.bgmClip?.name}");
                bgmDict[item.sceneName] = item.bgmClip;
            }
        }
    }

    public void PlayBGMForScene(string sceneName)
    {
        Debug.Log($"[BGM] PlayBGMForScene: {sceneName}, dict count: {bgmDict.Count}");
        
        // 列出所有 key
        foreach (var key in bgmDict.Keys)
        {
            Debug.Log($"[BGM] Dict key: '{key}'");
        }
        
        if (bgmDict.TryGetValue(sceneName, out AudioClip clip))
        {
            Debug.Log($"[BGM] Found clip: {clip?.name}");
            PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning($"[BGM] No BGM for scene: '{sceneName}'");
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        
        Debug.Log($"[BGM] PlayBGM: {clip.name}, current: {audioSource.clip?.name}");

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
        
        Debug.Log($"[BGM] Now playing: {clip.name}");
    }

    public static void RestartBGM()
    {
        if (instance != null && instance.audioSource != null)
        {
            instance.audioSource.Stop();
            instance.audioSource.time = 0f;
            instance.audioSource.Play();
        }
    }

    public static void Play(AudioClip clip)
    {
        if (instance != null)
        {
            instance.PlayBGM(clip);
        }
    }
}