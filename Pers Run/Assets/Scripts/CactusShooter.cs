using UnityEngine;

public class CactusShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab; // Префаб снаряда
    public Transform shootPoint; // Точка спавна снаряда
    public float shootInterval = 2f; // Интервал между выстрелами

    private Animator animator;
    private float shootTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
        shootTimer = shootInterval; // Начать с задержки
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval; // Сброс таймера
        }
    }

    void Shoot()
    {
        animator.SetTrigger("Shoot"); // Запускаем анимацию
    }

    // Этот метод вызывается анимационным эвентом в момент выстрела
    public void SpawnProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        }
    }
}
