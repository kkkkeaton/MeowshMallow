using UnityEngine;

/// <summary>
/// 可被捡起的物体：挂在场景物体上。由 PlayerMovement 在按下交互键时检测范围内是否存在本物体，存在则调用 DoPickup 删除本物体并将 TopoComponent 交给 Backpack。
/// </summary>
public class PickableItem : MonoBehaviour
{
    [Header("拓扑组件（捡起后传给 Backpack 的 PartItem）")]
    [SerializeField] private TopoComponent _topoComponent;

    [Header("捡起判定距离（世界单位）")]
    [SerializeField] private float _pickupRadius = 2f;

    private const string MapItemSortingLayer = "MapItem";

    /// <summary>捡起判定距离，供 PlayerMovement 检测范围内可捡物体时使用。</summary>
    public float PickupRadius => _pickupRadius;

    private void Awake()
    {
        TrySpawnTopoVisual();
    }

    /// <summary>根据当前 _topoComponent 生成子物体显示；无配置则跳过。</summary>
    private void TrySpawnTopoVisual()
    {
        if (_topoComponent == null || _topoComponent.prefab == null) return;
        GameObject child = Instantiate(_topoComponent.prefab, transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        foreach (SpriteRenderer sr in child.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingLayerName = MapItemSortingLayer;
    }

    /// <summary>运行时设置 TopoComponent 并生成显示（用于掉落等动态生成的可捡物体）。</summary>
    public void InitWithTopo(TopoComponent topo)
    {
        _topoComponent = topo;
        TrySpawnTopoVisual();
    }

    /// <summary>执行捡起：将 TopoComponent 交给 Backpack 并销毁本物体。由 PlayerMovement 在交互键按下且本物体在范围内时调用。</summary>
    public void DoPickup()
    {
        Debug.Log("捡起");

        if (_topoComponent == null) return;
        TopoComponent topo = _topoComponent;

        Backpack backpack = God.Instance?.Get<Backpack>();
        if (backpack != null)
            backpack.AddPartItem(topo);

        Destroy(gameObject);
    }

    /// <summary>获取/设置本物体对应的 TopoComponent（代码用）。</summary>
    public TopoComponent TopoComponent
    {
        get => _topoComponent;
        set => _topoComponent = value;
    }
}
