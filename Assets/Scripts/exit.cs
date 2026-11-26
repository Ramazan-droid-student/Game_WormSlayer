using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Настройки уровня")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private int levelToUnlock = 2;
    [SerializeField] private float loadDelay = 1.2f;   // чтобы успел прозвучать win

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        var controller = collision.GetComponent<PlayerController>();
        if (controller == null) controller = collision.GetComponentInParent<PlayerController>();
        if (controller == null || controller.IsDead) return;

        triggered = true;
        StartCoroutine(WinRoutine(controller));
    }

    private IEnumerator WinRoutine(PlayerController controller)
    {
        // остановить героя
        controller.SetMobileHorizontalInput(0f);
        controller.enabled = false;
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // звук победы
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWin();

        // открыть следующий уровень и сохранить
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnlockLevel(levelToUnlock);
            Debug.Log("Уровень " + levelToUnlock + " разблокирован и сохранён!");
        }

        yield return new WaitForSecondsRealtime(loadDelay);

        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextLevelName))
            SceneManager.LoadScene(nextLevelName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}