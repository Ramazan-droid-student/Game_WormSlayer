using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int version = 1;
    public int bestScore = 0;
    public List<int> unlockedLevels = new List<int> { 1 };
    public float musicVolume = 0.8f;
    public float sfxVolume = 0.8f;
    public bool musicMuted = false;
    public bool sfxMuted = false;
}