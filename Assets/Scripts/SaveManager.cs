using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public SaveData currentData;

    // Вызывается, когда прогресс изменился (разблокировка, сброс, загрузка)
    public event Action OnProgressChanged;

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
        catch (Exception e)
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

                // Защита: пустой файл или битые данные
                if (currentData == null || currentData.unlockedLevels == null)
                {
                    throw new Exception("Данные сохранения пусты");
                }

                // Первый уровень всегда должен быть открыт
                if (!currentData.unlockedLevels.Contains(1))
                {
                    currentData.unlockedLevels.Add(1);
                }

                Debug.Log("Игра успешно загружена!");
            }
            catch (Exception e)
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

        OnProgressChanged?.Invoke();
    }

    public void UnlockLevel(int levelIndex)
    {
        if (!currentData.unlockedLevels.Contains(levelIndex))
        {
            currentData.unlockedLevels.Add(levelIndex);
            SaveGame();
            Debug.Log("Уровень " + levelIndex + " успешно разблокирован и сохранен!");
            OnProgressChanged?.Invoke();
        }
    }

    public void ResetProgress()
    {
        currentData = new SaveData(); // открыт только 1-й уровень
        SaveGame();
        Debug.Log("Прогресс сброшен!");
        OnProgressChanged?.Invoke();  // MainMenu сам обновит кнопки
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}