using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenu;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource gameOverMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Sound UI Elements")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    private int score;
    private bool isGameOver = false;
    private bool isPaused = false;
    private bool isMusicMuted;
    private bool isSFXMuted;

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        OnScoreChanged += UpdateScoreUI;
        LoadAudioSettings();
        Score = 0;
        gameOverPanel.SetActive(false);
        pauseMenu.SetActive(false);

        if (!isMusicMuted && musicSource != null)
        {
            musicSource.Play();
        }
    }

    public int Score
    {
        get => score;
        private set
        {
            score = value;
            OnScoreChanged?.Invoke(score);
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }

    public void AddScore(int amount)
    {
        if (!isGameOver)
        {
            Score += amount;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over!");

        if (musicSource != null)
            musicSource.Stop();

        if (!isMusicMuted && gameOverMusicSource != null)
            gameOverMusicSource.Play();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

   public void ExitGame()
{
    Debug.Log("Exit Game");

    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;  // Завершаем игру в редакторе
    #else
    Application.Quit();  // Закрытие игры на реальной сборке
    #endif
}


    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }
    public void PlayButtonClickSound()
    {
        if (!isSFXMuted && buttonClickSound != null)
            sfxSource.PlayOneShot(buttonClickSound);
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        musicSource.mute = isMusicMuted;
        gameOverMusicSource.mute = isMusicMuted;
        musicIcon.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        SaveAudioSettings();
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        sfxSource.mute = isSFXMuted;
        sfxIcon.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
        SaveAudioSettings();
    }

    private void LoadAudioSettings()
    {
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        musicSource.mute = isMusicMuted;
        gameOverMusicSource.mute = isMusicMuted;
        sfxSource.mute = isSFXMuted;

        if (musicIcon != null)
            musicIcon.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        
        if (sfxIcon != null)
            sfxIcon.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFXMuted", isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
