using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("AudioSources")]
    [SerializeField] private AudioSource sfxSource;   // Источник для звуковых эффектов
    [SerializeField] private AudioSource musicSource; // Источник для музыки

    [Header("SFX Clips")]
    [SerializeField] private AudioData[] sfxClips; 

    [Header("Music Tracks")]
    [SerializeField] private AudioData[] musicClips;

    // Для быстрого поиска по идентификатору используем словари
    private Dictionary<string, AudioClip> sfxDictionary;
    private Dictionary<string, AudioClip> musicDictionary;

    // Флаги мьюта для SFX и музыки
    private bool isSFXMuted;
    private bool isMusicMuted;

    private void Awake()
    {
        // Singleton-паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Заполняем словари для быстрого доступа по id
    private void InitializeAudioDictionaries()
    {
        sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var audioData in sfxClips)
        {
            if (!sfxDictionary.ContainsKey(audioData.Id))
                sfxDictionary.Add(audioData.Id, audioData.Clip);
            else
                Debug.LogWarning("Дублирование SFX Id: " + audioData.Id);
        }

        musicDictionary = new Dictionary<string, AudioClip>();
        foreach (var audioData in musicClips)
        {
            if (!musicDictionary.ContainsKey(audioData.Id))
                musicDictionary.Add(audioData.Id, audioData.Clip);
            else
                Debug.LogWarning("Дублирование Music Id: " + audioData.Id);
        }
    }

    /// <summary>
    /// Воспроизводит звуковой эффект по его идентификатору.
    /// </summary>
    public void PlaySFX(string id)
    {
        if (isSFXMuted)
            return;

        if (sfxDictionary.TryGetValue(id, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SFX с id " + id + " не найден!");
        }
    }

    /// <summary>
    /// Воспроизводит музыкальную дорожку по идентификатору.
    /// </summary>
    public void PlayMusic(string id)
    {
        if (isMusicMuted)
            return;

        if (musicDictionary.TryGetValue(id, out AudioClip clip))
        {
            // Если уже воспроизводится музыка – останавливаем её
            if (musicSource.isPlaying)
                musicSource.Stop();

            musicSource.clip = clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music track с id " + id + " не найден!");
        }
    }

    /// <summary>
    /// Останавливает воспроизведение музыки.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    /// <summary>
    /// Переключает мьют для SFX.
    /// </summary>
    public void ToggleSFXMute()
    {
        isSFXMuted = !isSFXMuted;
        sfxSource.mute = isSFXMuted;
        // Можно добавить сохранение настроек в PlayerPrefs
    }

    /// <summary>
    /// Переключает мьют для музыки.
    /// </summary>
    public void ToggleMusicMute()
    {
        isMusicMuted = !isMusicMuted;
        musicSource.mute = isMusicMuted;
        // Можно добавить сохранение настроек в PlayerPrefs
    }
}

[Serializable]
public class AudioData
{
    [SerializeField] private string _id;
    [SerializeField] private AudioClip _clip;

    public string Id => _id;
    public AudioClip Clip => _clip;
}
