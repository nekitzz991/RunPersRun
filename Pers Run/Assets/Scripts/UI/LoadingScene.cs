using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{ 
    public Image loadingCircleBar; 
    public Text textLoading; 
    public GameObject generalButton; 

    private bool isLoading = false;

    private void Start()
    {
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
            StartCoroutine(LoadSceneAsync(sceneId));
        }
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        textLoading.text = "Loading...";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false; // Ждём загрузки

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            textLoading.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            loadingCircleBar.transform.Rotate(0, 0, -200 * Time.deltaTime); // Вращение индикатора

            if (operation.progress >= 0.9f) 
            {
                textLoading.text = "Tap to Continue";
                if (Input.GetMouseButtonDown(0)) // Ожидание клика для завершения
                {
                    operation.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    } 
}
