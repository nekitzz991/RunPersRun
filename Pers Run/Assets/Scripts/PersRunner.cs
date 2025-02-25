using UnityEngine;
using UnityEngine.EventSystems;

public class PersRunner : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip deathSound;

    [Header("Death Settings")]
    public Transform fallCheck;      
    public LayerMask obstacleLayer;  

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
{
    if (isDead) return;

    rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && !EventSystem.current.IsPointerOverGameObject())
{
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    AudioManager.Instance.PlaySFXSound(jumpSound, 0.2f);
}


    animator.SetBool("IsRunning", rb.linearVelocity.x > 0);
    animator.SetBool("IsJumping", !isGrounded);

    if (transform.position.y < fallCheck.position.y)
    {
        Die();
    }
}

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ItemComponent item = collision.GetComponent<ItemComponent>();
        if (item != null)
        {
            GameManager.Instance.AddScore(item.ScoreValue);
        }
    }

    private void Die()
    {
        if (isDead) return; 

        isDead = true;
        GameManager.Instance.GameOver();
        AudioManager.Instance.PlaySFXSound(deathSound);

        animator.SetTrigger("Die"); 

    }
public void Revive()
{
    isDead = false;
    animator.ResetTrigger("Die");
    animator.ResetTrigger("Revive");
    animator.SetTrigger("Revive");
    animator.SetBool("IsRunning", true);
    animator.SetBool("IsJumping", false);
}





    
}
