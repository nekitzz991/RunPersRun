using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    private static ProjectilePool instance;

    private readonly Dictionary<GameObject, Queue<Projectile>> pools = new();

    public static ProjectilePool Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindFirstObjectByType<ProjectilePool>();
            if (instance != null)
            {
                return instance;
            }

            var poolObject = new GameObject("ProjectilePool");
            instance = poolObject.AddComponent<ProjectilePool>();
            DontDestroyOnLoad(poolObject);
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Projectile Spawn(GameObject projectilePrefab, Vector3 position, Quaternion rotation)
    {
        if (projectilePrefab == null)
        {
            return null;
        }

        if (!pools.TryGetValue(projectilePrefab, out Queue<Projectile> queue))
        {
            queue = new Queue<Projectile>();
            pools[projectilePrefab] = queue;
        }

        Projectile projectile;
        if (queue.Count > 0)
        {
            projectile = queue.Dequeue();
            if (projectile == null)
            {
                return Spawn(projectilePrefab, position, rotation);
            }

            projectile.transform.SetParent(null);
            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.gameObject.SetActive(true);
        }
        else
        {
            var instanceProjectile = Instantiate(projectilePrefab, position, rotation);
            projectile = instanceProjectile.GetComponent<Projectile>();
            if (projectile == null)
            {
                projectile = instanceProjectile.AddComponent<Projectile>();
            }
        }

        projectile.SetPoolContext(this, projectilePrefab);
        return projectile;
    }

    public void ReturnToPool(Projectile projectile, GameObject projectilePrefab)
    {
        if (projectile == null || projectilePrefab == null)
        {
            return;
        }

        if (!pools.TryGetValue(projectilePrefab, out Queue<Projectile> queue))
        {
            queue = new Queue<Projectile>();
            pools[projectilePrefab] = queue;
        }

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform, false);
        queue.Enqueue(projectile);
    }
}
