using UnityEngine;

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

    [Header("调试")]
    [Tooltip("勾选后状态切换时在 Console 输出，便于验证索敌与移动")]
    [SerializeField] private bool debugLog;

    private MonsterBase _monster;
    private Transform _player;
    private float _detectionRange;
    private float _approachDistance;
    private float _moveSpeed;
    private float _detectionFillRatePerSecond;
    private float _detectionMaxValue;

    /// <summary>当前识破值（0 ~ 满值）。</summary>
    private float _currentDetectionValue;

    /// <summary>Idle=待机 Approaching=接近中 Observing=观察中 Disengaged=玩家已离开探测范围，待玩家再次进入后重新跟随</summary>
    private enum State { Idle, Approaching, Observing, Disengaged }
    private State _state = State.Idle;

    private void Awake()
    {
        _monster = GetComponent<MonsterBase>();
        if (_monster == null) return;
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
    }

    /// <summary>运行时注入配置（如由 MonsterManager 生成后调用），便于预制体不绑定 config。</summary>
    public void SetConfig(MonsterConfig newConfig)
    {
        config = newConfig;
        ApplyConfig();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(playerTag)) playerTag = "Player";
        var go = GameObject.FindWithTag(playerTag);
        if (go != null)
            _player = go.transform;
        else
            Debug.LogWarning($"[MonsterAI] {gameObject.name} 未找到 Tag=\"{playerTag}\" 的玩家，索敌与移动将不生效。请为玩家 GameObject 设置 Tag 为 Player。");
    }

    private void Update()
    {
        if (!_monster.IsAlive() || _player == null || config == null) return;

        Vector2 myPos = transform.position;
        Vector2 playerPos = _player.position;
        float distToPlayer = Vector2.Distance(myPos, playerPos);

        switch (_state)
        {
            case State.Idle:
                if (distToPlayer <= _detectionRange)
                {
                    _state = State.Approaching;
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 进入索敌，开始接近玩家 (距离={distToPlayer:F1})");
                }
                break;

            case State.Approaching:
                if (distToPlayer > _detectionRange)
                {
                    _state = State.Disengaged;
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家离开探测范围，停止跟随 (距离={distToPlayer:F1})");
                    break;
                }
                if (distToPlayer <= _approachDistance)
                {
                    _state = State.Observing;
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 到达观察距离，开始观察玩家 (距离={distToPlayer:F1})");
                    break;
                }
                Vector2 dir = (playerPos - myPos).normalized;
                transform.position = Vector2.MoveTowards(myPos, playerPos - dir * _approachDistance, _moveSpeed * Time.deltaTime);
                break;

            case State.Observing:
                if (distToPlayer > _detectionRange)
                {
                    _state = State.Disengaged;
                    _currentDetectionValue = 0f;
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家离开探测范围，停止跟随 (距离={distToPlayer:F1})");
                    break;
                }
                FacePlayer(myPos, playerPos);
                _currentDetectionValue += _detectionFillRatePerSecond * Time.deltaTime;
                if (_currentDetectionValue >= _detectionMaxValue)
                {
                    _currentDetectionValue = _detectionMaxValue;
                    var exposure = PlayerExposure.Instance;
                    if (exposure != null)
                        exposure.AddExposureForMonsterType(_monster.GetId(), 1f); // 暴露值增加量可后续改为配置
                    _currentDetectionValue = 0f; // 满后重置，继续观察可再次累积
                }
                break;

            case State.Disengaged:
                if (distToPlayer <= _detectionRange)
                {
                    _state = State.Approaching;
                    if (debugLog) Debug.Log($"[MonsterAI] {gameObject.name} 玩家再次进入探测范围，重新开始跟随 (距离={distToPlayer:F1})");
                }
                break;
        }
    }

    private void FacePlayer(Vector2 myPos, Vector2 playerPos)
    {
        Vector2 dir = playerPos - myPos;
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>减少当前识破值，用于隐身、打断观察等后续逻辑。</summary>
    public void ReduceDetectionValue(float amount)
    {
        _currentDetectionValue = Mathf.Max(0f, _currentDetectionValue - amount);
    }

    /// <summary>将识破值设为 0。</summary>
    public void ResetDetectionValue()
    {
        _currentDetectionValue = 0f;
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
