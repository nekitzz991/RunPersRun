using UnityEngine;
using UnityEngine.UI;

public class LocalizationUIInitializer : MonoBehaviour
{
    [Header("Ссылки на UI элементы второй сцены")]
    public Button languageButton;
    public Image flagIcon;
    public Sprite englishFlag;
    public Sprite russianFlag;

    private void Start()
    {
        if (LocalizationSwitcher.Instance != null)
        {
            LocalizationSwitcher.Instance.UpdateUIReferences(languageButton, flagIcon, englishFlag, russianFlag);
        }
        else
        {
            Debug.LogWarning("LocalizationSwitcher.Instance == null. Убедитесь, что объект с LocalizationSwitcher создан в первой сцене.");
        }
    }
}
