using UnityEngine;

public class LevelDestroy : MonoBehaviour
{
    [SerializeField] private float despawnDistanceBehindPlayer = 100f;

    private PersRunner player;
    private LevelPartPool levelPartPool;
    private bool isInitialized;

    private void Start()
    {
        // Поиск игрока
        player = FindFirstObjectByType<PersRunner>();
        if (player == null)
        {
            Debug.LogError("Компонент PersRunner не найден на сцене!");
            enabled = false;
            return;
        }

        // Поиск менеджера пула
        levelPartPool = FindFirstObjectByType<LevelPartPool>();
        if (levelPartPool == null)
        {
            Debug.LogError("LevelPartPool не найден на сцене!");
            enabled = false;
            return;
        }

        isInitialized = true;
    }

    private void Update()
    {
        CheckAndReturnToPool();
    }

    // Если объект находится слишком далеко позади игрока – возвращаем его в пул
    private void CheckAndReturnToPool()
    {
        if (!isInitialized || player == null || levelPartPool == null)
        {
            return;
        }

        if (transform.position.x < player.transform.position.x - despawnDistanceBehindPlayer)
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
