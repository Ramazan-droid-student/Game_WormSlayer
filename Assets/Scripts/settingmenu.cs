using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Музыка")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;

    [Header("Эффекты (SFX)")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    private SaveData Data =>
        SaveManager.Instance != null ? SaveManager.Instance.currentData : null;

    void OnEnable()
    {
        // защита от значения 0 в слайдерах
        musicSlider.minValue = 0.0001f;
        musicSlider.maxValue = 1f;
        sfxSlider.minValue = 0.0001f;
        sfxSlider.maxValue = 1f;

        var d = Data;
        if (d != null)
        {
            musicSlider.SetValueWithoutNotify(d.musicVolume);
            sfxSlider.SetValueWithoutNotify(d.sfxVolume);
            musicToggle.SetIsOnWithoutNotify(!d.musicMuted);
            sfxToggle.SetIsOnWithoutNotify(!d.sfxMuted);
        }
        RefreshInteractable();

        musicSlider.onValueChanged.AddListener(OnMusicSlider);
        sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        musicToggle.onValueChanged.AddListener(OnMusicToggle);
        sfxToggle.onValueChanged.AddListener(OnSfxToggle);
    }

    void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicSlider);
        sfxSlider.onValueChanged.RemoveListener(OnSfxSlider);
        musicToggle.onValueChanged.RemoveListener(OnMusicToggle);
        sfxToggle.onValueChanged.RemoveListener(OnSfxToggle);
        SaveToDisk();
    }

    void OnApplicationPause(bool paused) { if (paused) SaveToDisk(); }
    void OnApplicationQuit() { SaveToDisk(); }

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
            if (isOn) AudioManager.Instance.PlayUiClick(); // проверочный звук
        }
        RefreshInteractable();
        SaveToDisk();
    }

    private void RefreshInteractable()
    {
        musicSlider.interactable = musicToggle.isOn;
        sfxSlider.interactable = sfxToggle.isOn;
    }

    private void SaveToDisk()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
    }
}