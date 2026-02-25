using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
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
    private InputAction jumpAction;

    private void Awake()
    {
        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Keyboard>/upArrow");
        jumpAction.AddBinding("<Keyboard>/w");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
        jumpAction.AddBinding("<Pointer>/press");
        jumpAction.AddBinding("<Touchscreen>/primaryTouch/press");
    }

    private void OnEnable()
    {
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        jumpAction?.Disable();
    }

    private void OnDestroy()
    {
        jumpAction?.Dispose();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (groundCheck == null)
        {
            Debug.LogError("PersRunner: groundCheck не назначен.");
        }

        if (fallCheck == null)
        {
            Debug.LogError("PersRunner: fallCheck не назначен.");
        }
    }

    private void Update()
    {
        if (isDead || rb == null || animator == null)
        {
            return;
        }

        var velocity = rb.linearVelocity;
        velocity.x = speed;
        rb.linearVelocity = velocity;

        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool jumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();
        if (jumpPressed && isGrounded && !IsPointerOverUI())
        {
            velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
            AudioManager.Instance?.PlaySFXSound(jumpSound, 0.2f);
        }

        animator.SetBool("IsRunning", rb.linearVelocity.x > 0f);
        animator.SetBool("IsJumping", !isGrounded);

        if (fallCheck != null && transform.position.y < fallCheck.position.y)
        {
            Die();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        if (Touchscreen.current == null)
        {
            return false;
        }

        var touches = Touchscreen.current.touches;
        for (int i = 0; i < touches.Count; i++)
        {
            if (!touches[i].press.isPressed)
            {
                continue;
            }

            if (EventSystem.current.IsPointerOverGameObject(touches[i].touchId.ReadValue()))
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Die();
        }
    }

    public void ReceiveFatalDamage()
    {
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        GameManager.Instance?.GameOver();
        AudioManager.Instance?.PlaySFXSound(deathSound);
        animator?.SetTrigger("Die");
    }

    public void Revive()
    {
        isDead = false;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed, 0f);
        }
        if (animator == null)
        {
            return;
        }
        animator.ResetTrigger("Die");
        animator.ResetTrigger("Revive");
        animator.SetTrigger("Revive");
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsJumping", false);
    }
}
