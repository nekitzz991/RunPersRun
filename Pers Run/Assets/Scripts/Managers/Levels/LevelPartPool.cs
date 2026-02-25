using System.Collections.Generic;
using UnityEngine;

public class LevelPartPool : MonoBehaviour
{
    [Header("Префабы частей уровня для пула")]
    [SerializeField] private List<Transform> levelPartPrefabs;
    [SerializeField] private Transform poolRoot;
    
    // Словарь: для каждого префаба своя очередь объектов
    private Dictionary<Transform, Queue<Transform>> poolDictionary;

    private void Awake()
    {
        poolDictionary = new Dictionary<Transform, Queue<Transform>>();
        foreach (Transform prefab in levelPartPrefabs)
        {
            if (prefab != null)
            {
                poolDictionary[prefab] = new Queue<Transform>();
            }
        }

        if (poolRoot == null)
        {
            poolRoot = transform;
        }
    }

    /// <summary>
    /// Возвращает объект уровня из пула или создаёт новый, если пул пуст.
    /// </summary>
    public Transform GetLevelPart(Transform prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!poolDictionary.TryGetValue(prefab, out Queue<Transform> queue))
        {
            queue = new Queue<Transform>();
            poolDictionary[prefab] = queue;
        }

        if (queue.Count > 0)
        {
            Transform obj = queue.Dequeue();
            obj.SetParent(null);
            obj.position = position;
            obj.rotation = rotation;
            obj.gameObject.SetActive(true);
            return obj;
        }

        // Если в пуле нет доступных объектов, создаём новый.
        Transform created = Instantiate(prefab, position, rotation);
        LevelPart levelPart = created.GetComponent<LevelPart>();
        if (levelPart == null)
        {
            levelPart = created.gameObject.AddComponent<LevelPart>();
        }
        levelPart.originalPrefab = prefab;
        return created;
    }

    /// <summary>
    /// Возвращает объект в пул (деактивирует его и помещает в очередь).
    /// </summary>
    public void ReturnToPool(Transform obj, Transform prefab)
    {
        if (obj == null || prefab == null)
        {
            return;
        }

        if (!poolDictionary.TryGetValue(prefab, out Queue<Transform> queue))
        {
            queue = new Queue<Transform>();
            poolDictionary[prefab] = queue;
        }

        obj.SetParent(poolRoot);
        obj.gameObject.SetActive(false);
        queue.Enqueue(obj);
    }
}
