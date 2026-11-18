using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelData currentLevelData;

    void Start()
    {
        if (currentLevelData != null)
        {
            Debug.Log("Запущен уровень: " + currentLevelData.levelName);
            Debug.Log("Лимит времени: " + currentLevelData.timeLimit + " секунд");
            Debug.Log("Врагов на уровне: " + currentLevelData.enemyCount);
        }
    }
}