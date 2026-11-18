using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Identification")]
    public string levelName = "Level 1";
    public int levelIndex = 1;
    public string sceneName = "Level 1"; 

    [Header("Level Parameters")]
    public float timeLimit = 90f;
    public int enemyCount = 4;
}