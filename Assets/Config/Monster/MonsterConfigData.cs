using UnityEngine;

/// <summary>
/// 单只怪物的配置数据：单独存为一个 ScriptableObject 资产，由 MonsterConfig 的列表引用。
/// 右键 Create > Scriptable Objects > MonsterConfigData 创建，填写后拖入 MonsterConfig 的列表。
/// </summary>
[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterConfigData")]
public class MonsterConfigData : ScriptableObject
{
    [Tooltip("怪物类型 ID，与 MonsterConfig 列表内唯一")]
    [SerializeField] private string id;

    [Tooltip("2D 怪物预制体（需挂 MonsterBase）")]
    [SerializeField] private GameObject prefab;

    [Tooltip("最大血量")]
    [SerializeField] private float maxHp = 100f;

    [Tooltip("移动速度")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("索敌与移动")]
    [Tooltip("发现玩家时的检测距离")]
    [SerializeField] private float detectionRange = 8f;

    [Tooltip("发现玩家后向玩家移动并停下的目标距离（与玩家的距离）")]
    [SerializeField] private float approachDistance = 3f;

    [Header("识破值")]
    [Tooltip("面向玩家观察时，识破值每秒增加量")]
    [SerializeField] private float detectionFillRatePerSecond = 20f;

    [Tooltip("识破值满值，达到后会增加玩家在该类型怪物中的暴露值")]
    [SerializeField] private float detectionMaxValue = 100f;

    [Header("同类判定")]
    [Tooltip("怪物将玩家视作同类的最低阈值，0～1。玩家与怪物拓扑匹配度 >= 此值时视为同类")]
    [Range(0f, 1f)]
    [SerializeField] private float sameTypeThreshold = 0f;

    [Header("怪物拓扑")]
    [Tooltip("怪物的拓扑配置，配置规则问虾丸")]
    [SerializeField] private string monsterTopoConfig = "";

    [Header("掉落")]
    [Tooltip("暗杀/死亡时掉落的 Composable，配置其 topoComponent 作为 PickableItem 的拾取内容；空则不掉落")]
    [SerializeField] private Composable dropComposable;

    /// <summary>怪物类型 ID。</summary>
    public string Id => id;

    /// <summary>怪物预制体。</summary>
    public GameObject Prefab => prefab;

    /// <summary>最大血量。</summary>
    public float MaxHp => maxHp;

    /// <summary>移动速度。</summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>索敌检测距离。</summary>
    public float DetectionRange => detectionRange;

    /// <summary>靠近玩家的目标距离。</summary>
    public float ApproachDistance => approachDistance;

    /// <summary>识破值每秒增加量。</summary>
    public float DetectionFillRatePerSecond => detectionFillRatePerSecond;

    /// <summary>识破值满值。</summary>
    public float DetectionMaxValue => detectionMaxValue;

    /// <summary>将玩家视作同类的最低阈值，0～1。</summary>
    public float SameTypeThreshold => sameTypeThreshold;

    /// <summary>怪物拓扑配置</summary>
    public string MonsterTopoConfig => monsterTopoConfig;

    /// <summary>掉落配置（暗杀/死亡时生成 PickableItem 使用其 topoComponent）；空则不掉落。</summary>
    public Composable DropComposable => dropComposable;
}
