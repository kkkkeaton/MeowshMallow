using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 怪物索敌与移动：在探测范围内发现 Player 后向 Player 移动至可配置的观察距离并面对 Player 观察；
/// 一旦玩家走出探测范围则停止跟随；玩家再次进入探测范围时会重新开始跟随。观察时累积识破值，满时增加玩家暴露值。
/// 需与 MonsterBase 挂在同一 GameObject 上。MonsterConfig 由 MonsterManager 在生成时自动注入。
/// </summary>
[RequireComponent(typeof(MonsterBase))]
public class MonsterAI : MonoBehaviour
{
    [Header("配置")]
    private MonsterConfig config; // 仅由 MonsterManager.SetConfig 在生成时注入
    [Tooltip("用于查找玩家的 Tag，不填则用 \"Player\"")]
    [SerializeField] private string playerTag = "Player";

    [Header("发现玩家")]
    [Tooltip("发现玩家时是否播放抖动小动画")]
    [SerializeField] private bool discoverShakeEnabled = true;
    [Tooltip("抖动持续时间（秒）")]
    [SerializeField] private float discoverShakeDuration = 2f;
    [Tooltip("抖动强度（2D 下为 XY 方向位移幅度）")]
    [SerializeField] private float discoverShakeStrength = 2f;

    [Header("状态表现（子物体 suspect / bark）")]
    [Tooltip("警觉状态表现：追踪/观察时一直显示；不填则用子物体名 suspect")]
    [SerializeField] private GameObject _suspect;
    [Tooltip("发现状态表现：识破值满时一直显示；不填则用子物体名 bark")]
    [SerializeField] private GameObject _bark;
    [Tooltip("符号 Animator 状态名：in（出现时播放）、loop（之后循环）")]
    [SerializeField] private string _symbolInState = "ani_suspect_in";
    [SerializeField] private string _symbolLoopState = "ani_suspect_loop";
    [SerializeField] private string _barkInState = "ani_bark_in";
    [SerializeField] private string _barkLoopState = "ani_bark_loop";

    [Header("调试")]
    [Tooltip("勾选后状态切换时在 Console 输出，便于验证索敌与移动")]
    [SerializeField] private bool debugLog;

    [SerializeField] private GameObject emojiConfused;

    private MonsterBase _monster;
    private Transform _player;

    private IMaskInfoProvider _playerMaskInfoProvider;
    private float _detectionRange;
    private float _approachDistance;
    private float _moveSpeed;
    private float _detectionFillRatePerSecond;
    private float _detectionMaxValue;
    private float _sameTypeThreshold;
    private AudioClip[] _moveSfxList;
    private float _moveSfxInterval;
    private float _moveSfxCooldown;
    private AudioClip _discoverSfx;
    private AudioClip _spottedSfx;

    private Animator _suspectAnimator;
    private Animator _barkAnimator;
    private Coroutine _suspectCoroutine;
    private Coroutine _barkCoroutine;

    /// <summary>当前是否被判定为同类（伪装成功）；仅在收到通知时更新，避免每帧检测。</summary>
    private bool _isSameType;

    /// <summary>当前识破值（0 ~ 满值）；满后不立刻清零，等死亡或丢失视野后清零。</summary>
    private float _currentDetectionValue;

    /// <summary>识破值满后已触发过 EnterSpotted/AddExposure，避免重复；丢失视野或死亡时重置。</summary>
    private bool _hasTriggeredDetectionFull;

    /// <summary>Idle=待机 Approaching=接近中 Observing=观察中 Disengaged=玩家已离开探测范围，待玩家再次进入后重新跟随</summary>
    private enum State { Idle, Approaching, Observing, Disengaged }
    private State _state = State.Idle;

    private void Awake()
    {
        _monster = GetComponent<MonsterBase>();
        if (_monster == null) return;
        if (_suspect == null) _suspect = transform.Find("suspect")?.gameObject;
        if (_bark == null) _bark = transform.Find("bark")?.gameObject;
        if (_suspect != null)
        {
            _suspectAnimator = _suspect.GetComponent<Animator>();
            _suspect.SetActive(false);
        }
        if (_bark != null)
        {
            _barkAnimator = _bark.GetComponent<Animator>();
            _bark.SetActive(false);
        }
        ApplyConfig();
    }

