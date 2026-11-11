using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Movement & Patrol")]
    [SerializeField] private float patrolDistance = 3f; // На какое расстояние вправо и влево ходить от точки старта
    [SerializeField] private float patrolSpeed = 1.7f;
    [SerializeField] private float chaseSpeed = 3f;

    private Vector2 startPosition;
    private int patrolDirection = 1; // 1 — вправо, -1 — влево

    [Header("Vision & Combat")]
    [SerializeField] private Transform player;
    [SerializeField] private float viewRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackHeight = 1f;
    [SerializeField] private float windUpTime = 0.5f;
    [SerializeField] private float recoveryTime = 0.8f;
    [SerializeField] private int damageAmount = 15;
    [SerializeField] private float loseTime = 2f;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;

    private State currentState;

    private float stateTimer;
    private float facingDirection = 1f;

    private bool hasStruck;

    private Vector3 initialScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        initialScale = transform.localScale;

        startPosition = transform.position; // Запоминаем стартовую позицию слайма

        if (anim == null)
            anim = GetComponent<Animator>();

        if (player != null)
            playerCollider = player.GetComponent<Collider2D>();

        SetState(State.Patrol);
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        if (player != null && playerCollider == null)
            playerCollider = player.GetComponent<Collider2D>();

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();

                if (CanSeePlayer())
                    SetState(State.Chase);
                break;

            case State.Chase:
                ChaseLogic();
                break;

            case State.Attack:
                AttackLogic();
                break;
        }
    }

    void PatrolLogic()
    {
        // Двигаемся влево и вправо от стартовой позиции на расстояние patrolDistance
        float targetX = startPosition.x + (patrolDistance * patrolDirection);
        Vector2 targetPos = new Vector2(targetX, transform.position.y);

        FlipSprite(targetPos.x - transform.position.x);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPos,
            patrolSpeed * Time.deltaTime
        );

        // Если дошли до точки — разворачиваемся в другую сторону
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            patrolDirection *= -1;
        }
    }

    void ChaseLogic()
    {
        if (player == null)
            return;

        bool sees = CanSeePlayer();

        if (sees)
            stateTimer = 0f;

        float dx = player.position.x - transform.position.x;

        FlipSprite(dx);

        if (CanAttackPlayer())
        {
            SetState(State.Attack);
            return;
        }

        if (Mathf.Abs(dx) > attackRange * 0.8f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(player.position.x, transform.position.y),
                chaseSpeed * Time.deltaTime
            );
        }

        if (stateTimer > loseTime && !sees)
        {
            SetState(State.Patrol);
        }
    }

    void AttackLogic()
    {
        if (player == null)
        {
            SetState(State.Patrol);
            return;
        }

        float dx = player.position.x - transform.position.x;

        FlipSprite(dx);

        if (stateTimer >= windUpTime && !hasStruck)
        {
            hasStruck = true;

            if (anim != null)
                anim.SetTrigger("Attack");

            if (CanAttackPlayer())
            {
                PlayerHealth hp = player.GetComponent<PlayerHealth>();

                if (hp == null)
                    hp = player.GetComponentInParent<PlayerHealth>();

                if (hp != null)
                {
                    hp.TakeDamage(damageAmount);
                }
            }
        }

        if (stateTimer >= windUpTime + recoveryTime)
        {
            SetState(State.Chase);
        }
    }

    bool CanAttackPlayer()
    {
        if (player == null)
            return false;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        if (dx > attackRange || dy > attackHeight)
            return false;

        if (enemyCollider != null && playerCollider != null)
        {
            bool playerAbove =
                playerCollider.bounds.min.y >=
                enemyCollider.bounds.max.y - 0.05f;

            if (playerAbove)
                return false;
        }
        else if (player.position.y > transform.position.y + attackHeight)
        {
            return false;
        }

        return true;
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 eye = (Vector2)transform.position + Vector2.up * 0.5f;
        Vector2 targetEye = (Vector2)player.position + Vector2.up * 0.5f;
        Vector2 toPlayer = targetEye - eye;

        if (toPlayer.magnitude > viewRange)
            return false;

        Vector2 facingVector = new Vector2(facingDirection, 0f);

        if (Vector2.Angle(facingVector, toPlayer) > fov / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            eye,
            toPlayer.normalized,
            toPlayer.magnitude,
            obstacleMask
        );

        return hit.collider == null;
    }

    void FlipSprite(float directionX)
    {
        if (directionX > 0.05f)
        {
            facingDirection = 1f;

            transform.localScale = new Vector3(
                Mathf.Abs(initialScale.x),
                initialScale.y,
                initialScale.z
            );
        }
        else if (directionX < -0.05f)
        {
            facingDirection = -1f;

            transform.localScale = new Vector3(
                -Mathf.Abs(initialScale.x),
                initialScale.y,
                initialScale.z
            );
        }
    }

    void SetState(State nextState)
    {
        currentState = nextState;
        stateTimer = 0f;
        hasStruck = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        Gizmos.color = Color.red;

        Vector3 left = Quaternion.Euler(0, 0, fov / 2f) *
                       Vector3.right * facingDirection;

        Vector3 right = Quaternion.Euler(0, 0, -fov / 2f) *
                        Vector3.right * facingDirection;

        Gizmos.DrawRay(transform.position, left * viewRange);
        Gizmos.DrawRay(transform.position, right * viewRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}