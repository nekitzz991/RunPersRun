using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("UI Канвасы")]
    [SerializeField] private GameObject startScreenMenu;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject achievementScreen;

    [Header("Единый шрифт")]
    [Tooltip("Шрифт, который будет применяться ко всем UI Text элементам")]
    [SerializeField] private Font globalFont;

    private static bool hasGameStarted = false;
    private const string selectedFontKey = "SelectedFont";

    private void Awake()
    {
        if (startScreenMenu == null || tutorialScreen == null || achievementScreen == null)
        {
            Debug.LogError("MenuManager: Не все UI-ссылки назначены в инспекторе!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Пытаемся загрузить сохранённый шрифт
        if (PlayerPrefs.HasKey(selectedFontKey))
        {
            string fontName = PlayerPrefs.GetString(selectedFontKey);
            Font loadedFont = Resources.Load<Font>(fontName);
            if (loadedFont != null)
            {
                globalFont = loadedFont;
                Debug.Log("Загружен сохранённый шрифт: " + fontName);
            }
            else
            {
                Debug.LogWarning("Шрифт с именем " + fontName + " не найден в Resources.");
            }
        }
        else
        {
            // Сохраняем шрифт по умолчанию, если сохранения ещё нет
            if (globalFont != null)
            {
                PlayerPrefs.SetString(selectedFontKey, globalFont.name);
                PlayerPrefs.Save();
                Debug.Log("Сохранён шрифт по умолчанию: " + globalFont.name);
            }
        }

        if (!hasGameStarted)
        {
            ShowStartScreen();
            Time.timeScale = 0f;
        }
        else
        {
            startScreenMenu.SetActive(false);
            tutorialScreen.SetActive(false);
            achievementScreen.SetActive(false);
            Time.timeScale = 1f;
        }

        ApplyGlobalFont(); // Применяем единый шрифт ко всем текстовым элементам
    }

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

    /// <summary>
    /// Устанавливает новый глобальный шрифт и сохраняет его выбор.
    /// </summary>
    /// <param name="newFont">Новый шрифт для установки.</param>
    public void SetGlobalFont(Font newFont)
    {
        if (newFont != null)
        {
            globalFont = newFont;
            // Сохраняем выбранный шрифт в PlayerPrefs
            PlayerPrefs.SetString(selectedFontKey, newFont.name);
            PlayerPrefs.Save();
            ApplyGlobalFont();
            Debug.Log("Установлен глобальный шрифт: " + newFont.name);
        }
    }

    /// <summary>
    /// Применяет глобальный шрифт ко всем UI Text элементам в сцене.
    /// </summary>
    private void ApplyGlobalFont()
    {
        if (globalFont == null)
        {
            Debug.LogWarning("Глобальный шрифт не установлен.");
            return;
        }
        Text[] allTexts = FindObjectsOfType<Text>();
        foreach (Text text in allTexts)
        {
            text.font = globalFont;
        }

        Debug.Log("Глобальный шрифт применён ко всем UI Text элементам.");
    }
}
