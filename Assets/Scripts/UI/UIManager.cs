using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 管理器：启动时在场景中实例化规定的 MainUI 预制体，并开放通过预制体创建/销毁 UI 的方法。
/// 可通过 God 获取：God.Instance?.Get&lt;UIManager&gt;()
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>规定的 MainUI 预制体，Awake 时会在场景中实例化。</summary>
    [SerializeField] private GameObject _mainUIPrefab;

    /// <summary>MainUI 实例，创建后缓存。</summary>
    private GameObject _mainUIInstance;

    /// <summary>暴露值 Slider，从 MainUI 子物体中遍历查找得到。</summary>
    private Slider _exposedValueSlider;

    private void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);

        if (_mainUIPrefab != null)
        {
            _mainUIInstance = CreateUI(_mainUIPrefab);
            RefreshExposedValueSlider();
        }
    }

    /// <summary>遍历 MainUI 所有子物体，查找并缓存 Slider 引用。</summary>
    private void RefreshExposedValueSlider()
    {
        _exposedValueSlider = null;
        if (_mainUIInstance == null) return;
        _exposedValueSlider = _mainUIInstance.GetComponentInChildren<Slider>(true);
        if (_exposedValueSlider != null)
            _exposedValueSlider.interactable = false;
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

    #region 暴露值 Slider 控制

    /// <summary>获取暴露值 Slider 的当前数值。若未找到 Slider 返回 0。</summary>
    public float GetExposedValue()
    {
        return _exposedValueSlider != null ? _exposedValueSlider.value : 0f;
    }

    /// <summary>设置暴露值 Slider 的当前数值。</summary>
    public void SetExposedValue(float value)
    {
        if (_exposedValueSlider != null)
            _exposedValueSlider.value = Mathf.Clamp(value, _exposedValueSlider.minValue, _exposedValueSlider.maxValue);
    }

    /// <summary>设置暴露值 Slider 的最小值与最大值。</summary>
    public void SetExposedValueRange(float min, float max)
    {
        if (_exposedValueSlider == null) return;
        _exposedValueSlider.minValue = min;
        _exposedValueSlider.maxValue = max;
        _exposedValueSlider.value = Mathf.Clamp(_exposedValueSlider.value, min, max);
    }

    /// <summary>获取暴露值 Slider 的最小值。</summary>
    public float GetExposedValueMin()
    {
        return _exposedValueSlider != null ? _exposedValueSlider.minValue : 0f;
    }

    /// <summary>获取暴露值 Slider 的最大值。</summary>
    public float GetExposedValueMax()
    {
        return _exposedValueSlider != null ? _exposedValueSlider.maxValue : 1f;
    }

    /// <summary>将暴露值设为最小值。</summary>
    public void SetExposedValueToMin()
    {
        if (_exposedValueSlider != null)
            _exposedValueSlider.value = _exposedValueSlider.minValue;
    }

    /// <summary>将暴露值设为最大值。</summary>
    public void SetExposedValueToMax()
    {
        if (_exposedValueSlider != null)
            _exposedValueSlider.value = _exposedValueSlider.maxValue;
    }

    #endregion
}
