using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 3.0f; // Время жизни в секундах

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Автоматически уничтожаем пулю через 3 секунды, если она никуда не попала
        Destroy(gameObject, lifetime);
    }

    // Метод задает направление полета (вызывается из скрипта врага при спавне)
    public void Initialize(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        // Отключаем гравитацию, чтобы пуля летела ровно
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем столкновение с игроком
        if (collision.CompareTag("Player") || collision.gameObject.name.Contains("Main guy"))
        {
            PlayerHealth hp = collision.GetComponent<PlayerHealth>();
            if (hp == null)
            {
                hp = collision.GetComponentInParent<PlayerHealth>();
            }

            if (hp != null)
            {
                hp.TakeDamage(damage);
            }

            // Уничтожаем пулю при попадании в игрока
            Destroy(gameObject);
        }
        // Если пуля врезалась в стену (слой или тег Obstacle / Ground)
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Destroy(gameObject);
        }
    }
}