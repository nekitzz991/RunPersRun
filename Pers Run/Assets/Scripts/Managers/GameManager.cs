using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class GameManager : MonoBehaviour
{
    #region Singleton & Initialization
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Сделаем объект постоянным между сценами
    }
    #endregion

    #region Поля настроек и UI
    private const string BEST_DISTANCE_KEY = "BestDistance";
    private const string BEST_SCORE_KEY = "BestScore";

    [Header("Настройки начисления сердец и очков")]
    [SerializeField] private int pointsForExtraHeart = 100;

    [Header("UI элементы основного игрового интерфейса")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestDistanceTextUI;
    [SerializeField] private Text currentDistanceTextUI;
    [SerializeField] private GameObject pauseMenu;

    [Header("UI элементы панели GameOver")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text currentScoreText;
    [SerializeField] private Text bestScoreTextUI;
    [SerializeField] private Text gameOverDistanceTextUI;
    [SerializeField] private Text bestDistanceGameOverTextUI;
    [SerializeField] private Text reviveCostText;
    [SerializeField] private int maxReviveCost = 3;
    [SerializeField] private Text heartsCountGameOverText;

    [Header("Элементы UI для сердец")]
    [SerializeField] private Image heartIcon;
    [SerializeField] private Text heartCountText;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Sprite fullHeartSprite;

    [Header("Контейнер основного UI (Gameplay UI)")]
    [SerializeField] private GameObject gameplayUI;
    #endregion

    #region Локализованные строки
    [Header("Локализованные строки для текущего счета")]
    [SerializeField] private LocalizedString scoreFormat;
    [SerializeField] private LocalizeStringEvent currentDistanceLocalizeEvent;
    [SerializeField] private LocalizeStringEvent bestDistanceLocalizeEvent;
    #endregion

    #region Приватные поля
    private int score;
    private bool isGameOver = false;
    private bool isPaused = false;
    private int availableHearts = 0;
    private int nextHeartThreshold;
    private int reviveCount = 0;
    public event Action<int> OnScoreChanged;
    [SerializeField] private PersRunner playerInstance;
    private float gameStartX;
    private float currentDistance;
    private float bestDistance;
    private int bestScore;
    private Transform[] respawnPoints;
    #endregion

    #region Инициализация
    private void Start()
    {
        OnScoreChanged += UpdateScoreUI;
        AudioManager.Instance?.PlayMusic();

        Score = 0;
        availableHearts = 0;
        nextHeartThreshold = pointsForExtraHeart;
        UpdateHeartUI();

        gameOverPanel?.SetActive(false);
        pauseMenu?.SetActive(false);
        gameplayUI?.SetActive(true);

        if (playerInstance == null)
            playerInstance = FindObjectOfType<PersRunner>();
        if (playerInstance != null)
            gameStartX = playerInstance.transform.position.x;

        currentDistance = 0f;
        bestDistance = PlayerPrefs.GetFloat(BEST_DISTANCE_KEY, 0f);
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        UpdateDistanceUI();

        GameObject[] points = GameObject.FindGameObjectsWithTag("RespawnPoint");
        if (points.Length > 0)
        {
            respawnPoints = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
                respawnPoints[i] = points[i].transform;
        }
    }

    private void OnDestroy()
    {
        OnScoreChanged -= UpdateScoreUI;
    }
    #endregion

    #region Update методы
    private void Update()
    {
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
    #endregion

    #region Обновление UI

    /// <summary>
    /// Вспомогательный метод для обновления локализованного текста (улучшение 4)
    /// </summary>
    private void UpdateLocalizedText(LocalizeStringEvent localizeEvent, object value)
    {
        if (localizeEvent != null)
        {
            localizeEvent.StringReference.Arguments = new object[] { value };
            localizeEvent.RefreshString();
        }
    }

    private void UpdateDistanceUI()
    {
        int currentDistanceMeters = Mathf.RoundToInt(currentDistance);
        int bestDistanceMeters = Mathf.RoundToInt(bestDistance);

        UpdateLocalizedText(currentDistanceLocalizeEvent, currentDistanceMeters);
        UpdateLocalizedText(bestDistanceLocalizeEvent, bestDistanceMeters);
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreFormat.Arguments = new object[] { newScore };
            scoreText.text = scoreFormat.GetLocalizedString();
        }
    }

    private void UpdateHeartUI()
    {
        if (availableHearts <= 0)
        {
            if (heartIcon != null)
                heartIcon.sprite = emptyHeartSprite;
            if (heartCountText != null)
                heartCountText.text = "";
        }
        else
        {
            if (heartIcon != null)
                heartIcon.sprite = fullHeartSprite;
            if (heartCountText != null)
                heartCountText.text = availableHearts > 1 ? $"x{availableHearts}" : "";
        }
    }
    #endregion

    #region Управление очками и сердцами
    public int Score
    {
        get => score;
        private set
        {
            score = value;
            OnScoreChanged?.Invoke(score);
            while (score >= nextHeartThreshold)
            {
                availableHearts++;
                nextHeartThreshold += pointsForExtraHeart;
            }
            UpdateHeartUI();
        }
    }

    public void AddScore(int amount)
    {
        if (!isGameOver)
            Score += amount;
    }
    #endregion

    #region Игровой процесс
    public void GameOver()
    {
        isGameOver = true;

        // Скрываем основной UI при показе панели GameOver
        gameplayUI?.SetActive(false);

        // Обновляем UI панели GameOver для текущего счета (с локализацией)
        if (currentScoreText != null)
        {
            scoreFormat.Arguments = new object[] { Score };
            currentScoreText.text = scoreFormat.GetLocalizedString();
        }

        bool recordUpdated = false;
        if (Score > bestScore)
        {
            bestScore = Score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
            recordUpdated = true;
        }
        if (bestScoreTextUI != null)
        {
            bestScoreTextUI.text = bestScore.ToString();
        }
        
        // Отображаем дистанцию в виде целых чисел (в метрах) с использованием интерполяции строк (улучшение 8)
        if (gameOverDistanceTextUI != null)
        {
            int currentDistanceMeters = Mathf.RoundToInt(currentDistance);
            gameOverDistanceTextUI.text = $"{currentDistanceMeters} m";
        }
        if (bestDistanceGameOverTextUI != null)
        {
            int bestDistanceMeters = Mathf.RoundToInt(bestDistance);
            bestDistanceGameOverTextUI.text = $"{bestDistanceMeters} m";
        }
        
        int currentReviveCost = GetCurrentReviveCost();
        if (reviveCostText != null)
        {
            reviveCostText.text = currentReviveCost.ToString();
        }

        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetFloat(BEST_DISTANCE_KEY, bestDistance);
            recordUpdated = true;
        }
        if (heartsCountGameOverText != null)
        {
            heartsCountGameOverText.text = availableHearts.ToString();
        }

        if (recordUpdated)
            PlayerPrefs.Save();

        gameOverPanel?.SetActive(true);
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlayGameOverMusic();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        MenuManager.ResetGameStart();
        SceneManager.LoadScene("LoadingScene");
    }
    #endregion

    #region Управление паузой
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu?.SetActive(isPaused);

        // Скрываем или показываем основной UI в зависимости от состояния паузы
        gameplayUI?.SetActive(!isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }
    #endregion

    #region Аудио
    public void OnSomeButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClickSound();
    }

    public void ToggleMusic()
    {
        AudioManager.Instance?.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.Instance?.ToggleSFX();
    }
    #endregion

    #region Логика возрождения
    public void Revive()
    {
        int currentReviveCost = GetCurrentReviveCost();
        if (availableHearts >= currentReviveCost)
        {
            availableHearts -= currentReviveCost;
            reviveCount++;

            // Закрываем панель GameOver и возвращаем основной UI
            gameOverPanel?.SetActive(false);
            gameplayUI?.SetActive(true);

            isGameOver = false;
            AudioManager.Instance?.StopGameOverMusic();
            AudioManager.Instance?.PlayMusic();
            RevivePlayer();
            UpdateHeartUI();
        }
        else
        {
            ShowAdForHeart();
        }
    }

    /// <summary>
    /// Вычисление текущей стоимости возрождения (улучшение 6)
    /// </summary>
    private int GetCurrentReviveCost()
    {
        return Mathf.Min(reviveCount + 1, maxReviveCost);
    }

    private Transform GetLastRespawnPoint()
    {
        if (respawnPoints != null && respawnPoints.Length > 0)
        {
            Transform lastPoint = null;
            foreach (Transform point in respawnPoints)
            {
                // Пропускаем уничтожённые объекты
                if (point == null)
                    continue;

                if (lastPoint == null || point.position.x > lastPoint.position.x)
                {
                    lastPoint = point;
                }
            }
            if (lastPoint != null)
                return lastPoint;
        }
        
        // Если массив respawnPoints пуст или все объекты уничтожены, ищем заново
        GameObject[] points = GameObject.FindGameObjectsWithTag("RespawnPoint");
        if (points.Length == 0)
            return null;
        Transform lastFound = points[0].transform;
        foreach (GameObject point in points)
        {
            if (point.transform.position.x > lastFound.position.x)
                lastFound = point.transform;
        }
        return lastFound;
    }

    private void RevivePlayer()
    {
        if (playerInstance == null)
        {
            playerInstance = FindObjectOfType<PersRunner>();
            if (playerInstance == null)
                return;
        }

        Transform lastRespawnPoint = GetLastRespawnPoint();
        if (lastRespawnPoint != null)
            playerInstance.transform.position = lastRespawnPoint.position;
        else
            playerInstance.transform.position = Vector3.zero;

        playerInstance.gameObject.SetActive(true);
        playerInstance.Revive();
    }

    private void ShowAdForHeart()
    {
        Debug.Log("Показ рекламы для получения сердца");
    }
    #endregion
}
