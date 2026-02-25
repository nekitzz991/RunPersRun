using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    #region Singleton

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Проверяем наличие единственного экземпляра AudioManager
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Еще один экземпляр AudioManager обнаружен. Уничтожаем новый объект.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Поля настроек и UI

    private const string MUSIC_MUTED_KEY = "MusicMuted";
    private const string SFX_MUTED_KEY = "SFXMuted";

    [Header("Аудио источники")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource gameOverMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Аудио клипы")]
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Элементы UI для аудио")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    private bool isMusicMuted;
    private bool isSFXMuted;

    #endregion

    #region Инициализация

    private void Start()
    {
        LoadAudioSettings();

        // Запускаем фоновую музыку, если звук включён
        if (!isMusicMuted && musicSource != null)
        {
            musicSource.Play();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Проигрывает звуковой эффект для нажатия кнопки.
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (!isSFXMuted && buttonClickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSound);
    }

    public void PlaySFXSound(AudioClip clip, float volume = 1f)
    {
        if (isSFXMuted || clip == null || sfxSource == null)
        {
            return;
        }
        sfxSource.PlayOneShot(clip, volume);
    }



    /// <summary>
    /// Переключает музыку: включает или выключает.
    /// </summary>
    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (musicSource != null)
            musicSource.mute = isMusicMuted;
        if (gameOverMusicSource != null)
            gameOverMusicSource.mute = isMusicMuted;
        if (musicIcon != null)
            musicIcon.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        SaveAudioSettings();
    }

    /// <summary>
    /// Переключает звуковые эффекты: включает или выключает.
    /// </summary>
    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        if (sfxSource != null)
            sfxSource.mute = isSFXMuted;
        if (sfxIcon != null)
            sfxIcon.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
        SaveAudioSettings();
    }

    /// <summary>
    /// Останавливает фоновую музыку.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    /// <summary>
    /// Запускает фоновую музыку, если звук не отключён.
    /// </summary>
    public void PlayMusic()
    {
        if (!isMusicMuted && musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    /// <summary>
    /// Останавливает музыку для экрана GameOver.
    /// </summary>
    public void StopGameOverMusic()
    {
        if (gameOverMusicSource != null)
            gameOverMusicSource.Stop();
    }

    /// <summary>
    /// Проигрывает музыку для экрана GameOver, если звук не отключён.
    /// </summary>
    public void PlayGameOverMusic()
    {
        if (!isMusicMuted && gameOverMusicSource != null && !gameOverMusicSource.isPlaying)
            gameOverMusicSource.Play();
    }

    #endregion

    #region Private Methods

    private void LoadAudioSettings()
    {
        isMusicMuted = SaveService.GetInt(MUSIC_MUTED_KEY, 0) == 1;
        isSFXMuted = SaveService.GetInt(SFX_MUTED_KEY, 0) == 1;

        if (musicSource != null)
            musicSource.mute = isMusicMuted;
        if (gameOverMusicSource != null)
            gameOverMusicSource.mute = isMusicMuted;
        if (sfxSource != null)
            sfxSource.mute = isSFXMuted;

        if (musicIcon != null)
            musicIcon.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        if (sfxIcon != null)
            sfxIcon.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
    }

    private void SaveAudioSettings()
    {
        SaveService.SetInt(MUSIC_MUTED_KEY, isMusicMuted ? 1 : 0);
        SaveService.SetInt(SFX_MUTED_KEY, isSFXMuted ? 1 : 0);
        SaveService.SaveNow();
    }

    #endregion
}
