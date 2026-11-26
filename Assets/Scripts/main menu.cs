using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Кнопки уровней (по порядку: Уровень 1, 2, 3, 4)")]
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        ShowOnly(mainMenuPanel);

        if (SaveManager.Instance != null)
            SaveManager.Instance.OnProgressChanged += UpdateLevelButtons;

        UpdateLevelButtons();
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnProgressChanged -= UpdateLevelButtons;
    }

    // ---------- Панели ----------
    private void ShowOnly(GameObject panel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(panel == mainMenuPanel);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(panel == levelSelectPanel);
        if (settingsPanel != null) settingsPanel.SetActive(panel == settingsPanel);
    }

    public void OpenLevelSelect()
    {
        ShowOnly(levelSelectPanel);
        UpdateLevelButtons();
    }

    public void CloseLevelSelect() => ShowOnly(mainMenuPanel);

    public void OpenSettings() => ShowOnly(settingsPanel);

    public void CloseSettings() => ShowOnly(mainMenuPanel);

    // ---------- Сохранение ----------
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
        {
            SaveManager.Instance.ResetProgress();
            // после сброса применяем звук из свежих данных
            if (AudioManager.Instance != null)
                AudioManager.Instance.ApplySavedVolumes();
        }
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
            levelButtons[i].interactable = unlockedLevels.Contains(i + 1);
        }
    }

    public void RefreshLevelButtons() => UpdateLevelButtons();

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