using UnityEngine;

public class PersRunner : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public AudioClip jumpSound; // Звук прыжка
    public AudioClip deathSound; // Звук смерти
    public AudioSource audioSource; // AudioSource, который можно назначить в инспекторе

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Если audioSource не назначен в инспекторе, попробуем получить его автоматически
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource не найден на объекте. Добавьте компонент AudioSource вручную.");
            }
        }
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlayJumpSound(); // Воспроизводим звук прыжка
        }

        animator.SetBool("IsRunning", rb.linearVelocity.x > 0);
        animator.SetBool("IsJumping", !isGrounded);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ItemComponent item = collision.GetComponent<ItemComponent>();
        
        if (item != null)
        {
            GameManager.Instance.AddScore(item.ScoreValue);
        }
    }

    // Метод для воспроизведения звука прыжка
    private void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
        else
        {
            Debug.LogWarning("jumpSound или audioSource не назначены.");
        }
    }

    // Метод для воспроизведения звука смерти (например, при столкновении с врагом)
    public void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        else
        {
            Debug.LogWarning("deathSound или audioSource не назначены.");
        }
    }
}