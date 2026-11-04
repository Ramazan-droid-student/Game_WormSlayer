using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Имя сцены с уровнем")]
    [SerializeField] private string levelSceneName = "SampleScene";

    // Вызывается при нажатии на кнопку Play
    public void PlayGame()
    {
        Time.timeScale = 1f; // Сбрасываем паузу, если она была активна
        SceneManager.LoadScene(levelSceneName);
    }

    // Вызывается при нажатии на кнопку Quit
    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}