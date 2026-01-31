using UnityEngine;

/// <summary>
/// 游戏进程配置资源：暴露值上限/下限、每秒自然增长量、玩家预制体等，供 GameProcessManager 使用。
/// 将本资产拖到 GameProcessManager 的 Config 槽位即可。
/// </summary>
[CreateAssetMenu(fileName = "GameProcessConfig", menuName = "Scriptable Objects/GameProcessConfig")]
public class GameProcessConfig : ScriptableObject
{
    [Header("暴露值基础")]
    [Tooltip("暴露值上限，达到此值即游戏结束")]
    [SerializeField] private float maxExposure = 100f;

    [Tooltip("暴露值下限，不会被减到比这更小")]
    [SerializeField] private float minExposure = 0f;

    [Header("玩家状态：增速")]
    [Tooltip("正常状态下的暴露值每秒增加量")]
    [SerializeField] private float normalGrowthPerSecond = 2f;

    [Tooltip("被识破状态下的暴露值每秒增加量（通常更高）")]
    [SerializeField] private float spottedGrowthPerSecond = 6f;

    [Header("玩家状态：伪装成功")]
    [Tooltip("伪装成功时立即减少的暴露值")]
    [SerializeField] private float disguiseSuccessExposureDecrease = 20f;

    [Tooltip("伪装成功后多少秒内不再增加暴露值")]
    [SerializeField] private float disguiseSuccessImmunityDuration = 5f;

    [Header("玩家")]
    [Tooltip("游戏开始时实例化的玩家预制体")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("玩家出生位置（世界坐标）")]
    [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;

    [Header("地图")]
    [Tooltip("游戏开始时在 (0,0,0) 实例化的地图预制体")]
    [SerializeField] private GameObject mapPrefab;

    /// <summary>暴露值上限。</summary>
    public float MaxExposure => maxExposure;

    /// <summary>暴露值下限。</summary>
    public float MinExposure => minExposure;

    /// <summary>正常状态默认增速（每秒增加量）。</summary>
    public float NormalGrowthPerSecond => normalGrowthPerSecond;

    /// <summary>被识破时增速（每秒增加量）。</summary>
    public float SpottedGrowthPerSecond => spottedGrowthPerSecond;

    /// <summary>伪装成功时立即减少的暴露值。</summary>
    public float DisguiseSuccessExposureDecrease => disguiseSuccessExposureDecrease;

    /// <summary>伪装成功后免疫增加暴露值的时长（秒）。</summary>
    public float DisguiseSuccessImmunityDuration => disguiseSuccessImmunityDuration;

    /// <summary>游戏开始时实例化的玩家预制体。</summary>
    public GameObject PlayerPrefab => playerPrefab;

    /// <summary>玩家出生位置（世界坐标）。</summary>
    public Vector3 PlayerSpawnPosition => playerSpawnPosition;

    /// <summary>游戏开始时在 (0,0,0) 实例化的地图预制体。</summary>
    public GameObject MapPrefab => mapPrefab;
}