    /// <summary>从当前 config 刷新索敌与识破参数；若 config 为空则使用默认/已有值。</summary>
    private void ApplyConfig()
    {
        if (config == null || _monster == null) return;
        string id = _monster.GetId();
        _detectionRange = config.GetDetectionRange(id);
        _approachDistance = config.GetApproachDistance(id);
        _moveSpeed = config.GetMoveSpeed(id);
        _detectionFillRatePerSecond = config.GetDetectionFillRatePerSecond(id);
        _detectionMaxValue = config.GetDetectionMaxValue(id);
        _sameTypeThreshold = config.GetSameTypeThreshold(id);
        _moveSfxList = config.GetMoveSfxList(id);
        _moveSfxInterval = config.GetMoveSfxInterval(id);
        _discoverSfx = config.GetDiscoverSfx(id);
        _spottedSfx = config.GetSpottedSfx(id);
    }

    /// <summary>玩家与怪物拓扑匹配度是否 >= 同类阈值（视作同类则不索敌、不累积识破）。</summary>
    private bool IsPlayerSameType()
    {
        // var playerMask = _playerMaskInfoProvider?.GetMaskInfo();
        // if (playerMask == null) 
        // {
        //     Debug.LogWarning($"[MonsterAI] {gameObject.name} 未找到玩家，索敌与移动将不生效。请为玩家 GameObject 设置 IMaskInfoProvider 组件。");
        //     return false;
        // };
        // float similarity = _monster.JudgeMaskInfo(playerMask);
        // Debug.Log($"[MonsterAI] {gameObject.name} 玩家与怪物拓扑匹配度: {similarity}");
        // return similarity >= _sameTypeThreshold;

        var similarity_topo = God.Instance.Get<MonsterManager>().CheckIsSameKind(_monster, _playerMaskInfoProvider, _sameTypeThreshold);

        var playerColorId = _player != null ? (_player.GetComponent<Player>()?.GetColorId() ?? 1) : 1;
        var monsterColorId = _monster.GetColorId();
        var similarity_color = (playerColorId == monsterColorId);

        bool isSameType = similarity_topo && similarity_color;
        if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家与怪物拓扑匹配: {similarity_topo}, 颜色相同: {similarity_color}, 视作同类: {isSameType}");
        return isSameType;
    }

    /// <summary>运行时注入配置（如由 MonsterManager 生成后调用），便于预制体不绑定 config。</summary>
    public void SetConfig(MonsterConfig newConfig)
    {
        config = newConfig;
        ApplyConfig();
    }

    /// <summary>发现玩家时播放的抖动小动画（DOTween）。</summary>
    private void PlayDiscoverShake()
    {
        emojiConfused.SetActive(true);
        // if (!discoverShakeEnabled || discoverShakeDuration <= 0f || discoverShakeStrength <= 0f) return;
        // transform.DOKill(true);
        // Vector3 strength = new Vector3(discoverShakeStrength, discoverShakeStrength, 0f);
        // transform.DOShakePosition(discoverShakeDuration, strength, 10, 90f, true, false)
        //     .SetTarget(transform)
        //     .SetUpdate(UpdateType.Normal);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(playerTag)) playerTag = "Player";
        var go = GameObject.FindWithTag(playerTag);
        if (go != null)
        {
            _player = go.transform;
            _playerMaskInfoProvider = _player?.GetComponent<IMaskInfoProvider>();
        }
        else
            Debug.LogWarning($"[MonsterAI] {gameObject.name} 未找到 Tag=\"{playerTag}\" 的玩家，索敌与移动将不生效。请为玩家 GameObject 设置 Tag 为 Player。");
        
        // 初始检测一次伪装状态
        CheckPlayerDisguise();
    }

    /// <summary>由 MonsterManager 调用：当玩家装扮变化时重新检测是否伪装成功。</summary>
    public void CheckPlayerDisguise()
    {
        _isSameType = IsPlayerSameType();
    }

