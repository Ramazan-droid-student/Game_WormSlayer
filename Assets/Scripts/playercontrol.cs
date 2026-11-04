using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Components")]
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;

    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    // Animation Triggers / Hash IDs
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        
        if (Input.GetKeyDown(KeyCode.G))
        {
            Attack();
        }

        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator != null) animator.SetTrigger(JumpTriggerHash);
        }
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTriggerHash);
        }
    }

    public void SetMobileHorizontalInput(float input)
    {
        horizontalInput = input;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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