using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Components")]
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;

    private float horizontalInput;
    private bool isGrounded;

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

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void OnMove(InputValue value)
    {
        Vector2 moveVal = value.Get<Vector2>();
        horizontalInput = moveVal.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            Jump();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Attack();
        }
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator != null) animator.SetTrigger("jump");
        }
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
    }

    public void SetMobileHorizontalInput(float input)
    {
        horizontalInput = input;
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