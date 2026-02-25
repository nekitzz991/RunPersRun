using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;

public class LoadingScene : MonoBehaviour
{ 
    public Image loadingCircleBar; 
    public Text textLoading; 
    public GameObject generalButton;
    [Header("Input Settings")]
    [SerializeField] private int sceneIdToLoad = 1;

    [Header("Локализованные строки")]
    [Tooltip("Press to Play")]
    public LocalizedString pressToPlayText;
    [Tooltip("Loading...")]
    public LocalizedString loadingText;
    [Tooltip("Loading {0}%")]
    public LocalizedString loadingProgressText;

    private Animator loadingCircleAnimator;
    private Animator textAnimator;
    private bool isLoading = false;
    private InputAction pressToStartAction;

    private void Awake()
    {
        pressToStartAction = new InputAction("PressToStart", InputActionType.Button);
        pressToStartAction.AddBinding("<Keyboard>/enter");
        pressToStartAction.AddBinding("<Keyboard>/numpadEnter");
        pressToStartAction.AddBinding("<Keyboard>/space");
        pressToStartAction.AddBinding("<Gamepad>/buttonSouth");
        pressToStartAction.AddBinding("<Gamepad>/start");
    }

    private void OnEnable()
    {
        pressToStartAction?.Enable();
    }

    private void OnDisable()
    {
        pressToStartAction?.Disable();
    }

    private void Start()
    {
        if (loadingCircleBar != null)
        {
            loadingCircleAnimator = loadingCircleBar.GetComponent<Animator>(); // Аниматор круга загрузки
            loadingCircleBar.gameObject.SetActive(false);
        }
        if (textLoading != null)
        {
            textAnimator = textLoading.GetComponent<Animator>(); // Аниматор текста

            // Подписываемся на событие обновления локализованной строки для кнопки "Press to Play"
            pressToPlayText.StringChanged += UpdatePressToPlayText;
            // Обновляем строку сразу, чтобы получить актуальное значение
            pressToPlayText.RefreshString();
        }
        if (generalButton != null)
        {
            generalButton.SetActive(true);
            StartCoroutine(FocusGeneralButtonDelayed());
        }
    }

    private void Update()
    {
        if (isLoading || pressToStartAction == null || !pressToStartAction.WasPressedThisFrame())
        {
            return;
        }

        if (generalButton == null || !generalButton.activeInHierarchy)
        {
            return;
        }

        LoadScene(sceneIdToLoad);
    }

    private void UpdatePressToPlayText(string localizedValue)
    {
        if (textLoading != null)
            textLoading.text = localizedValue;
    }

    private void OnDestroy()
    {
        pressToPlayText.StringChanged -= UpdatePressToPlayText;
        loadingText.StringChanged -= UpdateLoadingText;
        pressToStartAction?.Dispose();
        pressToStartAction = null;
    }

    // Этот метод должен быть привязан к кнопке из загрузочной сцены
    public void LoadScene(int sceneId)
    {
        if (!isLoading)
        {
            isLoading = true;
            if (loadingCircleBar != null)
                loadingCircleBar.gameObject.SetActive(true);
            if (generalButton != null)
                generalButton.SetActive(false);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (loadingCircleAnimator != null)
            {
                loadingCircleAnimator.enabled = true; // Включаем анимацию круга
            }
            if (textAnimator != null)
            {
                textAnimator.enabled = true; // Включаем анимацию текста
            }

            StartCoroutine(LoadSceneAsync(sceneId));
        }
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        // Перед началом загрузки обновляем текст на "Loading..." с помощью локализации
        if (textLoading != null)
        {
            // Можно сразу получить значение без подписки, если не требуется динамическое обновление
            loadingText.StringChanged += UpdateLoadingText;
            loadingText.RefreshString();
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (textLoading != null)
            {
                // Если нужно выводить процент загрузки в виде "Loading {0}%", передаём параметр в локализованную строку:
                loadingProgressText.Arguments = new object[] { Mathf.RoundToInt(progress * 100) };
                textLoading.text = loadingProgressText.GetLocalizedString();
            }
            if (loadingCircleBar != null)
                loadingCircleBar.transform.Rotate(0, 0, -200 * Time.deltaTime);

            if (operation.progress >= 0.9f)
            {
                if (textAnimator != null)
                    textAnimator.enabled = false;
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        loadingText.StringChanged -= UpdateLoadingText;
    }

    private void UpdateLoadingText(string localizedValue)
    {
        if (textLoading != null)
            textLoading.text = localizedValue;
    }

    private void FocusGeneralButton()
    {
        if (EventSystem.current == null || generalButton == null || !generalButton.activeInHierarchy)
        {
            return;
        }

        var selectable = generalButton.GetComponent<Selectable>();
        if (selectable != null && selectable.interactable)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            return;
        }

        var button = generalButton.GetComponentInChildren<Button>(true);
        if (button != null && button.interactable)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private IEnumerator FocusGeneralButtonDelayed()
    {
        yield return null;
        FocusGeneralButton();
    }
}
