using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{ 
    public Image loadingCircleBar; 
    public Text textLoading; 
    public GameObject generalButton;
    
    private Animator loadingCircleAnimator;
    private Animator textAnimator;
    private bool isLoading = false;

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
            textLoading.text = "Press to Play";
        }
        if (generalButton != null)
        {
            generalButton.SetActive(true);
        }
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
        if (textLoading != null)
            textLoading.text = "Loading...";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (textLoading != null)
                textLoading.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
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
    } 
}
