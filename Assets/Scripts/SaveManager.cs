using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public SaveData currentData;

    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Игра успешно сохранена в: " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении игры: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Игра успешно загружена!");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Файл сохранения поврежден. Создаем дефолтные данные. Ошибка: " + e.Message);
                currentData = new SaveData();
                SaveGame();
            }
        }
        else
        {
            Debug.Log("Файл сохранения не найден. Создаем новый.");
            currentData = new SaveData();
            SaveGame();
        }
    }

    public void UnlockLevel(int levelIndex)
    {
        if (!currentData.unlockedLevels.Contains(levelIndex))
        {
            currentData.unlockedLevels.Add(levelIndex);
            SaveGame();
        }
    }

    
    public void ResetProgress()
    {
        currentData = new SaveData();
        SaveGame();
        Debug.Log("Прогресс сброшен!");

        MainMenu mainMenu = Object.FindFirstObjectByType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.RefreshLevelButtons();
        }
    }
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame(); 
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame(); 
    }
}