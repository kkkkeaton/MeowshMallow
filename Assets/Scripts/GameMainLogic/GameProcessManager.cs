using UnityEngine;
using System;

/// <summary>玩家暴露相关状态：正常、被识破、伪装成功（免疫增加中）。</summary>
public enum PlayerExposureState
{
    /// <summary>正常：按默认增速增加暴露值。</summary>
    Normal,

    /// <summary>被识破：按更高增速增加暴露值。</summary>
    Spotted,

    /// <summary>伪装成功：一段时间内不增加暴露值，结束后回到正常。</summary>
    DisguiseImmunity
}

/// <summary>
/// 游戏进程管理器：控制玩法逻辑意义上的「开始」与「结束」。
/// 维护「暴露值」：随游戏时间缓慢增长，达到满值时自动触发游戏结束。
/// 可通过 God 服务定位获取：God.Instance?.GetService&lt;GameProcessManager&gt;()
/// </summary>
public class GameProcessManager : MonoBehaviour
{
    // ---------- 配置（Inspector 手动绑定） ----------

    /// <summary>游戏进程配置，在 Inspector 中拖入 GameProcessConfig.asset。</summary>
    [SerializeField] private GameProcessConfig _config;

    // ---------- 运行时状态（由上述 config 初始化） ----------

    /// <summary>暴露值上限（来自 config 或默认 100）。</summary>
    private float _maxExposure;

    /// <summary>暴露值下限（来自 config 或默认 0）。</summary>
    private float _minExposure;

    /// <summary>当前暴露值每秒变化量（由玩家状态或手动设置决定）。</summary>
    private float _exposureGrowthPerSecond;

    /// <summary>正常状态默认增速，用于 ResetExposureGrowthRateToConfig / 状态切换。</summary>
    private float _normalGrowthPerSecond;

    /// <summary>被识破时增速。</summary>
    private float _spottedGrowthPerSecond;

    /// <summary>伪装成功时立即减少的暴露值。</summary>
    private float _disguiseSuccessExposureDecrease;

    /// <summary>伪装成功后免疫增加的时长（秒）。</summary>
    private float _disguiseSuccessImmunityDuration;

    /// <summary>当前玩家暴露状态。</summary>
    private PlayerExposureState _playerState;

    /// <summary>伪装免疫剩余时间（秒），仅在 DisguiseImmunity 状态下递减。</summary>
    private float _disguiseImmunityRemaining;

    // ---------- 其它运行时状态 ----------

    /// <summary>当前暴露值。只读，修改请通过 AddExposure / SetExposure / ResetExposure。</summary>
    private float _currentExposure;

    /// <summary>玩法是否已开始（未开始 / 进行中 / 已结束 用状态区分）。</summary>
    private bool _isPlaying;

    /// <summary>玩法是否已结束（一旦结束本局不再更新暴露值）。</summary>
    private bool _isEnded;

    /// <summary>玩家预制体（来自 config），游戏开始时实例化。</summary>
    private GameObject _playerPrefab;

    /// <summary>玩家出生位置（来自 config）。</summary>
    private Vector3 _playerSpawnPosition;

    /// <summary>当前局实例化的玩家物体，StartGame 时创建，可供其他脚本通过 Player 获取。</summary>
    private GameObject _playerInstance;

    // ---------- 事件（供 UI、音效等订阅） ----------

    /// <summary>玩法开始时触发。</summary>
    public event Action OnGameStart;

    /// <summary>玩法结束时触发。</summary>
    public event Action OnGameEnd;

    /// <summary>暴露值发生变化时触发，参数为 (当前值, 归一化 0~1)。</summary>
    public event Action<float, float> OnExposureChanged;

    // ---------- 公开属性 ----------

    /// <summary>暴露值上限。</summary>
    public float MaxExposure => _maxExposure;

    /// <summary>当前暴露值。</summary>
    public float CurrentExposure => _currentExposure;

    /// <summary>暴露值归一化到 [0, 1]，满为 1。</summary>
    public float ExposureNormalized => _maxExposure > 0 ? Mathf.Clamp01(_currentExposure / _maxExposure) : 0f;

    /// <summary>玩法是否正在进行中（已开始且未结束）。</summary>
    public bool IsPlaying => _isPlaying && !_isEnded;

    /// <summary>玩法是否已结束。</summary>
    public bool IsEnded => _isEnded;

    /// <summary>当前玩家暴露状态。</summary>
    public PlayerExposureState CurrentPlayerState => _playerState;

