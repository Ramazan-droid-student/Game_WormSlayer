using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int version = 1;
    public int bestScore = 0;
    public float musicVolume = 1f;
    public List<int> unlockedLevels = new List<int> { 1 };
}