    private void Update()
    {
        if (!_monster.IsAlive() || _player == null || config == null) return;

        Vector2 myPos = transform.position;
        Vector2 playerPos = _player.position;
        float distToPlayer = Vector2.Distance(myPos, playerPos);

        // 使用缓存的伪装状态（仅在装扮变化时更新）
        if (_isSameType)
        {
            God.Instance?.Get<GameProcessManager>()?.UnregisterSpotting(_monster);
            _state = State.Idle;
            _currentDetectionValue = 0f;
            _hasTriggeredDetectionFull = false;
            emojiConfused.SetActive(false);
            HideSuspect();
            HideBark();
            return;
        }

        switch (_state)
        {
            case State.Idle:
                if (distToPlayer <= _detectionRange)
                {
                    _state = State.Approaching;
                    God.Instance?.Get<GameProcessManager>()?.RegisterSpotting(_monster);
                    PlayDiscoverShake();
                    FlashSuspect();
                    PlayDiscoverSfx();
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 进入索敌，开始接近玩家 (距离={distToPlayer:F1})");
                }
                break;

            case State.Approaching:
                if (distToPlayer > _detectionRange)
                {
                    _state = State.Disengaged;
                    _currentDetectionValue = 0f;
                    _hasTriggeredDetectionFull = false;
                    God.Instance?.Get<GameProcessManager>()?.UnregisterSpotting(_monster);
                    HideSuspect();
                    HideBark();
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家离开探测范围，停止跟随 (距离={distToPlayer:F1})");
                    break;
                }
                if (distToPlayer <= _approachDistance )
                {
                    _state = State.Observing;
                    FlashSuspect();
                    PlayDiscoverSfx();
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 到达观察距离，开始观察玩家 (距离={distToPlayer:F1})");
                    break;
                }
                Vector2 dir = (playerPos - myPos).normalized;
                transform.position = Vector2.MoveTowards(myPos, playerPos - dir * _approachDistance, _moveSpeed * Time.deltaTime);
                if (_currentDetectionValue < _detectionMaxValue)
                    _currentDetectionValue = Mathf.Min(_currentDetectionValue + _detectionFillRatePerSecond * Time.deltaTime, _detectionMaxValue);
                TryTriggerDetectionFull();
                TryPlayMoveSfx();
                break;

            case State.Observing:
                if (distToPlayer > _detectionRange)
                {
                    _state = State.Disengaged;
                    _currentDetectionValue = 0f;
                    _hasTriggeredDetectionFull = false;
                    God.Instance?.Get<GameProcessManager>()?.UnregisterSpotting(_monster);
                    HideSuspect();
                    HideBark();
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家离开探测范围，停止跟随 (距离={distToPlayer:F1})");
                    break;
                }
                FacePlayer(myPos, playerPos);
                if (_currentDetectionValue < _detectionMaxValue)
                    _currentDetectionValue = Mathf.Min(_currentDetectionValue + _detectionFillRatePerSecond * Time.deltaTime, _detectionMaxValue);
                TryTriggerDetectionFull();
                break;

            case State.Disengaged:
                if (distToPlayer <= _detectionRange)
                {
                    _state = State.Approaching;
                    God.Instance?.Get<GameProcessManager>()?.RegisterSpotting(_monster);
                    PlayDiscoverShake();
                    FlashSuspect();
                    PlayDiscoverSfx();
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家再次进入探测范围，重新开始跟随 (距离={distToPlayer:F1})");
                }
                break;
        }
    }

    /// <summary>发现玩家时播放音效。</summary>
    private void PlayDiscoverSfx()
    {
        if (_discoverSfx != null)
            God.Instance?.Get<AudioManager>()?.PlaySfx(_discoverSfx);
    }

    /// <summary>识破玩家时播放音效。</summary>
    private void PlaySpottedSfx()
    {
        if (_spottedSfx != null)
            God.Instance?.Get<AudioManager>()?.PlaySfx(_spottedSfx);
    }

    /// <summary>移动时按间隔随机播放 move 音效（循环）。</summary>
    private void TryPlayMoveSfx()
    {
        if (_moveSfxList == null || _moveSfxList.Length == 0 || _moveSfxInterval <= 0f) return;
        _moveSfxCooldown -= Time.deltaTime;
        if (_moveSfxCooldown <= 0f)
        {
            _moveSfxCooldown = _moveSfxInterval;
            var clip = _moveSfxList[Random.Range(0, _moveSfxList.Length)];
            if (clip != null)
                God.Instance?.Get<AudioManager>()?.PlaySfx(clip);
        }
    }

    /// <summary>识破值满时：进入暴露状态、增加玩家暴露值；隐藏 suspect、显示 bark、播识破音效。识破值不立刻清零，等怪物死亡或丢失视野后清零。</summary>
    private void TryTriggerDetectionFull()
    {
        if (_currentDetectionValue < _detectionMaxValue) return;
        _currentDetectionValue = _detectionMaxValue;
        if (_hasTriggeredDetectionFull) return;
        _hasTriggeredDetectionFull = true;
        HideSuspect();
        FlashBark();
        PlaySpottedSfx();
        var process = God.Instance?.Get<GameProcessManager>();
        if (process != null)
            process.EnterSpotted();
        var exposure = PlayerExposure.Instance;
        if (exposure != null)
            exposure.AddExposureForMonsterType(_monster.GetId(), 1f);
    }

