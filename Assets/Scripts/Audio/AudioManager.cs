using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理器单例
/// 在场景中挂载到任意物体上即可自动工作
/// 如果不挂载，所有音效触发静默跳过（不会报错）
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("音效列表（名称 + 音频片段）")]
    [SerializeField] private SoundClip[] soundClips;

    [Header("背景音乐（名称 + 音频片段）")]
    [SerializeField] private SoundClip[] bgmClips;

    private Dictionary<string, AudioClip> _sfxDict = new();
    private Dictionary<string, AudioClip> _bgmDict = new();

    [System.Serializable]
    public class SoundClip
    {
        public string name;
        public AudioClip clip;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 如果没拖入 AudioSource，自动创建
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;
        }

        // 构建查找表
        foreach (var s in soundClips)
            if (s != null && !string.IsNullOrEmpty(s.name) && s.clip != null)
                _sfxDict[s.name] = s.clip;
        foreach (var b in bgmClips)
            if (b != null && !string.IsNullOrEmpty(b.name) && b.clip != null)
                _bgmDict[b.name] = b.clip;
    }

    private void OnEnable()
    {
        AudioEvents.Center.AddListener<string>(AudioEvent.PlaySFX, OnPlaySFX);
        AudioEvents.Center.AddListener<string>(AudioEvent.PlayBGM, OnPlayBGM);
        AudioEvents.Center.AddListener(AudioEvent.StopBGM, OnStopBGM);
    }

    private void OnDisable()
    {
        AudioEvents.Center.RemoveListener<string>(AudioEvent.PlaySFX, OnPlaySFX);
        AudioEvents.Center.RemoveListener<string>(AudioEvent.PlayBGM, OnPlayBGM);
        AudioEvents.Center.RemoveListener(AudioEvent.StopBGM, OnStopBGM);
    }

    private void OnPlaySFX(string clipName)
    {
        if (sfxSource == null) return;
        if (_sfxDict.TryGetValue(clipName, out var clip) && clip != null)
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"找不到音效: {clipName}");
    }

    private void OnPlayBGM(string clipName)
    {
        if (bgmSource == null) return;
        if (_bgmDict.TryGetValue(clipName, out var clip) && clip != null)
        {
            if (bgmSource.clip == clip) return; // 已在这首
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
        }
        else
            Debug.LogWarning($"找不到背景音乐: {clipName}");
    }

    private void OnStopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }
}
