using UnityEngine;

public class PersRunner : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
        collision.GetComponent<DestroyObjectComponent>()?.DestroyObject();
    }
}

}
