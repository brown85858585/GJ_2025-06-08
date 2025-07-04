using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum MusicTrack
{
    Track1 = 0,
    Track2 = 1,
    Track3= 2,
    Track4 = 3,
    Track5 = 4,
    // при необходимости добавляйте новые треки
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Список треков в порядке, соответствующем enum MusicTrack")]
    [SerializeField]
    private List<AudioClip> audioClips = new List<AudioClip>();

    [Header("Settings")]
    // По умолчанию звук включен
    [SerializeField]
    private bool soundOn = true;

    private AudioSource audioSource;
    private MusicTrack currentTrack;
    
    public bool IsOn => soundOn; // Свойство для проверки состояния звука

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            // Загрузка сохранённого состояния звука (0 — выключено, 1 — включено)
            soundOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
            audioSource.mute = !soundOn;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // В первой сцене сразу запускаем первый трек (если звук включен)
        SetTrack(MusicTrack.Track1);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Здесь можно добавить логику автоматической смены треков для разных сцен,  
        // но по умолчанию ничего не меняем
    }

    /// <summary>
    /// Устанавливает и (опционально) воспроизводит трек по его enum-идентификатору.
    /// </summary>
    public void SetTrack(MusicTrack trackId)
    {
        // Защита от выхода за границы списка
        int idx = (int)trackId;
        if (idx < 0 || idx >= audioClips.Count)
        {
            Debug.LogWarning($"MusicManager: трек с индексом {idx} не найден в audioClips.");
            return;
        }

        // Если тот же трек — ничего не делаем
        if (currentTrack == trackId && audioSource.clip != null)
            return;

        currentTrack = trackId;
        audioSource.clip = audioClips[idx];

        if (soundOn)
            audioSource.Play();
        else
            audioSource.Stop();
    }

    /// <summary>
    /// Переключает состояние звука: включает или выключает.
    /// </summary>
    public void ToggleSound(bool on)
    {
        soundOn = on;
        audioSource.mute = !soundOn;

        if (soundOn)
        {
            if (audioSource.clip != null && !audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }

        PlayerPrefs.SetInt("MusicOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
