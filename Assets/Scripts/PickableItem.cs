using UnityEngine;

/// <summary>
/// 可被捡起的物体：挂在场景物体上。由 PlayerMovement 在按下交互键时检测范围内是否存在本物体，存在则调用 DoPickup 删除本物体并将 Composable 交给 Backpack。
/// </summary>
public class PickableItem : MonoBehaviour
{
    [Header("可组合配置（捡起后传给 Backpack；可空则运行时由 InitWithComposable 注入）")]
    [SerializeField] private Composable _composable;

    [Tooltip("物品类型 ID，用于按类型播放捡起/掉落音效；有 Composable 时从其 itemTypeId 取")]
    [SerializeField] private int _itemTypeId;

    private const string MapItemSortingLayer = "MapItem";

    /// <summary>捡起判定距离，取自 GlobalSetting.PICKUP_RANGE。</summary>
    public float PickupRadius => GlobalSetting.PICKUP_RANGE;

    /// <summary>物品类型 ID，用于音效查找。</summary>
    public int ItemTypeId => _composable != null ? _composable.itemTypeId : _itemTypeId;

    private void Awake()
    {
        TrySpawnVisual();
    }

    /// <summary>根据当前 _composable.prefab 生成子物体显示；无配置则跳过。</summary>
    private void TrySpawnVisual()
    {
        if (_composable == null || _composable.prefab == null) return;

        GameObject child = Instantiate(_composable.prefab, transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        foreach (SpriteRenderer sr in child.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingLayerName = MapItemSortingLayer;
    }

    /// <summary>运行时设置 Composable 并生成显示（用于掉落等动态生成的可捡物体）。itemTypeId 由 composable.itemTypeId 提供。</summary>
    public void InitWithComposable(Composable composable)
    {
        _composable = composable;
        TrySpawnVisual();
    }

    /// <summary>执行捡起：按类型播放捡起音效、将 Composable 交给 Backpack 并销毁本物体。</summary>
    public void DoPickup()
    {
        if (_composable == null) return;

        God.Instance?.Get<AudioManager>()?.PlayPickupSfxForItemType(ItemTypeId);

        Backpack backpack = God.Instance?.Get<Backpack>();
        if (backpack != null && backpack.AddPartItem(_composable))
            Destroy(gameObject);
    }

    /// <summary>获取/设置本物体对应的 Composable（代码用）。</summary>
    public Composable Composable
    {
        get => _composable;
        set => _composable = value;
    }
}
