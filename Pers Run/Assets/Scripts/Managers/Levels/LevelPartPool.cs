using System.Collections.Generic;
using UnityEngine;

public class LevelPartPool : MonoBehaviour
{
    [Header("Префабы частей уровня для пула")]
    [SerializeField] private List<Transform> levelPartPrefabs;
    
    // Словарь: для каждого префаба своя очередь объектов
    private Dictionary<Transform, Queue<Transform>> poolDictionary;

    private void Awake()
    {
        poolDictionary = new Dictionary<Transform, Queue<Transform>>();
        foreach (Transform prefab in levelPartPrefabs)
        {
            poolDictionary[prefab] = new Queue<Transform>();
        }
    }

    /// <summary>
    /// Возвращает объект уровня из пула или создаёт новый, если пул пуст.
    /// </summary>
    public Transform GetLevelPart(Transform prefab, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            Transform obj = poolDictionary[prefab].Dequeue();
            obj.position = position;
            obj.rotation = rotation;
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            // Если в пуле нет доступных объектов, создаём новый
            Transform obj = Instantiate(prefab, position, rotation);
            // Если на объекте ещё нет компонента LevelPart – добавляем его
            LevelPart levelPart = obj.GetComponent<LevelPart>();
            if (levelPart == null)
            {
                levelPart = obj.gameObject.AddComponent<LevelPart>();
            }
            levelPart.originalPrefab = prefab;
            return obj;
        }
    }

    /// <summary>
    /// Возвращает объект в пул (деактивирует его и помещает в очередь).
    /// </summary>
    public void ReturnToPool(Transform obj, Transform prefab)
    {
        obj.gameObject.SetActive(false);
        if (poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Попытка вернуть объект в несуществующий пул!");
        }
    }
}
