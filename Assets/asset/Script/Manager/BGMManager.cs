
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 씬 기준으로 강제로 한 번 BGM 재생
        PlayBGMForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    void PlayBGMForScene(string sceneName)
    {
        // 예시: 씬 이름에 따라 다른 BGM 재생
        switch (sceneName)
        {
            case "Title":
                Play(bgmClips[0]);
                break;
            case "Store":
                Play(bgmClips[1]);
                break;
            case "BossScene":
                Play(bgmClips[2]);
                break;
            case "Finalboss":
                Play(bgmClips[3]);
                break;
            
            default:
                Play(null);
                break;
        }
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void Stop()
    {
        bgmSource.Stop();
    }
}
