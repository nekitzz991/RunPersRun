using UnityEngine;

public class CactusShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float minDistanceToPlayer = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private bool autoFindPlayer = true;

    private Animator animator;
    private float shootTimer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ResolvePlayerReference();
        shootTimer = shootInterval;
    }

    private void OnEnable()
    {
        shootTimer = shootInterval;
        ResolvePlayerReference();
    }

    private void Update()
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            return;
        }

        ResolvePlayerReference();
        if (player != null && Mathf.Abs(transform.position.x - player.position.x) < minDistanceToPlayer)
        {
            return;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval; // Сброс таймера
        }
    }

    private void ResolvePlayerReference()
    {
        if (!autoFindPlayer || player != null)
        {
            return;
        }

        var runner = FindFirstObjectByType<PersRunner>();
        if (runner != null)
        {
            player = runner.transform;
        }
    }

    private void Shoot()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
            return;
        }

        SpawnProjectile();
    }

    // Метод вызывается анимационным эвентом в момент выстрела.
    public void SpawnProjectile()
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            return;
        }

        ProjectilePool.Instance.Spawn(projectilePrefab, shootPoint.position, Quaternion.identity);
    }
}
