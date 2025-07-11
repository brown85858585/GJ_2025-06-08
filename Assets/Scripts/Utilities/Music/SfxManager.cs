using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum SfxId
{
    VomitDrain,
    Toilet,
    InnerDoorOpen,
    InnerDoorClose,
    
}

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Serializable]
    private struct SoundEntry
    {
        public SfxId id;
        public AudioClip clip;
    }

    [Header("Сериализуемые клипы")]
    [SerializeField] private List<SoundEntry> _sounds = new();

    private Dictionary<SfxId, AudioClip> _dict;
    private AudioSource _source;

    private void Awake()
    {
        // Singleton-охрана
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Подготовка аудио
        _source = gameObject.AddComponent<AudioSource>();
        _dict   = _sounds.ToDictionary(s => s.id, s => s.clip);
    }

    /// <summary>
    /// Проиграть клип и дождаться его окончания.
    /// Пример: await SfxManager.Play(SfxId.VomitDrain);
    /// </summary>
    public static UniTask Play(SfxId id, float volume = 1f) =>
        Instance.PlayInternal(id, volume);

    /// <summary>
    /// Fire-and-forget вариант, если ждать не нужно.
    /// Пример: SfxManager.PlayOneShot(SfxId.Hit);
    /// </summary>
    public static void PlayOneShot(SfxId id, float volume = 1f) =>
        _ = Instance.PlayInternal(id, volume, false);

    // ──────────────────────────────────────────────────────────────────────────────

    private async UniTask PlayInternal(SfxId id, float volume, bool awaitEnd = true)
    {
        if (!_dict.TryGetValue(id, out var clip) || clip == null)
            return; // нет такого клипа ─ тихо выходим

        _source.PlayOneShot(clip, volume);

        if (awaitEnd)
        {
            // Ждём длину клипа (корректно отменяется, если объект уничтожен)
            await UniTask.Delay(
                TimeSpan.FromSeconds(clip.length),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