    private void Awake()
    {
        if (_config != null)
        {
            _maxExposure = _config.MaxExposure;
            _minExposure = _config.MinExposure;
            _normalGrowthPerSecond = _config.NormalGrowthPerSecond;
            _spottedGrowthPerSecond = _config.SpottedGrowthPerSecond;
            _disguiseSuccessExposureDecrease = _config.DisguiseSuccessExposureDecrease;
            _disguiseSuccessImmunityDuration = _config.DisguiseSuccessImmunityDuration;
            _exposureGrowthPerSecond = _normalGrowthPerSecond;
            _playerState = PlayerExposureState.Normal;
            _playerPrefab = _config.PlayerPrefab;
            _playerSpawnPosition = _config.PlayerSpawnPosition;
        }
        else
        {
            _maxExposure = 100f;
            _minExposure = 0f;
            _normalGrowthPerSecond = 2f;
            _spottedGrowthPerSecond = 6f;
            _disguiseSuccessExposureDecrease = 20f;
            _disguiseSuccessImmunityDuration = 5f;
            _exposureGrowthPerSecond = _normalGrowthPerSecond;
            _playerState = PlayerExposureState.Normal;
            _playerPrefab = null;
            _playerSpawnPosition = Vector3.zero;
        }
        _disguiseImmunityRemaining = 0f;

        // 向 God 注册，方便全局获取
        var god = God.Instance;
        if (god != null)
            god.Add(this as GameProcessManager);
    }

    private void OnDestroy()
    {
        // 注：God 未提供 Unregister，销毁后由调用方避免再使用本服务即可
    }

    private void Update()
    {
        if (!IsPlaying) return;

        // 伪装免疫倒计时：结束后切回正常
        if (_playerState == PlayerExposureState.DisguiseImmunity)
        {
            _disguiseImmunityRemaining -= Time.deltaTime;
            if (_disguiseImmunityRemaining <= 0f)
                EnterNormal();
            return; // 免疫期间不增加暴露值
        }

        // 按当前增速变化暴露值（正=增加，负=减少）
        float delta = _exposureGrowthPerSecond * Time.deltaTime;
        if (delta >= 0f)
            AddExposure(delta);
        else
            SubtractExposure(-delta);
    }

    // ---------- 玩法控制（公开） ----------

    /// <summary>
    /// 开始玩法。将状态设为进行中，重置暴露值，并触发 OnGameStart。
    /// 若已在进行中或已结束，需先调用 ResetGame 再 StartGame（或本方法内可自动重置，见实现）。
    /// </summary>
    public void StartGame()
    {
        Debug.Log("StartGame");
        if (_isEnded || _isPlaying)
            ResetGame();

        _isPlaying = true;
        _isEnded = false;
        _playerState = PlayerExposureState.Normal;
        _exposureGrowthPerSecond = _normalGrowthPerSecond;
        _disguiseImmunityRemaining = 0f;
        ResetExposure();

        if (_playerPrefab != null)
        {
            if (_playerInstance != null)
            {
                if (God.Instance != null)
                    God.Instance.SetPlayer(null);
                Destroy(_playerInstance);
            }
            _playerInstance = Instantiate(_playerPrefab, _playerSpawnPosition, Quaternion.identity);
            if (God.Instance != null)
                God.Instance.SetPlayer(_playerInstance);
            if (CameraController.Instance != null)
                CameraController.Instance.BindTarget(_playerInstance.transform);
        }

        if (_config != null && _config.MapPrefab != null)
            Instantiate(_config.MapPrefab, Vector3.zero, Quaternion.identity);

        OnGameStart?.Invoke();
    }

    /// <summary>
    /// 结束玩法。停止暴露值增长，触发 OnGameEnd。
    /// 可由外部调用（如玩家放弃），或由本管理器在暴露值满时自动调用。
    /// </summary>
    public void EndGame()
    {
        if (_isEnded) return;

        _isEnded = true;
        _isPlaying = false;
        OnGameEnd?.Invoke();
    }

    /// <summary>
    /// 重置玩法状态（未开始、未结束），并重置暴露值。
    /// 可用于「再来一局」前调用，再配合 StartGame 开始新一局。
    /// </summary>
    public void ResetGame()
    {
        _isPlaying = false;
        _isEnded = false;
        _playerState = PlayerExposureState.Normal;
        _disguiseImmunityRemaining = 0f;
        ResetExposure();
    }

    // ---------- 玩家状态切换（公开） ----------

    /// <summary>
    /// 切换到正常状态：使用默认增速增加暴露值。
    /// </summary>
    public void EnterNormal()
    {
        _playerState = PlayerExposureState.Normal;
        _exposureGrowthPerSecond = _normalGrowthPerSecond;
        _disguiseImmunityRemaining = 0f;
    }

    /// <summary>
    /// 切换到被识破状态：使用更高增速增加暴露值。
    /// </summary>
    public void EnterSpotted()
    {
        _playerState = PlayerExposureState.Spotted;
        _exposureGrowthPerSecond = _spottedGrowthPerSecond;
        _disguiseImmunityRemaining = 0f;
    }

    /// <summary>
    /// 切换到伪装成功：立即减少一定暴露值，并在配置的时长内不再增加暴露值，结束后自动回到正常。
    /// </summary>
    public void EnterDisguiseSuccess()
    {
        SubtractExposure(_disguiseSuccessExposureDecrease);
        _playerState = PlayerExposureState.DisguiseImmunity;
        _exposureGrowthPerSecond = 0f;
        _disguiseImmunityRemaining = _disguiseSuccessImmunityDuration;
    }

    /// <summary>获取当前玩家暴露状态。</summary>
    public PlayerExposureState GetPlayerState() => _playerState;

