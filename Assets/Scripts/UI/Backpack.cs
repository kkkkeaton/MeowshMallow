using UnityEngine;

/// <summary>
/// 背包：挂在背包节点上，脚本创建时在子节点 grid（带 Grid Layout Group）下生成指定数量的格子。
/// </summary>
public class Backpack : MonoBehaviour
{
    /// <summary>格子预制体，在 Backpack 中指定。</summary>
    [SerializeField] private GameObject _slotPrefab;

    /// <summary>包含 Grid Layout Group 的节点，不指定则用子节点 "grid"。</summary>
    [SerializeField] private Transform _grid;

    /// <summary>格子数量，可在编辑器里配置。</summary>
    [SerializeField] private int _slotCount = 5;

    private void Awake()
    {
        Transform grid = _grid != null ? _grid : transform.Find("grid");
        if (grid == null || _slotPrefab == null) return;

        for (int i = 0; i < _slotCount; i++)
            Instantiate(_slotPrefab, grid);
    }
}
