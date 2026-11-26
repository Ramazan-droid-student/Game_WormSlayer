using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Reference")]
    public Slider healthSlider;

    [Header("Death / Restart")]
    [Tooltip("Если игрок упал ниже этой высоты (Y), он сразу погибает")]
    [SerializeField] private float fallDeathY = -10f;
    [SerializeField] private bool autoRestart = true;
    [Tooltip("Должно быть не короче анимации смерти")]
    [SerializeField] private float restartDelay = 1.5f;

    private PlayerController controller;

    public bool IsDead { get; private set; }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        if (controller == null) controller = GetComponentInParent<PlayerController>();
    }

    void Start()
    {
        Time.timeScale = 1f;
        currentHealth = maxHealth;
        IsDead = false;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        UpdateUI();

        if (!IsDead && transform.position.y < fallDeathY)
            Kill();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
        else if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHit();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void Kill()
    {
        if (IsDead) return;
        currentHealth = 0;
        Die();
    }
    public void TryAttack()
    {
        if (controller != null) controller.Attack();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * 10f);
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("Игрок погиб!");

        if (controller != null) controller.Die();   

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeath();
            AudioManager.Instance.PlayLose();
        }

        if (autoRestart)
            StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        RestartLevel();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}