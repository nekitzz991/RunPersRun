using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool rotateProjectile = false;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float lifetime = 5f;

    private float lifeTimer;
    private ProjectilePool ownerPool;
    private GameObject sourcePrefab;

    private void OnEnable()
    {
        lifeTimer = lifetime;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Despawn();
            return;
        }

        transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        if (rotateProjectile)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetPoolContext(ProjectilePool pool, GameObject prefab)
    {
        ownerPool = pool;
        sourcePrefab = prefab;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PersRunner>()?.ReceiveFatalDamage();
            Despawn();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PersRunner>()?.ReceiveFatalDamage();
            Despawn();
            return;
        }

        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (ownerPool != null && sourcePrefab != null)
        {
            ownerPool.ReturnToPool(this, sourcePrefab);
            return;
        }

        Destroy(gameObject);
    }
}
