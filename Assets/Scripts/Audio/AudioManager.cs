using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip shockwaveSound;
    [SerializeField] private AudioClip glassBreakSound;
    [SerializeField] private AudioClip woodBreakSound;
    [SerializeField] private AudioClip perfectSound;
    [SerializeField] private AudioClip pulseReadySound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip startSound;

    [Header("UI SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip menuSound;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        ConfigureSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void Start()
    {
        // Play music for the scene the game starts in.
        PlayMusicForCurrentScene();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }


    private void PlayMusicForCurrentScene()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        Debug.Log(
            "AudioManager: Scene loaded = " +
            sceneName
        );

        if (sceneName == "MainMenu")
        {
            PlayMenuMusic();
        }
        else if (sceneName == "Level")
        {
            PlayGameplayMusic();
        }
    }


    private void ConfigureSources()
    {
        if (musicSource == null)
        {
            Debug.LogError(
                "AudioManager: Music Source is missing!"
            );
        }
        else
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.spatialBlend = 0f;
        }


        if (sfxSource == null)
        {
            Debug.LogError(
                "AudioManager: SFX Source is missing!"
            );
        }
        else
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
            sfxSource.spatialBlend = 0f;
        }
    }


    // ==================================================
    // MUSIC
    // ==================================================

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }


    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }


    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogError(
                "AudioManager: Music Source missing!"
            );
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                "AudioManager: Music clip is missing."
            );
            return;
        }

        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.loop = true;

        musicSource.Play();

        Debug.Log(
            "AudioManager: Playing music -> " +
            clip.name
        );
    }


    // ==================================================
    // GAMEPLAY SFX
    // ==================================================

    public void PlayShockwave()
    {
        PlaySFX(shockwaveSound);
    }


    public void PlayGlassBreak(Vector3 position)
    {
        PlaySFX(glassBreakSound);
    }


    public void PlayWoodBreak(Vector3 position)
    {
        PlaySFX(woodBreakSound);
    }


    public void PlayPerfect()
    {
        PlaySFX(perfectSound);
    }


    public void PlayPulseReady()
    {
        PlaySFX(pulseReadySound);
    }


    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }


    public void PlayStart()
    {
        PlaySFX(startSound);
    }


    // ==================================================
    // UI
    // ==================================================

    public void PlayClick()
    {
        PlaySFX(clickSound);
    }


    public void PlayMenu()
    {
        PlaySFX(menuSound);
    }


    // ==================================================
    // INTERNAL
    // ==================================================

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError(
                "AudioManager: SFX Source missing!"
            );
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                "AudioManager: SFX clip is missing."
            );
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            sfxVolume
        );
    }
}