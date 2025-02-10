using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("UI Канвасы")]
    [SerializeField] private GameObject startScreenMenu;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject achievementScreen;

    // Статическая переменная сохраняет состояние между перезагрузками сцены
    private static bool hasGameStarted = false;

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
