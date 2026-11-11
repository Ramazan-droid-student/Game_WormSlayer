using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChargerAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        WindUp,
        Dash,
        Stunned
    }

    [Header("Movement & Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float maxDashDistance = 6f;

    private int currentWaypointIndex = 0;
    private int patrolDirection = 1;

    [Header("Vision & Combat")]
    [SerializeField] private Transform player;
    [SerializeField] private float viewRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float windUpTime = 0.7f;
    [SerializeField] private float stunTime = 1.0f;
    [SerializeField] private int damageAmount = 20;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private State currentState;
    private float stateTimer;
    private float facingDirection = 1f;
    private Vector3 initialScale;
    
    private Vector2 dashDirection;
    private float distanceTraveled;

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
                {
                    SetState(State.WindUp);
                }
                break;

            case State.WindUp:
                WindUpLogic();
                break;

            case State.Dash:
                DashLogic();
                break;

            case State.Stunned:
                StunnedLogic();
                break;
        }
    }

    void PatrolLogic()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        if (waypoints[currentWaypointIndex] == null)
            return;

        Vector2 targetPos = waypoints[currentWaypointIndex].position;
        FlipSprite(targetPos.x - transform.position.x);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPos,
            patrolSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            currentWaypointIndex += patrolDirection;

            if (currentWaypointIndex >= waypoints.Length || currentWaypointIndex < 0)
            {
                patrolDirection *= -1;
                currentWaypointIndex += patrolDirection * 2;
                currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
            }
        }
    }

    void WindUpLogic()
    {
        if (player != null)
        {
            float dx = player.position.x - transform.position.x;
            FlipSprite(dx);
        }

        if (stateTimer >= windUpTime)
        {
            if (player != null)
            {
                dashDirection = (player.position - transform.position).normalized;
            }
            else
            {
                dashDirection = new Vector2(facingDirection, 0f);
            }

            distanceTraveled = 0f;
            SetState(State.Dash); 
        }
    }

    void DashLogic()
    {
        float step = dashSpeed * Time.deltaTime;
        transform.position += (Vector3)(dashDirection * step);
        distanceTraveled += step;
        if (player != null && Vector2.Distance(transform.position, player.position) < 0.8f)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp == null) hp = player.GetComponentInParent<PlayerHealth>();

            if (hp != null)
            {
                hp.TakeDamage(damageAmount);
            }

            SetState(State.Stunned);
            return;
        }

        // Проверяем столкновение со стеной или максимальную дистанцию
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, 0.5f, obstacleMask);
        if (hit.collider != null || distanceTraveled >= maxDashDistance)
        {
            SetState(State.Stunned);
        }
    }

    void StunnedLogic()
    {
        if (stateTimer >= stunTime)
        {
            SetState(State.Patrol);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 eye = (Vector2)transform.position;
        Vector2 targetEye = (Vector2)player.position;
        Vector2 toPlayer = targetEye - eye;

        if (toPlayer.magnitude > viewRange) return false;

        Vector2 facingVector = new Vector2(facingDirection, 0f);
        if (Vector2.Angle(facingVector, toPlayer) > fov / 2f) return false;

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

        if (anim != null)
        {
            if (nextState == State.Dash)
            {
                anim.SetTrigger("Attack"); 
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        Gizmos.color = Color.red;
        Vector3 left = Quaternion.Euler(0, 0, fov / 2f) * Vector3.right * facingDirection;
        Vector3 right = Quaternion.Euler(0, 0, -fov / 2f) * Vector3.right * facingDirection;
        Gizmos.DrawRay(transform.position, left * viewRange);
        Gizmos.DrawRay(transform.position, right * viewRange);
    }
}