    /// <summary>识破值是否已满（满时玩家不可暗杀此怪）。</summary>
    public bool IsDetectionFull() => _currentDetectionValue >= _detectionMaxValue;

    /// <summary>显示 suspect（警觉符号）：播放 in 动画后切到 loop 循环；若已显示则不重新播放。</summary>
    private void ShowSuspect()
    {
        if (_suspect == null) return;
        if (_suspect.activeSelf) return; // 已显示则不重新播放
        if (_suspectCoroutine != null) StopCoroutine(_suspectCoroutine);
        _suspectCoroutine = StartCoroutine(ShowSymbolRoutine(_suspect, _suspectAnimator, _symbolInState, _symbolLoopState));
    }

    /// <summary>隐藏 suspect（警觉符号）。</summary>
    private void HideSuspect()
    {
        if (_suspect == null) return;
        if (_suspectCoroutine != null)
        {
            StopCoroutine(_suspectCoroutine);
            _suspectCoroutine = null;
        }
        _suspect.SetActive(false);
    }

    /// <summary>显示 bark（发现符号）：播放 in 动画后切到 loop 循环；若已显示则不重新播放。</summary>
    private void ShowBark()
    {
        if (_bark == null) return;
        if (_bark.activeSelf) return; // 已显示则不重新播放
        if (_barkCoroutine != null) StopCoroutine(_barkCoroutine);
        _barkCoroutine = StartCoroutine(ShowSymbolRoutine(_bark, _barkAnimator, _barkInState, _barkLoopState));
    }

    /// <summary>隐藏 bark（发现符号）。</summary>
    private void HideBark()
    {
        if (_bark == null) return;
        if (_barkCoroutine != null)
        {
            StopCoroutine(_barkCoroutine);
            _barkCoroutine = null;
        }
        _bark.SetActive(false);
    }

    /// <summary>通用符号显示协程：激活物体，播放 in 动画，播完后切到 loop。</summary>
    private IEnumerator ShowSymbolRoutine(GameObject obj, Animator animator, string inState, string loopState)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        if (animator != null && !string.IsNullOrEmpty(inState))
        {
            animator.Play(inState, 0, 0f);
            yield return null;
        }
    }

    // 保留旧方法名以兼容现有调用
    private void FlashSuspect() => ShowSuspect();
    private void FlashBark() => ShowBark();

    private void FacePlayer(Vector2 myPos, Vector2 playerPos)
    {
        // Vector2 dir = playerPos - myPos;
        // if (dir.sqrMagnitude < 0.0001f) return;
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>减少当前识破值，用于隐身、打断观察等后续逻辑。</summary>
    public void ReduceDetectionValue(float amount)
    {
        _currentDetectionValue = Mathf.Max(0f, _currentDetectionValue - amount);
    }

    /// <summary>将识破值设为 0，并重置满值触发标记。</summary>
    public void ResetDetectionValue()
    {
        _currentDetectionValue = 0f;
        _hasTriggeredDetectionFull = false;
    }

    /// <summary>当前识破值（只读）。</summary>
    public float GetCurrentDetectionValue() => _currentDetectionValue;

    /// <summary>识破值满值（只读）。</summary>
    public float GetDetectionMaxValue() => _detectionMaxValue;

    /// <summary>当前是否正在观察玩家。</summary>
    public bool IsObservingPlayer() => _state == State.Observing;

    /// <summary>当前状态（用于调试）。</summary>
    public string GetStateName() => _state.ToString();

    /// <summary>设置是否在状态切换时输出调试日志（测试索敌与移动时使用）。</summary>
    public void SetDebugLog(bool on) => debugLog = on;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var mb = _monster != null ? _monster : GetComponent<MonsterBase>();
        if (config == null || mb == null) return;
        string id = mb.GetId();
        if (string.IsNullOrEmpty(id)) return;
        float range = config.GetDetectionRange(id);
        float approach = config.GetApproachDistance(id);
        if (range <= 0f) return;
        Vector3 center = transform.position;
        center.z = 0f;
        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(center, range);
        UnityEditor.Handles.Label(center + Vector3.up * (range + 0.3f), $"检测范围 {range:F0}");
        if (_player != null)
        {
            Vector3 playerCenter = _player.position;
            playerCenter.z = 0f;
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(playerCenter, approach);
            UnityEditor.Handles.Label(playerCenter + Vector3.up * (approach + 0.3f), $"观察距离 {approach:F0}");
        }
        else if (approach > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(center, approach);
            UnityEditor.Handles.Label(center + Vector3.down * (approach + 0.3f), $"观察距离 {approach:F0}");
        }
    }
#endif
}
