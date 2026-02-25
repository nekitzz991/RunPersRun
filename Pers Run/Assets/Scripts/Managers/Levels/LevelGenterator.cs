using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Параметры генерации")]
    [SerializeField] private float playerDistanceSpawnLevelPart = 200f;
    [SerializeField] private int startingSpawnLevelParts = 3;
    
    [Header("Ссылки на объекты сцены")]
    [SerializeField] private Transform startZone; // Объект должен иметь дочернюю точку "EndPoint"
    [SerializeField] private List<Transform> levelPartPrefabs;

    private PersRunner player;  // Компонент игрока
    private Vector3 lastEndPosition;
    private LevelPartPool levelPartPool;
    
    // Переменные для перемешанного списка
    private List<Transform> shuffledPrefabs;
    private int currentIndex = 0;

    private void Awake()
    {
        if (startZone == null)
        {
            Debug.LogError("StartZone не задана в инспекторе!");
            return;
        }

        if (levelPartPrefabs == null || levelPartPrefabs.Count == 0)
        {
            Debug.LogError("Список префабов частей уровня пуст!");
            return;
        }

        player = FindFirstObjectByType<PersRunner>();
        if (player == null)
        {
            Debug.LogError("Компонент PersRunner не найден на сцене!");
            return;
        }

        Transform endPoint = startZone.Find("EndPoint");
        if (endPoint == null)
        {
            Debug.LogError("В StartZone не найден объект с именем 'EndPoint'!");
            return;
        }
        lastEndPosition = endPoint.position;

        levelPartPool = FindFirstObjectByType<LevelPartPool>();
        if (levelPartPool == null)
        {
            Debug.LogError("LevelPartPool не найден на сцене!");
            return;
        }
        
        // Инициализация перемешанного списка до спавна стартовых частей
        ShufflePrefabs();

        // Генерация стартовых частей уровня
        for (int i = 0; i < startingSpawnLevelParts; i++)
        {
            SpawnLevelPart();
        }
    }

    private void Start()
    {
        StartCoroutine(CheckSpawnCondition());
    }

    private IEnumerator CheckSpawnCondition()
    {
        while (true)
        {
            if (Vector3.Distance(player.transform.position, lastEndPosition) < playerDistanceSpawnLevelPart)
            {
                SpawnLevelPart();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Метод перемешивания списка префабов (алгоритм Фишера-Йетса)
    private void ShufflePrefabs()
    {
        shuffledPrefabs = new List<Transform>(levelPartPrefabs);
        for (int i = 0; i < shuffledPrefabs.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledPrefabs.Count);
            Transform temp = shuffledPrefabs[i];
            shuffledPrefabs[i] = shuffledPrefabs[randomIndex];
            shuffledPrefabs[randomIndex] = temp;
        }
        currentIndex = 0;
    }

    private void SpawnLevelPart()
    {
        // Если все зоны использованы, перемешиваем список заново
        if (currentIndex >= shuffledPrefabs.Count)
        {
            ShufflePrefabs();
        }

        Transform chosenLevelPart = shuffledPrefabs[currentIndex];
        currentIndex++;

        // Получаем часть уровня из пула
        Transform newLevelPart = levelPartPool.GetLevelPart(chosenLevelPart, lastEndPosition, Quaternion.identity);

        // Находим дочерний объект "EndPoint" для обновления позиции конца уровня
        Transform newEndPoint = newLevelPart.Find("EndPoint");
        if (newEndPoint != null)
        {
            lastEndPosition = newEndPoint.position;
        }
        else
        {
            Debug.LogWarning("В сгенерированной части уровня не найден 'EndPoint'!");
        }
    }
}