    /// <summary>是否处于伪装免疫中（伪装成功后的不增加暴露值时段）。</summary>
    public bool IsInDisguiseImmunity() => _playerState == PlayerExposureState.DisguiseImmunity;

    /// <summary>伪装免疫剩余时间（秒），非免疫状态返回 0。</summary>
    public float GetDisguiseImmunityRemaining() => _playerState == PlayerExposureState.DisguiseImmunity ? _disguiseImmunityRemaining : 0f;

    // ---------- 暴露值：增速（公开，支持负值=逐渐减少） ----------

    /// <summary>获取当前暴露值每秒变化量。正=随时间增加，负=随时间减少。</summary>
    public float GetExposureGrowthRate() => _exposureGrowthPerSecond;

    /// <summary>
    /// 设置暴露值每秒变化量。正数=随时间增加，负数=随时间减少（可用于安全区、道具等机制）。
    /// </summary>
    public void SetExposureGrowthRate(float ratePerSecond)
    {
        _exposureGrowthPerSecond = ratePerSecond;
    }

    /// <summary>
    /// 在当前增速上叠加一个变化量。用于临时加减速（如进入安全区 -2、拾取道具 +1）。
    /// </summary>
    /// <param name="deltaRate">叠加的每秒变化量，可正可负。</param>
    public void AddExposureGrowthRateModifier(float deltaRate)
    {
        _exposureGrowthPerSecond += deltaRate;
    }

    /// <summary>
    /// 将暴露值增速恢复为配置中的正常状态默认值（并切回正常状态）。
    /// </summary>
    public void ResetExposureGrowthRateToConfig()
    {
        EnterNormal();
    }

    // ---------- 暴露值：查询（公开） ----------

    /// <summary>获取当前暴露值。</summary>
    public float GetExposure() => _currentExposure;

    /// <summary>获取暴露值归一化 [0, 1]。</summary>
    public float GetExposureNormalized() => ExposureNormalized;

    /// <summary>获取暴露值上限。</summary>
    public float GetMaxExposure() => _maxExposure;

    /// <summary>暴露值是否已满（>= maxExposure）。</summary>
    public bool IsExposureFull() => _currentExposure >= _maxExposure;

    // ---------- 暴露值：修改与重置（公开） ----------

    /// <summary>
    /// 增加暴露值。若达到或超过上限则自动结束游戏并触发 OnGameEnd。
    /// </summary>
    /// <param name="amount">增加量，建议非负。</param>
    /// <returns>实际增加的量（可能因上限截断）。</returns>
    public float AddExposure(float amount)
    {
        if (amount <= 0f) return 0f;
        float oldVal = _currentExposure;
        _currentExposure = Mathf.Min(_currentExposure + amount, _maxExposure);
        float actual = _currentExposure - oldVal;
        NotifyExposureChanged();
        if (IsExposureFull())
            EndGame();
        return actual;
    }

    /// <summary>
    /// 减少暴露值。不会低于 minExposure。
    /// </summary>
    /// <param name="amount">减少量，建议非负。</param>
    /// <returns>实际减少的量。</returns>
    public float SubtractExposure(float amount)
    {
        if (amount <= 0f) return 0f;
        float oldVal = _currentExposure;
        _currentExposure = Mathf.Max(_currentExposure - amount, _minExposure);
        float actual = oldVal - _currentExposure;
        if (actual > 0f)
            NotifyExposureChanged();
        return actual;
    }

    /// <summary>
    /// 直接设置当前暴露值。会被夹在 [minExposure, maxExposure]。
    /// 若设置为 >= maxExposure 且正在玩法中，会触发游戏结束。
    /// </summary>
    public void SetExposure(float value)
    {
        float oldVal = _currentExposure;
        _currentExposure = Mathf.Clamp(value, _minExposure, _maxExposure);
        if (Mathf.Approximately(_currentExposure, oldVal)) return;
        NotifyExposureChanged();
        if (IsPlaying && IsExposureFull())
            EndGame();
    }

    /// <summary>
    /// 将暴露值重置为 minExposure（默认 0）。
    /// </summary>
    public void ResetExposure()
    {
        if (Mathf.Approximately(_currentExposure, _minExposure)) return;
        _currentExposure = _minExposure;
        NotifyExposureChanged();
    }

    /// <summary>
    /// 设置暴露值上限。若当前值已超过新上限，会被夹到新上限并可能触发游戏结束。
    /// </summary>
    public void SetMaxExposure(float max)
    {
        if (max <= 0f) return;
        _maxExposure = max;
        if (_currentExposure > _maxExposure)
        {
            _currentExposure = _maxExposure;
            NotifyExposureChanged();
            if (IsPlaying)
                EndGame();
        }
    }

    /// <summary>
    /// 设置暴露值下限（不会被减到比这更小）。
    /// </summary>
    public void SetMinExposure(float min)
    {
        _minExposure = Mathf.Min(min, _maxExposure);
        if (_currentExposure < _minExposure)
        {
            _currentExposure = _minExposure;
            NotifyExposureChanged();
        }
    }

    private void NotifyExposureChanged()
    {
        OnExposureChanged?.Invoke(_currentExposure, ExposureNormalized);
    }
}
