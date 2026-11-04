using UnityEngine;

public class SlimePatrol : MonoBehaviour
{
    [Header("Настройки патрулирования")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float moveDistance = 3f;

    private Vector3 startPosition;
    private bool movingRight = true;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= startPosition.x + moveDistance)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= startPosition.x - moveDistance)
            {
                movingRight = true;
                Flip();
            }
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}