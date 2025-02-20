using UnityEngine;

public class CactusShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab; // Префаб снаряда
    public Transform shootPoint; // Точка спавна снаряда
    public float shootInterval = 2f; // Интервал между выстрелами
    public float minDistanceToPlayer = 3f; // Минимальное расстояние до игрока, при котором кактус перестает стрелять
    public Transform player; // Ссылка на объект игрока

    private Animator animator;
    private float shootTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
        shootTimer = shootInterval; // Начинаем с задержки
    }

    void Update()
    {
        // Если ссылка на игрока задана и игрок слишком близко, то стрельба отключается
        if (player != null && Vector3.Distance(transform.position, player.position) < minDistanceToPlayer)
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

    void Shoot()
    {
        animator.SetTrigger("Shoot"); // Запускаем анимацию стрельбы
    }

    // Метод вызывается анимационным эвентом в момент выстрела
    public void SpawnProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        }
    }
}
