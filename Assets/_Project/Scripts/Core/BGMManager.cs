using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip townBgm;
    [SerializeField] private AudioClip dungeonBgm;
    [SerializeField] private AudioClip battleBgm;

    private AudioClip currentClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Title":
            case "Prologue":
            case "ToBeContinued":
                Play(titleBgm);
                break;

            case "PlayerCreation":
            case "Town":
                Play(townBgm);
                break;

            case "Status":
                break;

            case "Dungeon":
                Play(dungeonBgm);
                break;

            case "Battle":
                Play(battleBgm);
                break;

            case "GameOver":
                Stop();
                break;

            default:
                Debug.LogWarning(
                    $"BGMÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç»Ç¢ÉVÅ[ÉìÇ≈Ç∑: {sceneName}");
                break;
        }
    }

    public void Play(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        // ìØÇ∂ã»Ç»ÇÁç≈èâÇ©ÇÁçƒê∂ÇµíºÇ≥Ç»Ç¢
        if (currentClip == clip && audioSource.isPlaying)
        {
            return;
        }

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        currentClip = null;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}