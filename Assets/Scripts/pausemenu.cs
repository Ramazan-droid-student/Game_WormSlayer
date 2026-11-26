using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Из других скриптов: if (PauseMenu.IsPaused) return;
    public static bool IsPaused { get; private set; }

    [Header("Панель паузы (внутри неё настройки звука)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Кнопки")]
    [SerializeField] private Button pauseButton;     // кнопка паузы в игре
    [SerializeField] private Button resumeButton;    // «Продолжить»
    [SerializeField] private Button restartButton;   // не обязательно
    [SerializeField] private Button menuButton;      // не обязательно

    [Header("Музыка")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;

    [Header("Эффекты (SFX)")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    [Header("Сцены")]
    [SerializeField] private string menuSceneName = "Main Menu";

    private SaveData Data =>
        SaveManager.Instance != null ? SaveManager.Instance.currentData : null;

    // ---------- Жизненный цикл ----------
    void Awake()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (menuButton != null) menuButton.onClick.AddListener(ToMainMenu);

        if (musicSlider != null) { musicSlider.minValue = 0.0001f; musicSlider.maxValue = 1f; }
        if (sfxSlider != null) { sfxSlider.minValue = 0.0001f; sfxSlider.maxValue = 1f; }

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSlider);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggle);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxToggle);
    }

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // Esc на ПК, кнопка «Назад» на Android
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseButton != null) pauseButton.onClick.RemoveListener(Pause);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        if (menuButton != null) menuButton.onClick.RemoveListener(ToMainMenu);

        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSlider);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSlider);
        if (musicToggle != null) musicToggle.onValueChanged.RemoveListener(OnMusicToggle);
        if (sfxToggle != null) sfxToggle.onValueChanged.RemoveListener(OnSfxToggle);
    }

    void OnApplicationPause(bool paused) { if (paused) SaveToDisk(); }
    void OnApplicationQuit() { SaveToDisk(); }

    // ---------- Пауза ----------
    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        SyncUiFromSave();                 // показать сохранённые значения
        if (pausePanel != null) pausePanel.SetActive(true);

        Time.timeScale = 0f;
        if (AudioManager.Instance != null) AudioManager.Instance.SetPausedAudio(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (AudioManager.Instance != null) AudioManager.Instance.SetPausedAudio(false);
        SaveToDisk();
    }

    public void Restart()
    {
        SaveToDisk();
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMainMenu()
    {
        SaveToDisk();
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // ---------- Настройки звука ----------
    private void SyncUiFromSave()
    {
        var d = Data;
        if (d == null) return;

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(d.musicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(d.sfxVolume);
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(!d.musicMuted);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(!d.sfxMuted);
        RefreshInteractable();
    }

    private void OnMusicSlider(float v)
    {
        if (Data != null) Data.musicVolume = v;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
    }

    private void OnSfxSlider(float v)
    {
        if (Data != null) Data.sfxVolume = v;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
    }

    private void OnMusicToggle(bool isOn)
    {
        if (Data != null) Data.musicMuted = !isOn;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicMuted(!isOn);
        RefreshInteractable();
        SaveToDisk();
    }

    private void OnSfxToggle(bool isOn)
    {
        if (Data != null) Data.sfxMuted = !isOn;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxMuted(!isOn);
            if (isOn) AudioManager.Instance.PlayUiClick();
        }
        RefreshInteractable();
        SaveToDisk();
    }

    private void RefreshInteractable()
    {
        if (musicSlider != null && musicToggle != null) musicSlider.interactable = musicToggle.isOn;
        if (sfxSlider != null && sfxToggle != null) sfxSlider.interactable = sfxToggle.isOn;
    }

    private void SaveToDisk()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
    }
}