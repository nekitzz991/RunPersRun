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
        loadingCircleAnimator = loadingCircleBar.GetComponent<Animator>(); // Аниматор круга загрузки
        textAnimator = textLoading.GetComponent<Animator>(); // Аниматор текста

        loadingCircleBar.gameObject.SetActive(false);
        textLoading.text = "Press to Play";
        generalButton.SetActive(true);
    }

    public void LoadScene(int sceneId)
    {
        if (!isLoading) 
        {
            isLoading = true;
            loadingCircleBar.gameObject.SetActive(true);
            generalButton.gameObject.SetActive(false);

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
        textLoading.text = "Loading...";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            textLoading.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            loadingCircleBar.transform.Rotate(0, 0, -200 * Time.deltaTime);

            if (operation.progress >= 0.9f)
            {
                

                // Отключаем анимацию текста
                if (textAnimator != null)
                {
                    textAnimator.enabled = false;
                    operation.allowSceneActivation = true;
                }

               
            }
            yield return null;
        }
    } 
}
