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

    /// <summary>暴露值 Slider 的 Fill 区域 Image，用于按百分比变色（黄→橙）。</summary>
    private Image _exposedValueFillImage;

    /// <summary>可拾取提示物体（MainUI 下名为 "E" 的物体，自动查找）。</summary>
    private GameObject _pickableHint;
    /// <summary>可暗杀提示物体（MainUI 下名为 "F" 的物体，自动查找）。</summary>
    private GameObject _assassinationHint;

    private static readonly Color ExposedFillColorAtZero = new Color(0.498f, 0.745f, 0.635f); // #7fbea2
    private static readonly Color ExposedFillColorAtMid = new Color(0.706f, 0.439f, 0.184f);   // #b4702f 50%
    private static readonly Color ExposedFillColorAtFull = new Color(0.827f, 0.153f, 0.239f); // #d3393d

    private void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);

        if (_mainUIPrefab != null)
        {
            _mainUIInstance = CreateUI(_mainUIPrefab);
            RefreshExposedValueSlider();
            RefreshRangeHintRefs();
        }
        SetPickableHintVisible(false);
        SetAssassinationHintVisible(false);
    }

    /// <summary>从 MainUI 子物体中按名称查找「E」「F」并缓存为范围提示引用。</summary>
    private void RefreshRangeHintRefs()
    {
        _pickableHint = null;
        _assassinationHint = null;
        if (_mainUIInstance == null) return;
        foreach (Transform t in _mainUIInstance.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "E") _pickableHint = t.gameObject;
            else if (t.name == "F") _assassinationHint = t.gameObject;
        }
    }

    /// <summary>显示/隐藏「可拾取」提示物体（由 PlayerRangeDetector 根据范围内有无 PickableItem 调用）。</summary>
    public void SetPickableHintVisible(bool visible)
    {
        if (_pickableHint != null)
            _pickableHint.SetActive(visible);
    }

    /// <summary>显示/隐藏「可暗杀」提示物体（由 PlayerRangeDetector 根据范围内有无敌人调用）。</summary>
    public void SetAssassinationHintVisible(bool visible)
    {
        if (_assassinationHint != null)
            _assassinationHint.SetActive(visible);
    }

    /// <summary>遍历 MainUI 所有子物体，查找并缓存 Slider 引用。</summary>
    private void RefreshExposedValueSlider()
    {
        _exposedValueSlider = null;
        _exposedValueFillImage = null;
        if (_mainUIInstance == null) return;
        _exposedValueSlider = _mainUIInstance.GetComponentInChildren<Slider>(true);
        if (_exposedValueSlider != null)
        {
            _exposedValueSlider.interactable = false;
            if (_exposedValueSlider.fillRect != null)
                _exposedValueFillImage = _exposedValueSlider.fillRect.GetComponent<Image>();
            ApplyExposedValueFillColor();
        }
    }

    /// <summary>根据当前 Slider 百分比更新 Fill 区域颜色：0%→50%→100% 三段渐变。</summary>
    private void ApplyExposedValueFillColor()
    {
        if (_exposedValueSlider == null || _exposedValueFillImage == null) return;
        float min = _exposedValueSlider.minValue;
        float max = _exposedValueSlider.maxValue;
        if (max <= min) return;
        float t = Mathf.Clamp01((_exposedValueSlider.value - min) / (max - min));
        _exposedValueFillImage.color = t <= 0.5f
            ? Color.Lerp(ExposedFillColorAtZero, ExposedFillColorAtMid, t * 2f)
            : Color.Lerp(ExposedFillColorAtMid, ExposedFillColorAtFull, (t - 0.5f) * 2f);
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
        {
            _exposedValueSlider.value = Mathf.Clamp(value, _exposedValueSlider.minValue, _exposedValueSlider.maxValue);
            ApplyExposedValueFillColor();
        }
    }

    /// <summary>设置暴露值 Slider 的最小值与最大值。</summary>
    public void SetExposedValueRange(float min, float max)
    {
        if (_exposedValueSlider == null) return;
        _exposedValueSlider.minValue = min;
        _exposedValueSlider.maxValue = max;
        _exposedValueSlider.value = Mathf.Clamp(_exposedValueSlider.value, min, max);
        ApplyExposedValueFillColor();
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
        {
            _exposedValueSlider.value = _exposedValueSlider.minValue;
            ApplyExposedValueFillColor();
        }
    }

    /// <summary>将暴露值设为最大值。</summary>
    public void SetExposedValueToMax()
    {
        if (_exposedValueSlider != null)
        {
            _exposedValueSlider.value = _exposedValueSlider.maxValue;
            ApplyExposedValueFillColor();
        }
    }

    #endregion
}
