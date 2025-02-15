using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public bool rotateProjectile = false;
    public float rotationSpeed = 360f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        if (rotateProjectile)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}
