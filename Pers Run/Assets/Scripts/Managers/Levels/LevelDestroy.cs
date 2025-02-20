using UnityEngine;

public class LevelDestroy : MonoBehaviour
{
    private PersRunner player;
    private LevelPartPool levelPartPool;

    private void Start()
    {
        // Поиск игрока
        player = FindObjectOfType<PersRunner>();
        if (player == null)
        {
            Debug.LogError("Компонент PersRunner не найден на сцене!");
        }

        // Поиск менеджера пула
        levelPartPool = FindObjectOfType<LevelPartPool>();
        if (levelPartPool == null)
        {
            Debug.LogError("LevelPartPool не найден на сцене!");
        }
    }

    private void Update()
    {
        CheckAndReturnToPool();
    }

    // Если объект находится слишком далеко позади игрока – возвращаем его в пул
    private void CheckAndReturnToPool()
    {
        if (transform.position.x < player.transform.position.x - 100)
        {
            LevelPart levelPart = GetComponent<LevelPart>();
            if (levelPart != null && levelPartPool != null)
            {
                levelPartPool.ReturnToPool(transform, levelPart.originalPrefab);
            }
            else
            {
                // Если вдруг нет LevelPart, то уничтожаем (fallback)
                Destroy(gameObject);
            }
        }
    }
}
