using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Канвасы")]
    [SerializeField] private GameObject startScreenMenu;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject CreditsScreen;

    [Header("Единый шрифт")]
    [Tooltip("Шрифт, который будет применяться ко всем UI Text элементам")]
    [SerializeField] private Font globalFont;

    private static bool hasGameStarted = false;
    private const string selectedFontKey = "SelectedFont";

    private void Awake()
    {
        if (startScreenMenu == null || tutorialScreen == null )
        {
            Debug.LogError("MenuManager: Не все UI-ссылки назначены в инспекторе!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (SaveService.HasKey(selectedFontKey))
        {
            string fontName = SaveService.GetString(selectedFontKey);
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
            if (globalFont != null)
            {
                SaveService.SetString(selectedFontKey, globalFont.name, true);
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
            Time.timeScale = 1f;
        }

        ApplyGlobalFont(); 
    }

    public static void ResetGameStart()
    {
        hasGameStarted = false;
    }

    public void OnStartButtonPressed()
    {
        hasGameStarted = true;
        startScreenMenu.SetActive(false);
        FocusFirstSelectable(null);
        Time.timeScale = 1f;
    }
    
    public void OnTutorialButtonPressed()
    {
        startScreenMenu.SetActive(false);
        tutorialScreen.SetActive(true);
        FocusFirstSelectable(tutorialScreen);
    }
    
    public void OnCreditsButtonPressed()
    {
        startScreenMenu.SetActive(false);
        CreditsScreen.SetActive(true);
        FocusFirstSelectable(CreditsScreen);
    }
    
    public void OnBackFromTutorialPressed()
    {
        tutorialScreen.SetActive(false);
        startScreenMenu.SetActive(true);
        FocusFirstSelectable(startScreenMenu);
    }
    public void OnBackFromCreditsPressed()
    {
        CreditsScreen.SetActive(false);
        startScreenMenu.SetActive(true);
        FocusFirstSelectable(startScreenMenu);
    }
    
    public void ShowStartScreen()
    {
        startScreenMenu.SetActive(true);
        tutorialScreen.SetActive(false);
        CreditsScreen?.SetActive(false);
        FocusFirstSelectable(startScreenMenu);
    }

    public void SetGlobalFont(Font newFont)
    {
        if (newFont != null)
        {
            globalFont = newFont;
            SaveService.SetString(selectedFontKey, newFont.name, true);
            ApplyGlobalFont();
            Debug.Log("Установлен глобальный шрифт: " + newFont.name);
        }
    }

    private void ApplyGlobalFont()
    {
        if (globalFont == null)
        {
            Debug.LogWarning("Глобальный шрифт не установлен.");
            return;
        }
        Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (Text text in allTexts)
        {
            text.font = globalFont;
        }

        Debug.Log("Глобальный шрифт применён ко всем UI Text элементам.");
    }

    private void FocusFirstSelectable(GameObject root)
    {
        if (EventSystem.current == null)
        {
            return;
        }

        if (root == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
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

        EventSystem.current.SetSelectedGameObject(null);
    }
}
