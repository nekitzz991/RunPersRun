using System.Collections.Generic;
using UnityEngine;

// Интерфейс для объектов из пула (по желанию)
public interface IPooledObject
{
    void OnObjectSpawn();
}

[System.Serializable]
public class Pool
{
    public string tag;          // Идентификатор пула
    public GameObject prefab;   // Префаб, который будет клонироваться
    public int size;            // Начальное количество объектов в пуле
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Настройки пулов объектов")]
    public List<Pool> pools;
    // Словарь для быстрого доступа к очередям по тегу
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // Реализация Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Инициализация пула для каждого типа объекта
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);  // Деактивируем объект до использования
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Метод для получения объекта из пула.
    /// </summary>
    /// <param name="tag">Идентификатор пула</param>
    /// <param name="position">Позиция для размещения объекта</param>
    /// <param name="rotation">Вращение для размещения объекта</param>
    /// <returns>Активированный объект из пула</returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Пул с тегом " + tag + " не найден.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Активируем и устанавливаем позицию и вращение
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Если у объекта есть компонент IPooledObject, вызываем метод OnObjectSpawn
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        // После использования объект возвращаем в конец очереди
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
