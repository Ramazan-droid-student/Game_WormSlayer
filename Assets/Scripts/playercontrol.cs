using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Animator trigger names (как в Animator → Parameters)")]
    [SerializeField] private string jumpTrigger = "jump";
    [SerializeField] private string attackTrigger = "attack";
    [SerializeField] private string dieTrigger = "die";

    private Rigidbody2D rb;

    private float horizontalInput;
    private bool isGrounded;
    private bool isDead;
    private float nextAttackTime;

    public bool IsDead => isDead;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (!isDead)
        {
            if (horizontalInput > 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else if (horizontalInput < 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        float x = isDead ? 0f : horizontalInput * moveSpeed;
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    // ---------- Input System ----------
    public void OnMove(InputValue value)
    {
        if (isDead) { horizontalInput = 0f; return; }
        Vector2 moveVal = value.Get<Vector2>();
        horizontalInput = moveVal.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) Jump();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) Attack();
    }

    // ---------- Actions (вызываются и с мобильных кнопок) ----------
    public void Jump()
    {
        if (isDead || !isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (animator != null) animator.SetTrigger(jumpTrigger);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
    }

    public void Attack()
    {
        if (isDead || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null) animator.SetTrigger(attackTrigger);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayAttack();
    }

    // Вызывается из PlayerHealth
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        horizontalInput = 0f;

        if (animator != null) animator.SetTrigger(dieTrigger);
    }

    public void SetMobileHorizontalInput(float input)
    {
        horizontalInput = isDead ? 0f : input;
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, Mathf.Abs(horizontalInput));
            animator.SetBool(IsGroundedHash, isGrounded);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}