using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Настройки уровня")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private int levelToUnlock = 2; 
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            LoadNextLevel();
        }
    }

    void LoadNextLevel()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnlockLevel(levelToUnlock);
            Debug.Log("Уровень " + levelToUnlock + " успешно разблокирован и сохранен!");
        }

        // 2. Загружаем следующий уровень
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}