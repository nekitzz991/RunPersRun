using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocalizationSwitcher : MonoBehaviour
{
    public static LocalizationSwitcher Instance;

    [Header("Параметры переключения языка")]
    [Tooltip("Кнопка для переключения языка")]
    public Button languageButton;
    [Tooltip("Иконка флага")]
    public Image flagIcon;
    [Tooltip("Спрайт для английского языка")]
    public Sprite englishFlag;
    [Tooltip("Спрайт для русского языка")]
    public Sprite russianFlag;

    private bool isSwitchingLanguage = false;
    private const string selectedLanguageKey = "SelectedLanguage";

    private void Awake()
    {
        // Реализация синглтона: если объект уже существует, уничтожаем новый
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Если кнопка уже назначена (например, если объект создан в первой сцене с UI),
        // подписываемся на её событие
        if (languageButton != null)
        {
            languageButton.onClick.AddListener(ToggleLanguage);
        }

        // Если сохранён язык, устанавливаем его
        if (PlayerPrefs.HasKey(selectedLanguageKey))
        {
            string savedLanguage = PlayerPrefs.GetString(selectedLanguageKey);
            SetLanguageByCode(savedLanguage);
        }

        UpdateFlagIcon();
    }

    /// <summary>
    /// Позволяет обновить ссылки на UI элементы для переключения языка.
    /// Вызывайте этот метод во второй сцене, чтобы назначить элементы из этой сцены.
    /// </summary>
    public void UpdateUIReferences(Button newLanguageButton, Image newFlagIcon, Sprite newEnglishFlag, Sprite newRussianFlag)
    {
        // Если ранее была назначена кнопка, отписываемся от её события
        if (languageButton != null)
        {
            languageButton.onClick.RemoveListener(ToggleLanguage);
        }

        languageButton = newLanguageButton;
        flagIcon = newFlagIcon;
        englishFlag = newEnglishFlag;
        russianFlag = newRussianFlag;

        if (languageButton != null)
        {
            languageButton.onClick.AddListener(ToggleLanguage);
        }

        UpdateFlagIcon();
    }

    public void ToggleLanguage()
    {
        if (isSwitchingLanguage)
            return;

        StartCoroutine(SwitchLanguageCoroutine());
    }

    private IEnumerator SwitchLanguageCoroutine()
    {
        isSwitchingLanguage = true;
        yield return LocalizationSettings.InitializationOperation;

        int currentLocaleIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        int newLocaleIndex = (currentLocaleIndex + 1) % LocalizationSettings.AvailableLocales.Locales.Count;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[newLocaleIndex];

        // Сохраняем выбранный язык
        PlayerPrefs.SetString(selectedLanguageKey, LocalizationSettings.SelectedLocale.Identifier.Code);
        PlayerPrefs.Save();

        UpdateFlagIcon();
        isSwitchingLanguage = false;
    }

    private void UpdateFlagIcon()
    {
        if (flagIcon == null)
            return;

        string currentLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        Debug.Log("Current Locale Code: " + currentLocaleCode);

        if (currentLocaleCode.StartsWith("en"))
        {
            flagIcon.sprite = englishFlag;
        }
        else if (currentLocaleCode.StartsWith("ru"))
        {
            flagIcon.sprite = russianFlag;
        }
    }

    private void SetLanguageByCode(string languageCode)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }
        UpdateFlagIcon();
    }
}
