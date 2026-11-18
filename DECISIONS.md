# Decision Log — Assignment 11 (Data and Saves)

2026-11-16  Asked AI to build a JSON save system with JsonUtility. Added a manual null-check for the unlockedLevels list after deserialization because JsonUtility returns null for empty lists.
2026-11-17  Integrated level unlocking into LevelPortal and MainMenu. Replaced automatic buildIndex math with an explicit levelToUnlock integer in the Inspector to prevent scene order mismatches.
2026-11-18  Fixed SaveManager persistence across scenes by moving it out of the UI Canvas to the root Hierarchy (DontDestroyOnLoad only works on root GameObjects).
2026-11-18  Updated ResetProgress() in SaveManager to call RefreshLevelButtons() on MainMenu so locked level buttons update their interactable state immediately on click.
2026-11-18  Moved enemy and level configuration values out of scripts into ScriptableObject data assets (LevelData) so all 3 levels load from data without code edits.
