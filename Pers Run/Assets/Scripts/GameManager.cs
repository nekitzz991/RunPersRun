using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Константы для PlayerPrefs (для аудио настроек) и рекордов
    private const string MUSIC_MUTED_KEY = "MusicMuted";
    private const string SFX_MUTED_KEY = "SFXMuted";
    private const int NUM_HIGH_SCORES = 5; // число сохраняемых рекордов

    [Header("Настройки начисления сердец")]
    [SerializeField] private int pointsForExtraHeart = 100;

    [Header("UI элементы")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenu;

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
    [SerializeField] private Image heartIcon;            // Иконка сердца
    [SerializeField] private Text heartCountText;          // Текст с количеством сердец
    [SerializeField] private Sprite emptyHeartSprite;      // Силуэт сердца (если нет доступных)
    [SerializeField] private Sprite fullHeartSprite;       // Полное сердце (если есть доступные)

    [Header("Настройки Game Over")]
    [SerializeField] private Text currentScoreText;
    [SerializeField] private Text highScoreText; // сюда выводится таблица рекордов
    [SerializeField] private Text reviveCostText;
    [SerializeField] private int maxReviveCost = 3; // максимум сердец за возрождение

    private int score;
    private bool isGameOver = false;
    private bool isPaused = false;
    private bool isMusicMuted;
    private bool isSFXMuted;
    
    // Заработанные и доступные для возрождения сердца
    private int availableHearts = 0;
    // Следующий порог для начисления дополнительного сердца
    private int nextHeartThreshold;
    
    private int reviveCount = 0; // сколько раз игрок возрождался в текущей сессии

    public event Action<int> OnScoreChanged;

    // Кэш игрока для возрождения
    private PersRunner playerInstance;

    private void Awake()
    {
        // Реализация Singleton: если экземпляр уже есть, уничтожаем новый
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Если требуется сохранять GameManager между сценами, раскомментируйте:
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

        // Кэшируем ссылку на игрока
        playerInstance = FindObjectOfType<PersRunner>();

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
            // Начисление дополнительных сердец при достижении порога
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

    public void GameOver()
    {
        isGameOver = true;

        if (currentScoreText != null)
            currentScoreText.text = "Score: " + Score;

        // Обновляем таблицу рекордов с учетом текущего счёта
        UpdateHighScores(Score);
        int[] highScores = GetHighScores();
        if (highScoreText != null)
        {
            highScoreText.text = FormatHighScoresText(highScores);
        }

        // Рассчитываем стоимость возрождения (увеличивается с каждой смертью, но не более maxReviveCost)
        int reviveCost = Mathf.Min(reviveCount + 1, maxReviveCost);
        if (reviveCostText != null)
            reviveCostText.text = "Revive Cost: " + reviveCost + " heart(s)";

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
            RevivePlayer();
            UpdateHeartUI();
        }
        else
        {
            // Если недостаточно сердец – можно показать рекламу
            ShowAdForHeart();
        }
    }

    /// <summary>
    /// Ищет все объекты с тегом "RespawnPoint" и возвращает тот, который находится дальше (например, по оси X).
    /// При необходимости логику выбора можно изменить.
    /// </summary>
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
            // Предположим, что «последняя» точка – та, у которой больше координата X.
            if (point.transform.position.x > lastPoint.transform.position.x)
            {
                lastPoint = point;
            }
        }
        return lastPoint.transform;
    }

    private void RevivePlayer()
    {
        // Используем кэшированную ссылку на игрока, если её нет, ищем снова
        if (playerInstance == null)
        {
            playerInstance = FindObjectOfType<PersRunner>();
        }
        if (playerInstance != null)
        {
            Transform lastRespawnPoint = GetLastRespawnPoint();
            if (lastRespawnPoint != null)
            {
                playerInstance.transform.position = lastRespawnPoint.position;
            }
            else
            {
                playerInstance.transform.position = Vector3.zero;
            }
            // Если игрок был деактивирован после смерти, активируем его
            playerInstance.gameObject.SetActive(true);

            // Здесь можно добавить сброс состояния игрока, например, восстановление здоровья
            // или вызов метода playerInstance.Revive();
        }
        else
        {
            Debug.LogWarning("Игрок не найден для возрождения!");
        }
    }

    private int[] GetHighScores()
    {
        int[] scores = new int[NUM_HIGH_SCORES];
        for (int i = 0; i < NUM_HIGH_SCORES; i++)
        {
            scores[i] = PlayerPrefs.GetInt("HighScore" + i, 0);
        }
        return scores;
    }

    private void SaveHighScores(int[] scores)
    {
        for (int i = 0; i < NUM_HIGH_SCORES; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, scores[i]);
        }
        PlayerPrefs.Save();
    }

    private void UpdateHighScores(int currentScore)
    {
        int[] scores = GetHighScores();
        // Если текущий счёт больше хотя бы одного из сохранённых рекордов, вставляем его на нужное место.
        for (int i = 0; i < NUM_HIGH_SCORES; i++)
        {
            if (currentScore > scores[i])
            {
                // Сдвигаем нижние результаты вниз
                for (int j = NUM_HIGH_SCORES - 1; j > i; j--)
                {
                    scores[j] = scores[j - 1];
                }
                scores[i] = currentScore;
                break;
            }
        }
        SaveHighScores(scores);
    }

    private string FormatHighScoresText(int[] scores)
    {
        string text = "";
        for (int i = 0; i < NUM_HIGH_SCORES; i++)
        {
            text += (i + 1) + ". " + scores[i] + "\n";
        }
        return text;
    }

    private void ShowAdForHeart()
    {
        Debug.Log("Показ рекламы для получения сердца");
        // Здесь можно интегрировать систему рекламы.
        // После просмотра рекламы увеличить availableHearts и обновить UI:
        // availableHearts++;
        // UpdateHeartUI();
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
