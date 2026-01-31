using UnityEngine;

/// <summary>
/// 游戏进程配置资源：暴露值上限/下限、每秒自然增长量等，供 GameProcessManager 使用。
/// 资产需放在 Resources/Config/Global/GameProcessConfig，由管理器按路径强制加载，无需拖动赋值。
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
}
