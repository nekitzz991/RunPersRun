using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // Расстояние до конца уровня, при котором генерируется новая часть
    [SerializeField] private float playerDistanceSpawnLevelPart = 200f;
    
    // Количество частей уровня, которые создаются в начале игры
    [SerializeField] private int startingSpawnLevelParts = 3;
    
    // Стартовая зона уровня, содержащая точку окончания ("EndPoint")
    [SerializeField] private Transform startZone;
    
    // Список префабов частей уровня
    [SerializeField] private List<Transform> levelPartPrefabs;

    private PersRunner player;
    private Vector3 lastEndPosition;

    private void Awake()
    {
        // Проверка обязательных ссылок
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

        // Поиск игрока
        player = FindObjectOfType<PersRunner>();
        if (player == null)
        {
            Debug.LogError("Компонент PersRunner не найден на сцене!");
            return;
        }

        // Получение конечной точки стартовой зоны
        lastEndPosition = startZone.Find("EndPoint").position;

        // Генерация начальных частей уровня
        for (int i = 0; i < startingSpawnLevelParts; i++)
        {
            SpawnLevelPart();
        }
    }

    private void Start() 
    {
        // Запуск корутины для проверки условий спауна новых частей уровня
        StartCoroutine(CheckSpawnCondition());
    }

    private IEnumerator CheckSpawnCondition()
    {
        // В цикле проверяем расстояние между игроком и концом последней части уровня
        while (true)
        {
            if (Vector3.Distance(player.transform.position, lastEndPosition) < playerDistanceSpawnLevelPart)
            {
                SpawnLevelPart();
            }
            // Проверяем условие 5 раз в секунду
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SpawnLevelPart()
    {
        // Выбор случайного префаба из списка
        int randomIndex = Random.Range(0, levelPartPrefabs.Count);
        Transform chosenLevelPart = levelPartPrefabs[randomIndex];

        // Создание новой части уровня в позиции последней конечной точки
        Transform newLevelPart = Instantiate(chosenLevelPart, lastEndPosition, Quaternion.identity);
        
        // Используем метод Find для поиска конечной точки в новой части уровня (без дополнительных проверок)
        lastEndPosition = newLevelPart.Find("EndPoint").position;
    }
}
