using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    public Transform fallCheck; // Граница, ниже которой Перс умирает
    public LayerMask obstacleLayer; // Слой всех препятствий
    public AudioClip deathSound;

    private void Update()
    {
        // Если Перс упал ниже допустимого уровня — конец игры
        if (transform.position.y < fallCheck.position.y)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, на каком слое объект, в который врезался Перс
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Die();
            
        }
    }

    private void Die()
    {
        GameManager.Instance.GameOver();
        AudioManager.Instance.PlaySFXSound(deathSound);
        gameObject.SetActive(false); // Выключаем Перса
    }
}
