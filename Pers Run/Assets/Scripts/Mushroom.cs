using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private float bounceForce = 20f; // Сила отталкивания
    [SerializeField] private Animator animator; // Анимация пружинистого эффекта

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что объект — это игрок
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Применяем силу вверх
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);

                // Запускаем анимацию
                if (animator != null)
                {
                    animator.SetTrigger("Bounce");
                }
            }
        }
    }
}
