using UnityEngine;

/// <summary>
/// UI 管理器：启动时在场景中实例化规定的 MainUI 预制体，并开放通过预制体创建/销毁 UI 的方法。
/// 可通过 God 获取：God.Instance?.Get&lt;UIManager&gt;()
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>规定的 MainUI 预制体，Awake 时会在场景中实例化。</summary>
    [SerializeField] private GameObject _mainUIPrefab;

    private void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);

        if (_mainUIPrefab != null)
            CreateUI(_mainUIPrefab);
    }

    /// <summary>用预制体在场景中实例化 UI，失败返回 null。</summary>
    public GameObject CreateUI(GameObject prefab)
    {
        if (prefab == null) return null;
        return Instantiate(prefab);
    }

    /// <summary>用预制体在指定父节点下实例化 UI；parent 为 null 则实例化到场景根。失败返回 null。</summary>
    public GameObject CreateUI(GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;
        return parent != null ? Instantiate(prefab, parent) : Instantiate(prefab);
    }

    /// <summary>销毁指定 UI 物体。</summary>
    public void DestroyUI(GameObject ui)
    {
        if (ui == null) return;
        Destroy(ui);
    }

    /// <summary>立即销毁指定 UI 物体。</summary>
    public void DestroyUIImmediate(GameObject ui)
    {
        if (ui == null) return;
        DestroyImmediate(ui);
    }
}
