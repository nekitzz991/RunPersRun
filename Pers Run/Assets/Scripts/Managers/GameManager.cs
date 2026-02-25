using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
    }

    private void OnEnable()
    {
        pauseAction?.Enable();
    }

    private void OnDisable()
    {
        pauseAction?.Disable();
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
    [SerializeField] private Button reviveButton;

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

    #region Ссылки на аниматоры окон
    [Header("Аниматоры окон (с контроллером состояний Hidden, Show, Hide)")]
    [SerializeField] private Animator pauseAnimator;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private Selectable pauseFirstSelectable;
    [SerializeField] private Selectable gameOverFirstSelectable;
    #endregion

    #region Приватные поля
    private bool isGameOver;
    private bool isPaused;
    private RunStatsService statsService;
    private InputAction pauseAction;

    public event Action<int> OnScoreChanged;

    [SerializeField] private PersRunner playerInstance;
    private float gameStartX;
    private float currentDistance;
    private float bestDistance;

    private Vector3 startPosition;
    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint;
    #endregion

    #region Инициализация
    private void Start()
    {
        InitializeGameplayState();
        InitializeStats();
        InitializeUI();

        AudioManager.Instance?.PlayMusic();
        InvokeRepeating(nameof(SaveProgress), 5f, 5f);
        WebGLBeforeUnloadBridge.Register(this, nameof(OnBeforeUnload));
    }

    private void OnDestroy()
    {
        if (statsService != null)
        {
            statsService.ScoreChanged -= HandleScoreChanged;
        }

        CancelInvoke(nameof(SaveProgress));
        pauseAction?.Dispose();
        pauseAction = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeGameplayState()
    {
        if (playerInstance == null)
        {
            playerInstance = FindFirstObjectByType<PersRunner>();
        }

        if (playerInstance != null)
        {
            startPosition = playerInstance.transform.position;
            gameStartX = playerInstance.transform.position.x;
        }
        else
        {
            Debug.LogWarning("GameManager: PersRunner не найден на сцене.");
        }

        isGameOver = false;
        isPaused = false;
        hasCheckpoint = false;
        currentDistance = 0f;
        bestDistance = SaveService.GetFloat(BEST_DISTANCE_KEY, 0f);
    }

    private void InitializeStats()
    {
        int storedBestScore = SaveService.GetInt(BEST_SCORE_KEY, 0);
        statsService = new RunStatsService(pointsForExtraHeart, maxReviveCost, storedBestScore);
        statsService.ScoreChanged += HandleScoreChanged;
        statsService.ResetSession();
        UpdateHeartUI();
    }

    private void InitializeUI()
    {
        gameOverPanel?.SetActive(false);
        pauseMenu?.SetActive(false);
        gameplayUI?.SetActive(true);
        UpdateDistanceUI();
    }
    #endregion

    #region Update методы
    private void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }

        if (isGameOver || playerInstance == null)
        {
            return;
        }

        float newDistance = playerInstance.transform.position.x - gameStartX;
        if (newDistance > currentDistance)
        {
            currentDistance = newDistance;
            UpdateDistanceUI();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveService.SaveNow();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveService.SaveNow();
        }
    }
    #endregion

    #region Обновление UI
    private void HandleScoreChanged(int newScore)
    {
        UpdateScoreUI(newScore);
        UpdateHeartUI();
        OnScoreChanged?.Invoke(newScore);
    }

    private void UpdateLocalizedText(LocalizeStringEvent localizeEvent, object value)
    {
        if (localizeEvent == null)
        {
            return;
        }

        localizeEvent.StringReference.Arguments = new object[] { value };
        localizeEvent.RefreshString();
    }

    private void UpdateDistanceUI()
    {
        int currentDistanceMeters = Mathf.RoundToInt(currentDistance);
        int bestDistanceMeters = Mathf.RoundToInt(bestDistance);

        if (currentDistanceTextUI != null)
        {
            currentDistanceTextUI.text = $"{currentDistanceMeters} m";
        }
        if (bestDistanceTextUI != null)
        {
            bestDistanceTextUI.text = $"{bestDistanceMeters} m";
        }

        UpdateLocalizedText(currentDistanceLocalizeEvent, currentDistanceMeters);
        UpdateLocalizedText(bestDistanceLocalizeEvent, bestDistanceMeters);

        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            SaveService.SetFloat(BEST_DISTANCE_KEY, bestDistance);
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText == null)
        {
            return;
        }

        if (scoreFormat != null)
        {
            scoreFormat.Arguments = new object[] { newScore };
            scoreText.text = scoreFormat.GetLocalizedString();
            return;
        }

        scoreText.text = newScore.ToString();
    }

    private void UpdateHeartUI()
    {
        int hearts = statsService == null ? 0 : statsService.AvailableHearts;

        if (hearts <= 0)
        {
            if (heartIcon != null)
            {
                heartIcon.sprite = emptyHeartSprite;
            }
            if (heartCountText != null)
            {
                heartCountText.text = string.Empty;
            }
            return;
        }

        if (heartIcon != null)
        {
            heartIcon.sprite = fullHeartSprite;
        }
        if (heartCountText != null)
        {
            heartCountText.text = hearts > 1 ? $"x{hearts}" : string.Empty;
        }
    }
    #endregion

    #region Управление очками и сердцами
    public int Score => statsService == null ? 0 : statsService.Score;

    public void AddScore(int amount)
    {
        if (isGameOver || statsService == null)
        {
            return;
        }

        int previousBestScore = statsService.BestScore;
        statsService.AddScore(amount);

        if (statsService.BestScore > previousBestScore)
        {
            SaveService.SetInt(BEST_SCORE_KEY, statsService.BestScore);
        }
    }
    #endregion

    #region Игровой процесс
    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        gameplayUI?.SetActive(false);

        if (playerInstance != null)
        {
            playerInstance.enabled = false;
            var rb = playerInstance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }
        }

        if (currentScoreText != null)
        {
            currentScoreText.text = Score.ToString();
        }

        if (bestScoreTextUI != null && statsService != null)
        {
            bestScoreTextUI.text = statsService.BestScore.ToString();
        }

        if (gameOverDistanceTextUI != null)
        {
            gameOverDistanceTextUI.text = $"{Mathf.RoundToInt(currentDistance)} m";
        }

        if (bestDistanceGameOverTextUI != null)
        {
            bestDistanceGameOverTextUI.text = $"{Mathf.RoundToInt(bestDistance)} m";
        }

        int currentReviveCost = GetCurrentReviveCost();
        if (reviveCostText != null)
        {
            reviveCostText.text = currentReviveCost.ToString();
        }

        if (heartsCountGameOverText != null && statsService != null)
        {
            heartsCountGameOverText.text = statsService.AvailableHearts.ToString();
        }

        UpdateReviveButtonColor(statsService != null && statsService.AvailableHearts >= currentReviveCost);

        ShowGameOverPanel();
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlayGameOverMusic();

        SaveService.SaveNow();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        SaveService.SaveNow();
        Time.timeScale = 1f;
        MenuManager.ResetGameStart();
        SceneManager.LoadScene("LoadingScene");
    }
    #endregion

    #region Управление паузой
    public void TogglePause()
    {
        if (isGameOver)
        {
            return;
        }

        // Если игра уже остановлена другим потоком (например, стартовым меню), не открываем паузу.
        if (!isPaused && Time.timeScale <= 0f)
        {
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            ShowPauseMenu();
            gameplayUI?.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            HidePauseMenu();
            gameplayUI?.SetActive(true);
            Time.timeScale = 1f;
        }
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

    #region Логика возрождения и контрольных точек
    private void UpdateReviveButtonColor(bool hasEnoughHearts)
    {
        if (reviveButton == null)
        {
            return;
        }

        ColorBlock cb = reviveButton.colors;
        float alpha = hasEnoughHearts ? 1f : 100f / 255f;

        cb.normalColor = new Color(cb.normalColor.r, cb.normalColor.g, cb.normalColor.b, alpha);
        cb.highlightedColor = new Color(cb.highlightedColor.r, cb.highlightedColor.g, cb.highlightedColor.b, alpha);
        cb.pressedColor = new Color(cb.pressedColor.r, cb.pressedColor.g, cb.pressedColor.b, alpha);
        cb.selectedColor = new Color(cb.selectedColor.r, cb.selectedColor.g, cb.selectedColor.b, alpha);
        cb.disabledColor = new Color(cb.disabledColor.r, cb.disabledColor.g, cb.disabledColor.b, alpha);

        reviveButton.colors = cb;
    }

    public void Revive()
    {
        if (statsService == null)
        {
            return;
        }

        if (statsService.TrySpendHeartsForRevive())
        {
            UpdateReviveButtonColor(true);
            HideGameOverPanel();
            gameplayUI?.SetActive(true);
            isGameOver = false;

            if (playerInstance != null)
            {
                playerInstance.enabled = true;
                var rb = playerInstance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = true;
                }
            }

            AudioManager.Instance?.StopGameOverMusic();
            AudioManager.Instance?.PlayMusic();
            RevivePlayer();
            UpdateHeartUI();

            if (heartsCountGameOverText != null)
            {
                heartsCountGameOverText.text = statsService.AvailableHearts.ToString();
            }
            if (reviveCostText != null)
            {
                reviveCostText.text = GetCurrentReviveCost().ToString();
            }

            SaveService.SaveNow();
            return;
        }

        UpdateReviveButtonColor(false);
        ShowAdForHeart();
    }

    private int GetCurrentReviveCost()
    {
        return statsService == null ? 1 : statsService.GetCurrentReviveCost();
    }

    private Vector3 GetRespawnPosition()
    {
        return hasCheckpoint ? lastCheckpointPosition : startPosition;
    }

    private void RevivePlayer()
    {
        if (playerInstance == null)
        {
            playerInstance = FindFirstObjectByType<PersRunner>();
            if (playerInstance == null)
            {
                return;
            }
        }

        playerInstance.transform.position = GetRespawnPosition();
        playerInstance.gameObject.SetActive(true);
        playerInstance.Revive();
    }

    private void ShowAdForHeart()
    {
        Debug.Log("Показ рекламы для получения сердца");
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
        {
            return;
        }

        lastCheckpointPosition = checkpoint.position;
        hasCheckpoint = true;
        Debug.Log("Контрольная точка обновлена: " + lastCheckpointPosition);
    }
    #endregion

    #region Анимация окон
    public void ShowPauseMenu()
    {
        if (pauseMenu == null)
        {
            return;
        }

        pauseMenu.SetActive(true);
        if (pauseAnimator != null)
        {
            pauseAnimator.ResetTrigger("Hide");
            pauseAnimator.SetTrigger("Show");
        }

        FocusFirstSelectable(pauseMenu, pauseFirstSelectable);
    }

    public void HidePauseMenu()
    {
        if (pauseAnimator == null)
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            return;
        }

        pauseAnimator.ResetTrigger("Show");
        pauseAnimator.SetTrigger("Hide");
    }

    public void OnPauseHideAnimationEnd()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        gameOverPanel.SetActive(true);
        if (gameOverAnimator != null)
        {
            gameOverAnimator.ResetTrigger("Hide");
            gameOverAnimator.SetTrigger("Show");
        }

        FocusFirstSelectable(gameOverPanel, gameOverFirstSelectable != null ? gameOverFirstSelectable : reviveButton);
    }

    public void HideGameOverPanel()
    {
        if (gameOverAnimator == null)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            return;
        }

        gameOverAnimator.ResetTrigger("Show");
        gameOverAnimator.SetTrigger("Hide");
    }

    public void OnGameOverHideAnimationEnd()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    #endregion

    #region Дополнительные методы
    private void SaveProgress()
    {
        SaveService.SaveIfDirty(1f);
    }

    public void OnBeforeUnload()
    {
        SaveService.SaveNow();
    }

    private void FocusFirstSelectable(GameObject root, Selectable preferred = null)
    {
        if (EventSystem.current == null || root == null || !root.activeInHierarchy)
        {
            return;
        }

        if (preferred != null && preferred.gameObject.activeInHierarchy && preferred.interactable)
        {
            EventSystem.current.SetSelectedGameObject(preferred.gameObject);
            return;
        }

        var selectables = root.GetComponentsInChildren<Selectable>(true);
        for (int i = 0; i < selectables.Length; i++)
        {
            if (!selectables[i].interactable || !selectables[i].gameObject.activeInHierarchy)
            {
                continue;
            }

            EventSystem.current.SetSelectedGameObject(selectables[i].gameObject);
            return;
        }
    }
    #endregion
}
