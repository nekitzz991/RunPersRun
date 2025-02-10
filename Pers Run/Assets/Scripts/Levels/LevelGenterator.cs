using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Параметры генерации")]
    [SerializeField] private float playerDistanceSpawnLevelPart = 200f;
    [SerializeField] private int startingSpawnLevelParts = 3;
    
    [Header("Ссылки на объекты сцены")]
    [SerializeField] private Transform startZone; // В объекте должна быть дочерняя точка "EndPoint"
    [SerializeField] private List<Transform> levelPartPrefabs;

    private PersRunner player;  // Компонент игрока
    private Vector3 lastEndPosition;
    private LevelPartPool levelPartPool;
    
    // Переменная для хранения индекса последнего выбранного префаба
    private int lastPrefabIndex = -1;

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

        player = FindObjectOfType<PersRunner>();
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

        levelPartPool = FindObjectOfType<LevelPartPool>();
        if (levelPartPool == null)
        {
            Debug.LogError("LevelPartPool не найден на сцене!");
            return;
        }

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

    private void SpawnLevelPart()
    {
        int randomIndex = 0;
        // Если в списке больше одного префаба, выбираем случайный индекс, не равный последнему выбранному
        if (levelPartPrefabs.Count > 1)
        {
            do
            {
                randomIndex = Random.Range(0, levelPartPrefabs.Count);
            }
            while (randomIndex == lastPrefabIndex);
        }
        else
        {
            randomIndex = 0;
        }
        lastPrefabIndex = randomIndex;

        Transform chosenLevelPart = levelPartPrefabs[randomIndex];

        // Получаем часть уровня из пула
        Transform newLevelPart = levelPartPool.GetLevelPart(chosenLevelPart, lastEndPosition, Quaternion.identity);

        // Находим дочерний объект "EndPoint" в сгенерированной части
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
