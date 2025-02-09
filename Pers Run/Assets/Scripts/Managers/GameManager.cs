using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Ключи для PlayerPrefs
    private const string MUSIC_MUTED_KEY = "MusicMuted";
    private const string SFX_MUTED_KEY = "SFXMuted";
    private const string BEST_DISTANCE_KEY = "BestDistance"; // лучший результат по дистанции
    private const string BEST_SCORE_KEY = "BestScore";       // лучший результат по счёту

    [Header("Настройки начисления сердец и очков")]
    [SerializeField] private int pointsForExtraHeart = 100;

    [Header("UI элементы основного игрового интерфейса")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestDistanceTextUI;      // отображает лучший результат по дистанции (HUD)
    [SerializeField] private Text currentDistanceTextUI;   // отображает текущую дистанцию (HUD)
    [SerializeField] private GameObject pauseMenu;

    [Header("UI элементы панели GameOver")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text currentScoreText;              // текущий счёт за игру
    [SerializeField] private Text bestScoreTextUI;               // лучший счёт (один рекорд)
    [SerializeField] private Text gameOverDistanceTextUI;        // дистанция, пройденная за текущую игру
    [SerializeField] private Text bestDistanceGameOverTextUI;    // лучший результат по дистанции за всё время
    [SerializeField] private Text reviveCostText;
    [SerializeField] private int maxReviveCost = 3;

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

    [Header("Элементы UI для сердец")]
    [SerializeField] private Image heartIcon;
    [SerializeField] private Text heartCountText;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Sprite fullHeartSprite;

    private int score;
    private bool isGameOver = false;
    private bool isPaused = false;
    private bool isMusicMuted;
    private bool isSFXMuted;

    // Сердца для возрождения
    private int availableHearts = 0;
    private int nextHeartThreshold;
    private int reviveCount = 0;

    public event Action<int> OnScoreChanged;

    // Ссылка на игрока (для возрождения и отслеживания позиции)
    private PersRunner playerInstance;

    // --- Переменные для отслеживания дистанции ---
    private float gameStartX;      // стартовая позиция игрока по оси X в начале игры
    private float currentDistance; // дистанция, пройденная за текущую игру
    private float bestDistance;    // лучший результат по дистанции (загружается из PlayerPrefs)

    // --- Переменная для лучшего счёта ---
    private int bestScore;       // лучший результат по счёту (загружается из PlayerPrefs)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Если требуется сохранить GameManager между сценами, раскомментируйте:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        OnScoreChanged += UpdateScoreUI;
        LoadAudioSettings();
        Score = 0;
        availableHearts = 0;
        nextHeartThreshold = pointsForExtraHeart;
        UpdateHeartUI();
        gameOverPanel.SetActive(false);
        pauseMenu.SetActive(false);

        // Получаем ссылку на игрока
        playerInstance = FindObjectOfType<PersRunner>();
        if (playerInstance != null)
        {
            gameStartX = playerInstance.transform.position.x;
        }
        currentDistance = 0f;
        // Загружаем лучший результат по дистанции и счёту из PlayerPrefs
        bestDistance = PlayerPrefs.GetFloat(BEST_DISTANCE_KEY, 0f);
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        UpdateDistanceUI();

        if (!isMusicMuted && musicSource != null)
        {
            musicSource.Play();
        }
    }

    private void Update()
    {
        // Обновляем дистанцию, пока игра не окончена
        if (!isGameOver && playerInstance != null)
        {
            float newDistance = playerInstance.transform.position.x - gameStartX;
            if (newDistance > currentDistance)
            {
                currentDistance = newDistance;
                UpdateDistanceUI();
            }
        }
    }

    /// <summary>
    /// Обновляет UI элементов, связанных с дистанцией.
    /// </summary>
    private void UpdateDistanceUI()
    {
        if (currentDistanceTextUI != null)
            currentDistanceTextUI.text = "Distance: " + currentDistance.ToString("F2") + " m";
        if (bestDistanceTextUI != null)
            bestDistanceTextUI.text = "Best: " + bestDistance.ToString("F2") + " m";
    }

    public int Score
    {
        get => score;
        private set
        {
            score = value;
            OnScoreChanged?.Invoke(score);
            // Начисляем дополнительные сердца при достижении порога
            while (score >= nextHeartThreshold)
            {
                availableHearts++;
                nextHeartThreshold += pointsForExtraHeart;
            }
            UpdateHeartUI();
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + newScore;
    }

    /// <summary>
    /// Вызывается при начислении очков.
    /// </summary>
    public void AddScore(int amount)
    {
        if (!isGameOver)
        {
            Score += amount;
        }
    }

    private void UpdateHeartUI()
    {
        if (availableHearts <= 0)
        {
            heartIcon.sprite = emptyHeartSprite;
            heartCountText.text = "";
        }
        else
        {
            heartIcon.sprite = fullHeartSprite;
            heartCountText.text = availableHearts > 1 ? $"x{availableHearts}" : "";
        }
    }

    /// <summary>
    /// Вызывается при окончании игры.
    /// Обновляет лучший счёт и дистанцию, а также отображает их в панели GameOver.
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;

        if (currentScoreText != null)
            currentScoreText.text = "Score: " + Score;

        // Обновляем лучший счёт, если текущий выше
        if (Score > bestScore)
        {
            bestScore = Score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
            PlayerPrefs.Save();
        }

        // Обновляем панель GameOver:
        if (bestScoreTextUI != null)
            bestScoreTextUI.text = "Best Score: " + bestScore;
        if (gameOverDistanceTextUI != null)
            gameOverDistanceTextUI.text = "Distance: " + currentDistance.ToString("F2") + " m";
        if (bestDistanceGameOverTextUI != null)
            bestDistanceGameOverTextUI.text = "Best Distance: " + bestDistance.ToString("F2") + " m";

        int reviveCost = Mathf.Min(reviveCount + 1, maxReviveCost);
        if (reviveCostText != null)
            reviveCostText.text = "Revive Cost: " + reviveCost + " heart(s)";

        // Если текущая дистанция превышает лучший результат, обновляем его
        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetFloat(BEST_DISTANCE_KEY, bestDistance);
            PlayerPrefs.Save();
        }

        gameOverPanel.SetActive(true);

        if (musicSource != null)
            musicSource.Stop();
        if (!isMusicMuted && gameOverMusicSource != null)
            gameOverMusicSource.Play();
    }

    public void Revive()
    {
        int reviveCost = Mathf.Min(reviveCount + 1, maxReviveCost);
        if (availableHearts >= reviveCost)
        {
            availableHearts -= reviveCost;
            reviveCount++;

            gameOverPanel.SetActive(false);
            isGameOver = false;

            if (gameOverMusicSource != null && gameOverMusicSource.isPlaying)
                gameOverMusicSource.Stop();
            if (musicSource != null && !isMusicMuted)
                musicSource.Play();

            RevivePlayer();
            UpdateHeartUI();
        }
        else
        {
            ShowAdForHeart();
        }
    }

    private Transform GetLastRespawnPoint()
    {
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
        if (respawnPoints.Length == 0)
        {
            Debug.LogWarning("RespawnPoint не найден, используем (0,0,0) по умолчанию.");
            return null;
        }
        GameObject lastPoint = respawnPoints[0];
        foreach (GameObject point in respawnPoints)
        {
            if (point.transform.position.x > lastPoint.transform.position.x)
                lastPoint = point;
        }
        return lastPoint.transform;
    }

    private void RevivePlayer()
    {
        if (playerInstance == null)
            playerInstance = FindObjectOfType<PersRunner>();

        if (playerInstance != null)
        {
            Transform lastRespawnPoint = GetLastRespawnPoint();
            if (lastRespawnPoint != null)
                playerInstance.transform.position = lastRespawnPoint.position;
            else
                playerInstance.transform.position = Vector3.zero;

            playerInstance.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Игрок не найден для возрождения!");
        }
    }
 private void ShowAdForHeart()
    {
        Debug.Log("Показ рекламы для получения сердца");
        // Здесь можно интегрировать систему рекламы.
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
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
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
        isMusicMuted = PlayerPrefs.GetInt(MUSIC_MUTED_KEY, 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt(SFX_MUTED_KEY, 0) == 1;

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
        PlayerPrefs.SetInt(MUSIC_MUTED_KEY, isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt(SFX_MUTED_KEY, isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
