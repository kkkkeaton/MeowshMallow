using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏音频管理：集中挂载 BGM、小音效引用，提供公用播放方法。
/// BGM 切换支持交叉淡入淡出（需两个 AudioSource）；仅填一个则硬切。
/// 可通过 God.Instance?.Get&lt;AudioManager&gt;() 获取。
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    [Tooltip("BGM 主音源，不填则使用本物体上的第一个 AudioSource。")]
    [SerializeField] private AudioSource _bgmSource;
    [Tooltip("BGM 副音源，用于交叉淡入淡出；不填则切换时硬切。")]
    [SerializeField] private AudioSource _bgmSourceSecondary;
    [Tooltip("交叉淡入淡出时长（秒）。")]
    [SerializeField] private float _bgmCrossfadeDuration = 2f;
    [Tooltip("平常状态 BGM（如 Src/Music/bgm3）。")]
    [SerializeField] private AudioClip _normalBgm;
    [Tooltip("暴露/被识破状态 BGM（如 Src/Music/danger）。")]
    [SerializeField] private AudioClip _dangerBgm;

    private AudioSource _currentBgmSource;
    private Coroutine _crossfadeRoutine;

    [Header("小音效")]
    [Tooltip("播放小音效的 AudioSource，不填则使用本物体上的第二个 AudioSource 或与 BGM 共用。")]
    [SerializeField] private AudioSource _sfxSource;
    [Tooltip("捡起物品音效。")]
    [SerializeField] private AudioClip _pickupSfx;
    [Tooltip("暗杀成功音效。")]
    [SerializeField] private AudioClip _assassinationSfx;

    private void Awake()
    {
        if (_bgmSource == null)
            _bgmSource = GetComponent<AudioSource>();
        if (_bgmSource != null)
        {
            _bgmSource.volume = 1f;
            _currentBgmSource = _bgmSource;
        }
        if (_bgmSourceSecondary != null)
            _bgmSourceSecondary.volume = 0f;

        if (_sfxSource == null)
        {
            var sources = GetComponents<AudioSource>();
            _sfxSource = sources.Length > 1 ? sources[1] : _bgmSource;
        }

        if (God.Instance != null)
            God.Instance.Add(this);
    }

    // ---------- BGM ----------

    /// <summary>播放平常状态 BGM（支持与当前 BGM 交叉淡入淡出）。</summary>
    public void PlayNormalBgm()
    {
        if (_normalBgm == null) return;
        PlayBgmWithCrossfade(_normalBgm);
    }

    /// <summary>播放暴露/危险状态 BGM（支持与当前 BGM 交叉淡入淡出）。</summary>
    public void PlayDangerBgm()
    {
        if (_dangerBgm == null) return;
        PlayBgmWithCrossfade(_dangerBgm);
    }

    /// <summary>切换到指定 BGM：若配置了副音源则交叉淡入淡出，否则硬切。</summary>
    private void PlayBgmWithCrossfade(AudioClip clip)
    {
        if (_currentBgmSource == null)
        {
            if (_bgmSource != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.loop = true;
                _bgmSource.volume = 1f;
                _bgmSource.Play();
                _currentBgmSource = _bgmSource;
            }
            return;
        }

        if (_currentBgmSource.clip == clip && _currentBgmSource.isPlaying)
            return;

        if (_bgmSourceSecondary == null || _bgmCrossfadeDuration <= 0f)
        {
            _currentBgmSource.clip = clip;
            _currentBgmSource.loop = true;
            _currentBgmSource.volume = 1f;
            _currentBgmSource.Play();
            return;
        }

        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);

        AudioSource from = _currentBgmSource;
        AudioSource to = _currentBgmSource == _bgmSource ? _bgmSourceSecondary : _bgmSource;

        to.clip = clip;
        to.loop = true;
        to.volume = 0f;
        to.Play();

        _crossfadeRoutine = StartCoroutine(CrossfadeRoutine(from, to));
    }

    private IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to)
    {
        float duration = Mathf.Max(0.01f, _bgmCrossfadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            from.volume = 1f - t;
            to.volume = t;
            yield return null;
        }

        from.Stop();
        from.volume = 1f;
        to.volume = 1f;
        _currentBgmSource = to;
        _crossfadeRoutine = null;
    }

    // ---------- 小音效（OneShot，不打断 BGM） ----------

    /// <summary>播放捡起物品音效。</summary>
    public void PlayPickupSfx()
    {
        if (_sfxSource != null && _pickupSfx != null)
            _sfxSource.PlayOneShot(_pickupSfx);
    }

    /// <summary>播放暗杀成功音效。</summary>
    public void PlayAssassinationSfx()
    {
        if (_sfxSource != null && _assassinationSfx != null)
            _sfxSource.PlayOneShot(_assassinationSfx);
    }

    /// <summary>播放任意小音效（扩展用）。</summary>
    public void PlaySfx(AudioClip clip)
    {
        if (_sfxSource != null && clip != null)
            _sfxSource.PlayOneShot(clip);
    }
}
