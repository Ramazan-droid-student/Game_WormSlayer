using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Reference")]
    public Slider healthSlider;

    [Header("Combat Settings")]
    [SerializeField] private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TryAttack()
    {
        // Атака больше не требует стамины
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            // Плавное уменьшение/увеличение полоски здоровья
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * 10f);
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб!");
    }

}