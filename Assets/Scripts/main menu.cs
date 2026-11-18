using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Обязательно для работы с Button

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    [SerializeField] private GameObject mainMenuPanel; 
    [SerializeField] private GameObject levelSelectPanel; 

    [Header("Кнопки уровней (По порядку: Уровень 1, Уровень 2, Уровень 3)")]
    [SerializeField] private Button[] levelButtons; 

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

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

    private void UpdateLevelButtons()
    {
        if (SaveManager.Instance == null) return;

        var unlockedLevels = SaveManager.Instance.currentData.unlockedLevels;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;

            int levelIndex = i + 1; 
            bool isUnlocked = unlockedLevels.Contains(levelIndex);

            levelButtons[i].interactable = isUnlocked;
        }
    }

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
    public void RefreshLevelButtons()
    {
        UpdateLevelButtons();
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}