using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;      // 背景音乐播放器
    [SerializeField] private AudioSource sfxSource;        // 音效播放器

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusic;     // 背景音乐
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("音效")]
    [SerializeField] private AudioClip attackSound;         // 攻击音效
    [SerializeField] private AudioClip jumpSound;           // 跳跃音效
    [SerializeField] private AudioClip killSound;           // 击杀敌人音效
    [SerializeField] private AudioClip deathSound;          // 玩家死亡音效
    [SerializeField] private AudioClip stompSound;          // 踩踏敌人音效
    [SerializeField] private AudioClip scoreSound;          // 得分音效
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;

    [Header("音效设置")]
    [SerializeField] private bool musicOn = true;           // 音乐开关
    [SerializeField] private bool sfxOn = true;             // 音效开关

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        // 单例模式
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 自动创建音频源（如果没有指定）
        SetupAudioSources();
    }

    void Start()
    {
        // 开始播放背景音乐
        PlayBackgroundMusic();
    }

    /// <summary>
    /// 设置音频源
    /// </summary>
    private void SetupAudioSources()
    {
        // 创建背景音乐音频源
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        // 创建音效音频源
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (musicOn && backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("背景音乐已开始播放");
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            Debug.Log("背景音乐已停止");
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBackgroundMusic()
    {
        if (musicSource != null && musicOn)
        {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// 播放攻击音效
    /// </summary>
    public void PlayAttackSound()
    {
        PlaySFX(attackSound, "攻击音效");
    }

    /// <summary>
    /// 播放跳跃音效
    /// </summary>
    public void PlayJumpSound()
    {
        PlaySFX(jumpSound, "跳跃音效");
    }

    /// <summary>
    /// 播放击杀音效
    /// </summary>
    public void PlayKillSound()
    {
        PlaySFX(killSound, "击杀音效");
    }

    /// <summary>
    /// 播放死亡音效
    /// </summary>
    public void PlayDeathSound()
    {
        PlaySFX(deathSound, "死亡音效");
    }

    /// <summary>
    /// 播放踩踏音效
    /// </summary>
    public void PlayStompSound()
    {
        PlaySFX(stompSound, "踩踏音效");
    }

    /// <summary>
    /// 播放得分音效
    /// </summary>
    public void PlayScoreSound()
    {
        PlaySFX(scoreSound, "得分音效");
    }

    /// <summary>
    /// 播放音效的通用方法
    /// </summary>
    private void PlaySFX(AudioClip clip, string soundName)
    {
        if (sfxOn && clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"播放音效: {soundName}");
        }
        else if (clip == null)
        {
            Debug.LogWarning($"音效未设置: {soundName}");
        }
    }

    /// <summary>
    /// 设置背景音乐音量
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        Debug.Log($"背景音乐音量设置为: {musicVolume}");
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        Debug.Log($"音效音量设置为: {sfxVolume}");
    }

    /// <summary>
    /// 切换背景音乐开关
    /// </summary>
    public void ToggleMusic()
    {
        musicOn = !musicOn;
        if (musicOn)
        {
            PlayBackgroundMusic();
        }
        else
        {
            StopBackgroundMusic();
        }
        Debug.Log($"背景音乐开关: {(musicOn ? "开" : "关")}");
    }

    /// <summary>
    /// 切换音效开关
    /// </summary>
    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        Debug.Log($"音效开关: {(sfxOn ? "开" : "关")}");
    }

    /// <summary>
    /// 获取音乐开关状态
    /// </summary>
    public bool IsMusicOn()
    {
        return musicOn;
    }

    /// <summary>
    /// 获取音效开关状态
    /// </summary>
    public bool IsSFXOn()
    {
        return sfxOn;
    }
}
