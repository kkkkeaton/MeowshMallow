using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包：挂在背包节点上，脚本创建时在子节点 grid（带 Grid Layout Group）下生成指定数量的格子；
/// 可通过 AddPartItem 在第一个空 slot 里生成 PartItem 并设置其 UIDraggable 的 Composable。
/// </summary>
public class Backpack : MonoBehaviour
{
    /// <summary>格子预制体，在 Backpack 中指定。</summary>
    [SerializeField] private GameObject _slotPrefab;

    /// <summary>PartItem 预制体，AddPartItem 时在空 slot 里实例化。</summary>
    [SerializeField] private GameObject _partItemPrefab;

    /// <summary>包含 Grid Layout Group 的节点，不指定则用子节点 "grid"。</summary>
    [SerializeField] private Transform _grid;

    /// <summary>格子数量，可在编辑器里配置。</summary>
    [SerializeField] private int _slotCount = 5;

    private void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);

        Transform grid = _grid != null ? _grid : transform.Find("grid");
        if (grid == null || _slotPrefab == null) return;

        for (int i = 0; i < _slotCount; i++)
            Instantiate(_slotPrefab, grid);
    }

    /// <summary>在第一个空 slot 里实例化 PartItem，并设置其 UIDraggable 的 Composable；PartItem 的 Image 使用 Composable.prefab 子物体的 Sprite。无空 slot 或 prefab 未设置时返回 false。</summary>
    public bool AddPartItem(Composable composable)
    {
        Transform grid = _grid != null ? _grid : transform.Find("grid");
        if (grid == null || _partItemPrefab == null || composable == null) return false;

        for (int i = 0; i < grid.childCount; i++)
        {
            Transform slot = grid.GetChild(i);
            if (slot.childCount != 0) continue;

            GameObject item = Instantiate(_partItemPrefab, slot);
            UIDraggable draggable = item.GetComponent<UIDraggable>();
            if (draggable != null)
                draggable.SetComposable(composable);

            if (composable.prefab != null)
            {
                SpriteRenderer sr = composable.prefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null && sr.sprite != null)
                {
                    Image img = item.GetComponent<Image>() ?? item.GetComponentInChildren<Image>(true);
                    if (img != null)
                        img.sprite = sr.sprite;
                }
            }

            return true;
        }

        return false;
    }
}
