using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TrainingWindow : MonoBehaviour
{
    [SerializeField] private GameObject trainingPanel; // Панель обучения
    [SerializeField] private Button backButton; // Кнопка "Назад"

    private void Start()
    {
        backButton.onClick.AddListener(CloseTraining);
        ShowTraining();
    }

    public void ShowTraining()
    {
        trainingPanel.SetActive(true);
    }

    public void CloseTraining()
    {
        trainingPanel.SetActive(false);
        SceneManager.LoadScene("MainMenu"); // Возвращаемся в главное меню
    }
}
