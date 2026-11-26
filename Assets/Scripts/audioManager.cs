using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer & Snapshots")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private AudioMixerSnapshot pausedSnapshot;
    [SerializeField] private float snapshotTransitionTime = 0.25f;

    [Header("Music (Crossfade)")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private string menuSceneName = "Main Menu";
    [SerializeField] private float crossfadeDuration = 1.0f;

    [Header("SFX")]
    [Tooltip("Источник нужен только для группы Output = SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] hitClips;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip[] enemyWindupClips;
    [SerializeField] private AudioClip[] deathClips;
    [SerializeField] private AudioClip[] uiClickClips;
    [SerializeField] private AudioClip[] winClips;
    [SerializeField] private AudioClip[] loseClips;

    [Header("Pitch Variation")]
    [SerializeField] private float minPitch = 0.92f;
    [SerializeField] private float maxPitch = 1.08f;

    private const string MusicParam = "MusicVol";
    private const string SfxParam = "SFXVol";
    private const int SfxPoolSize = 8;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private Coroutine crossfadeRoutine;
    private bool isLevelMusicActive;

    private AudioSource[] sfxPool;
    private int sfxPoolIndex;

    private float musicVolume01 = 0.8f;
    private float sfxVolume01 = 0.8f;
    private bool musicMuted;
    private bool sfxMuted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            activeMusicSource = musicSourceA;
            inactiveMusicSource = musicSourceB;

            BuildSfxPool();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySavedVolumes();
        EvaluateMusicForScene(SceneManager.GetActiveScene().name, true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPausedAudio(false);
        ApplySavedVolumes();
        EvaluateMusicForScene(scene.name, false);
    }

    // ---------- SFX pool ----------
    private void BuildSfxPool()
    {
        sfxPool = new AudioSource[SfxPoolSize];
        for (int i = 0; i < SfxPoolSize; i++)
        {
            var go = new GameObject("SFX_" + i);
            go.transform.SetParent(transform);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            if (sfxSource != null)
                s.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            sfxPool[i] = s;
        }
    }

    private void PlayVariedClip(AudioClip[] clips)
    {
        if (sfxPool == null || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        AudioSource s = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;

        s.Stop();
        s.pitch = Random.Range(minPitch, maxPitch);
        s.clip = clip;
        s.Play();
    }

    // ---------- Music ----------
    private void EvaluateMusicForScene(string sceneName, bool instant)
    {
        bool wantLevelMusic = sceneName != menuSceneName;

        if (wantLevelMusic && isLevelMusicActive &&
            activeMusicSource.clip == levelMusic && activeMusicSource.isPlaying)
            return;

        AudioClip targetClip = wantLevelMusic ? levelMusic : menuMusic;
        isLevelMusicActive = wantLevelMusic;
        if (targetClip == null) return;

        if (instant || activeMusicSource.clip == null)
        {
            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            activeMusicSource.clip = targetClip;
            activeMusicSource.volume = 1f;
            activeMusicSource.Play();
            inactiveMusicSource.Stop();
            inactiveMusicSource.volume = 0f;
            return;
        }

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeTo(targetClip));
    }

    private IEnumerator CrossfadeTo(AudioClip newClip)
    {
        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        float startVol = activeMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);
            activeMusicSource.volume = Mathf.Lerp(startVol, 0f, t);
            inactiveMusicSource.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        activeMusicSource.volume = 0f;
        activeMusicSource.Stop();
        inactiveMusicSource.volume = 1f;

        var temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
        crossfadeRoutine = null;
    }

    // ---------- Pause ----------
    public void SetPausedAudio(bool isPaused)
    {
        if (isPaused && pausedSnapshot != null)
            pausedSnapshot.TransitionTo(snapshotTransitionTime);
        else if (!isPaused && normalSnapshot != null)
            normalSnapshot.TransitionTo(snapshotTransitionTime);
    }

    // ---------- Volume (Log10 * 20) + Mute ----------
    public void SetMusicVolume(float sliderValue)
    {
        musicVolume01 = sliderValue;
        ApplyMusic();
    }

    public void SetSfxVolume(float sliderValue)
    {
        sfxVolume01 = sliderValue;
        ApplySfx();
    }

    public void SetMusicMuted(bool muted)
    {
        musicMuted = muted;
        ApplyMusic();
    }

    public void SetSfxMuted(bool muted)
    {
        sfxMuted = muted;
        ApplySfx();
    }

    private void ApplyMusic()
    {
        if (mixer == null) return;
        mixer.SetFloat(MusicParam, musicMuted ? -80f : LinearToDecibels(musicVolume01));
    }

    private void ApplySfx()
    {
        if (mixer == null) return;
        mixer.SetFloat(SfxParam, sfxMuted ? -80f : LinearToDecibels(sfxVolume01));
    }

    public static float LinearToDecibels(float linear)
    {
        float clamped = Mathf.Clamp(linear, 0.0001f, 1f);
        return Mathf.Log10(clamped) * 20f;
    }

    public void ApplySavedVolumes()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.currentData != null)
        {
            var d = SaveManager.Instance.currentData;
            musicVolume01 = d.musicVolume;
            sfxVolume01 = d.sfxVolume;
            musicMuted = d.musicMuted;
            sfxMuted = d.sfxMuted;
            ApplyMusic();
            ApplySfx();
        }
    }

    // ---------- 8 sound events ----------
    public void PlayJump() => PlayVariedClip(jumpClips);
    public void PlayHit() => PlayVariedClip(hitClips);
    public void PlayAttack() => PlayVariedClip(attackClips);
    public void PlayEnemyWindup() => PlayVariedClip(enemyWindupClips);
    public void PlayDeath() => PlayVariedClip(deathClips);
    public void PlayUiClick() => PlayVariedClip(uiClickClips);
    public void PlayWin() => PlayVariedClip(winClips);
    public void PlayLose() => PlayVariedClip(loseClips);
}