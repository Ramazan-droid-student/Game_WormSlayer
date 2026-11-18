using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Кнопки уровней (по порядку: Уровень 1, 2, 3, 4)")]
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnProgressChanged += UpdateLevelButtons;
        }

        UpdateLevelButtons();
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnProgressChanged -= UpdateLevelButtons;
        }
    }

    // ---------- Панели ----------

    public void OpenLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);

        UpdateLevelButtons();
    }

    public void CloseLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }
    public void OnSaveButtonPressed()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
        else
            Debug.LogWarning("SaveManager.Instance не найден!");
    }

    public void OnResetButtonPressed()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetProgress();
        else
            Debug.LogWarning("SaveManager.Instance не найден!");
    }
    private void UpdateLevelButtons()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.currentData == null) return;

        var unlockedLevels = SaveManager.Instance.currentData.unlockedLevels;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;

            int levelIndex = i + 1;
            levelButtons[i].interactable = unlockedLevels.Contains(levelIndex);
        }
    }

    // Оставлено для совместимости, если где-то ещё вызывается
    public void RefreshLevelButtons()
    {
        UpdateLevelButtons();
    }

    // ---------- Запуск уровней ----------

    public void PlayLevel(string levelSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSceneName);
    }

    public void PlayGame(string levelSceneName = "Level 1")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}