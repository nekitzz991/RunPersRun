using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("UI Канвасы")]
    [SerializeField] private GameObject startScreenMenu;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject achievementScreen;

    // Статическая переменная сохраняет состояние между перезагрузками сцены
    private static bool hasGameStarted = false;

    private void Awake()
    {
        // Если какие-либо ссылки не назначены, выводим сообщение об ошибке и отключаем компонент.
        if (startScreenMenu == null || tutorialScreen == null || achievementScreen == null)
        {
            Debug.LogError("MenuManager: Одну или несколько UI-ссылок не назначены в инспекторе. Пожалуйста, назначьте их!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!hasGameStarted)
        {
            ShowStartScreen();
            Time.timeScale = 0f;
        }
        else
        {
            // Если игра уже была запущена, сразу запускаем игровой процесс
            startScreenMenu.SetActive(false);
            tutorialScreen.SetActive(false);
            achievementScreen.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Статический метод сбрасывает флаг запуска игры.
    /// Его можно вызывать из других скриптов (например, при выходе из игры).
    /// </summary>
    public static void ResetGameStart()
    {
        hasGameStarted = false;
    }

    public void OnStartButtonPressed()
    {
        hasGameStarted = true;
        startScreenMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void OnTutorialButtonPressed()
    {
        startScreenMenu.SetActive(false);
        tutorialScreen.SetActive(true);
    }
    
    public void OnAchievementButtonPressed()
    {
        startScreenMenu.SetActive(false);
        achievementScreen.SetActive(true);
    }
    
    public void OnBackFromTutorialPressed()
    {
        tutorialScreen.SetActive(false);
        startScreenMenu.SetActive(true);
    }
    
    public void OnBackFromAchievementPressed()
    {
        achievementScreen.SetActive(false);
        startScreenMenu.SetActive(true);
    }
    
    public void ShowStartScreen()
    {
        startScreenMenu.SetActive(true);
        tutorialScreen.SetActive(false);
        achievementScreen.SetActive(false);
    }
}
