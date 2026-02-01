using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 管理器：启动时在场景中实例化规定的 MainUI 预制体，并开放通过预制体创建/销毁 UI 的方法。
/// 可通过 God 获取：God.Instance?.Get&lt;UIManager&gt;()
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>规定的 MainUI 预制体，Awake 时会在场景中实例化。</summary>
    [SerializeField] private GameObject _mainUIPrefab;

    [Header("主菜单 / 胜利·失败")]
    [Tooltip("主菜单预制体，游戏一开始就显示，内有 StartButton 开始游戏")]
    [SerializeField] private GameObject _mainMenuPrefab;
    [Tooltip("胜利界面预制体，内有 BackHomeButton 返回主菜单")]
    [SerializeField] private GameObject _victoryUIPrefab;
    [Tooltip("失败界面预制体，内有 BackHomeButton 返回主菜单")]
    [SerializeField] private GameObject _failureUIPrefab;

    /// <summary>MainUI 实例，创建后缓存。</summary>
    private GameObject _mainUIInstance;
    private GameObject _mainMenuInstance;
    private GameObject _victoryUIInstance;
    private GameObject _failureUIInstance;

    /// <summary>暴露值 Slider，从 MainUI 子物体中遍历查找得到。</summary>
    private Slider _exposedValueSlider;

    /// <summary>暴露值 Slider 的 Fill 区域 Image，用于按百分比变色（黄→橙）。</summary>
    private Image _exposedValueFillImage;

    /// <summary>可拾取提示物体（MainUI 下名为 "E" 的物体，自动查找）。</summary>
    private GameObject _pickableHint;
    /// <summary>可暗杀提示物体（MainUI 下名为 "F" 的物体，自动查找）。</summary>
    private GameObject _assassinationHint;
    /// <summary>旋转提示物体（MainUI 下名为 "R" 的物体，拖拽物品时显示）。</summary>
    private GameObject _rotateHint;

    /// <summary>MainUI 下名为 "Env_Panel" 的 Image，用于被识破时变红、非识破时黑色。</summary>
    private Image _envPanelImage;

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
            BindTBtnClearComposable();
        }

        if (_mainMenuPrefab != null)
        {
            _mainMenuInstance = CreateUI(_mainMenuPrefab);
            _mainMenuInstance.SetActive(true);
        }

        if (_victoryUIPrefab != null)
        {
            _victoryUIInstance = CreateUI(_victoryUIPrefab);
            _victoryUIInstance.SetActive(false);
        }

        if (_failureUIPrefab != null)
        {
            _failureUIInstance = CreateUI(_failureUIPrefab);
            _failureUIInstance.SetActive(false);
        }

        if (_mainMenuInstance != null && _mainUIInstance != null)
            _mainUIInstance.SetActive(false);

        SetPickableHintVisible(false);
        SetAssassinationHintVisible(false);
        SetRotateHintVisible(false);
        SetEnvPanelSpotted(false);

        Time.timeScale = 0f;
    }

    private void Start()
    {
        var process = God.Instance?.Get<GameProcessManager>();
        if (process != null)
        {
            process.OnVictory += OnGameVictory;
            process.OnGameOver += OnGameOver;
        }
        BindMainMenuStartButton();
        BindBackHomeButton(_victoryUIInstance);
        BindBackHomeButton(_failureUIInstance);
    }

    private void OnDestroy()
    {
        var process = God.Instance?.Get<GameProcessManager>();
        if (process != null)
        {
            process.OnVictory -= OnGameVictory;
            process.OnGameOver -= OnGameOver;
        }
    }

    private void OnGameVictory()
    {
        Time.timeScale = 0f;
        if (_victoryUIInstance != null)
            _victoryUIInstance.SetActive(true);
    }

    private void OnGameOver()
    {
        Time.timeScale = 0f;
        if (_failureUIInstance != null)
            _failureUIInstance.SetActive(true);
    }

    private void BindMainMenuStartButton()
    {
        if (_mainMenuInstance == null) return;
        var btn = FindButtonInChildren(_mainMenuInstance.transform, "StartButton");
        if (btn != null)
            btn.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        if (_mainMenuInstance != null)
            _mainMenuInstance.SetActive(false);
        if (_mainUIInstance != null)
            _mainUIInstance.SetActive(true);
        God.Instance?.Get<GameProcessManager>()?.StartGame();
    }

    private void BindBackHomeButton(GameObject root)
    {
        if (root == null) return;
        var btn = FindButtonInChildren(root.transform, "BackHomeButton");
        if (btn != null)
            btn.onClick.AddListener(OnBackHomeButtonClicked);
    }

    private void OnBackHomeButtonClicked()
    {
        Time.timeScale = 0f;
        if (_victoryUIInstance != null)
            _victoryUIInstance.SetActive(false);
        if (_failureUIInstance != null)
            _failureUIInstance.SetActive(false);
        if (_mainUIInstance != null)
            _mainUIInstance.SetActive(false);
        if (_mainMenuInstance != null)
            _mainMenuInstance.SetActive(true);
    }

    private static Button FindButtonInChildren(Transform root, string buttonName)
    {
        if (root == null) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != buttonName) continue;
            var btn = t.GetComponent<Button>();
            if (btn != null) return btn;
        }
        return null;
    }

    /// <summary>从 MainUI 中查找名为 "T_Btn" 的节点，为其上的按钮绑定点击时调用 ClearAllPlayerComposable。</summary>
    private void BindTBtnClearComposable()
    {
        if (_mainUIInstance == null) return;
        Transform tBtn = _mainUIInstance.transform.Find("Canvas/Bottom/LeftPart/T_Btn");
        if (tBtn == null) return;
        var button = tBtn.GetComponent<Button>();
        if (button == null) return;
        button.onClick.AddListener(OnTBtnClicked);
    }

    private void OnTBtnClicked()
    {
        God.Instance?.Get<ComposableManager>()?.ClearAllPlayerComposable();
    }

    /// <summary>从 MainUI 子物体中按名称查找「E」「F」「R」并缓存为范围提示引用。</summary>
    private void RefreshRangeHintRefs()
    {
        _pickableHint = null;
        _assassinationHint = null;
        _rotateHint = null;
        _envPanelImage = null;
        if (_mainUIInstance == null) return;
        foreach (Transform t in _mainUIInstance.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "E") _pickableHint = t.gameObject;
            else if (t.name == "F") _assassinationHint = t.gameObject;
            else if (t.name == "R") _rotateHint = t.gameObject;
            else if (t.name == "Env_Panel")
            {
                var img = t.GetComponent<Image>();
                if (img != null) _envPanelImage = img;
            }
        }
    }

    /// <summary>显示/隐藏「可拾取」提示物体；显示时若传入 text 则同步设置 E 下子物体中第一个 Text 的内容。</summary>
    public void SetPickableHintVisible(bool visible, string text = null)
    {
        if (_pickableHint == null) return;
        _pickableHint.SetActive(visible);
        if (visible && !string.IsNullOrEmpty(text))
            SetHintChildText(_pickableHint.transform, text);
    }

    /// <summary>显示/隐藏「可暗杀」提示物体；显示时若传入 text 则同步设置 F 下子物体中第一个 Text 的内容。</summary>
    public void SetAssassinationHintVisible(bool visible, string text = null)
    {
        if (_assassinationHint == null) return;
        _assassinationHint.SetActive(visible);
        if (visible && !string.IsNullOrEmpty(text))
            SetHintChildText(_assassinationHint.transform, text);
    }

    /// <summary>显示/隐藏「旋转」提示物体（与 E/F 同区域，拖拽物品时显示）；显示时若传入 text 则同步设置 R 下子物体中第一个 Text 的内容。</summary>
    public void SetRotateHintVisible(bool visible, string text = null)
    {
        if (_rotateHint == null) return;
        _rotateHint.SetActive(visible);
        if (visible && !string.IsNullOrEmpty(text))
            SetHintChildText(_rotateHint.transform, text);
    }

    /// <summary>设置 MainUI 中 Env_Panel 颜色：被识破为红，非识破为黑。</summary>
    public void SetEnvPanelSpotted(bool isSpotted)
    {
        if (_envPanelImage == null) return;
        _envPanelImage.color = isSpotted ? Color.red : Color.black;
    }

    /// <summary>在指定节点下查找第一个 Text 或 TextMeshProUGUI 子物体并设置其文本。</summary>
    private static void SetHintChildText(Transform root, string text)
    {
        if (root == null) return;
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }
        var uiText = root.GetComponentInChildren<Text>(true);
        if (uiText != null)
            uiText.text = text;
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
