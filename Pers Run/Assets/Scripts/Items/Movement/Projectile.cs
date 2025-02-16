using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public bool rotateProjectile = false;
    public float rotationSpeed = 360f;
    public float lifetime = 5f; // время жизни снаряда в секундах

    void Start()
    {
        Destroy(gameObject, lifetime); // уничтожаем снаряд через заданное время
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        if (rotateProjectile)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}
