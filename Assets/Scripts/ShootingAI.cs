using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingShooterAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        KeepDistance,
        Aim,
        Cooldown
    }

    [Header("Data Asset")]
    [SerializeField] private EnemyStats stats;

    [Header("Movement & Flight")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float hoverHeight = 2f;
    private int currentWaypointIndex = 0;

    [Header("Combat Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float aimTime = 0.5f;
    [SerializeField] private float cooldownTime = 1.5f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private State currentState;
    private float stateTimer;
    private float unseenTimer;
    private float facingDirection = 1f;
    private Vector3 initialScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        initialScale = transform.localScale;

        if (anim == null)
            anim = GetComponent<Animator>();

        SetState(State.Patrol);
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                if (CanSeePlayer())
                    SetState(State.KeepDistance);
                break;

            case State.KeepDistance:
                KeepDistanceLogic();
                break;

            case State.Aim:
                AimLogic();
                break;

            case State.Cooldown:
                CooldownLogic();
                break;
        }
    }

    void PatrolLogic()
    {
        if (waypoints == null || waypoints.Length == 0 || stats == null)
            return;

        Vector2 targetPos = waypoints[currentWaypointIndex].position;
        FlipSprite(targetPos.x - transform.position.x);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, stats.moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void KeepDistanceLogic()
    {
        bool sees = CanSeePlayer();
        float dist = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
        float shootRange = stats != null ? stats.attackRange * 3f : 6f;

        if (player != null && stats != null)
        {
            FlipSprite(player.position.x - transform.position.x);

            if (sees)
            {
                unseenTimer = 0f;
                Vector2 targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, stats.moveSpeed * 1.2f * Time.deltaTime);
            }
            else
            {
                unseenTimer += Time.deltaTime;
            }
        }

        if (unseenTimer > 1.5f)
        {
            SetState(State.Patrol);
        }
        else if (sees && dist <= shootRange && stateTimer >= 0.3f)
        {
            SetState(State.Aim);
        }
    }

    void AimLogic()
    {
        if (player != null)
        {
            FlipSprite(player.position.x - transform.position.x);
        }

        if (stateTimer >= aimTime)
        {
            Shoot();
            SetState(State.Cooldown);
        }
    }

    void CooldownLogic()
    {
        if (player != null && stats != null)
        {
            FlipSprite(player.position.x - transform.position.x);
            Vector2 targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, (stats.moveSpeed * 0.8f) * Time.deltaTime);
        }

        if (stateTimer >= cooldownTime)
        {
            SetState(State.KeepDistance);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        if (anim != null)
            anim.SetTrigger("Shoot");

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            Vector2 shootDir = (player.position - firePoint.position).normalized;
            float bulletSpeed = 6f;
            bulletRb.linearVelocity = shootDir * bulletSpeed;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || stats == null) return false;

        Vector2 eye = transform.position;
        Vector2 targetEye = player.position;
        Vector2 toPlayer = targetEye - eye;

        if (toPlayer.magnitude > stats.visionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
        return hit.collider == null;
    }

    void FlipSprite(float directionX)
    {
        if (directionX > 0.05f)
        {
            facingDirection = 1f;
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }
        else if (directionX < -0.05f)
        {
            facingDirection = -1f;
            transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }
    }

    void SetState(State nextState)
{
    currentState = nextState;
    stateTimer = 0f;

    if (nextState == State.Aim && AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayEnemyWindup();
    }
}

    void OnDrawGizmosSelected()
    {
        if (stats != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.visionRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange * 3f);
        }
    }
}