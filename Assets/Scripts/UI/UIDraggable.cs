using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 挂在 UI 物体上，拖拽时创建副本并拖动副本；松手时若放在 Player 附近则触发逻辑，否则仅删除副本。
/// 需要：① 场景有 EventSystem 且带输入模块；② 本物体或子物体有 Graphic 且勾选 Raycast Target。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("调试（运行时检查一次）")]
    [SerializeField] private bool _logSetupWarnings = true;

    [Header("放在 Player 附近判定与归一化范围（世界单位，X/Y 共用）")]
    [SerializeField] private float _playerNearWorldRadius = GlobalSetting.PLAYER_NEAR_WORLD_RADIUS;

    [Header("拖拽时旋转")]
    [Tooltip("拖入 Assets/InputSystem_Actions.inputactions")]
    [SerializeField] private InputActionAsset _playerInputActions;
    [SerializeField] private float _rotateSpeedDegPerSec = 90f;

    private Composable _composable;
    private InputAction _rotateAction;

    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Camera _eventCamera;
    private Vector2 _pointerStartInParent;
    private Vector2 _pivotStartInParent;
    private RectTransform _dragClone; // 当前被拖动的副本

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _eventCamera = _canvas.worldCamera;

        if (_playerInputActions != null)
        {
            var playerMap = _playerInputActions.FindActionMap("Player", true);
            _rotateAction = playerMap.FindAction("RotateItem");
        }
        if (_logSetupWarnings)
            CheckSetup();
    }

    /// <summary>设置当前存储的 Composable。</summary>
    public void SetComposable(Composable composable)
    {
        _composable = composable;
    }

    /// <summary>获取当前存储的 Composable。</summary>
    public Composable GetComposable()
    {
        return _composable;
    }

    /// <summary>检查拖拽能否被触发，缺配置时在 Console 里打一次提示。</summary>
    private void CheckSetup()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[UIDraggable] 场景里没有 EventSystem，拖拽不会触发。请菜单 GameObject → UI → Event System 添加。", this);
            return;
        }
        if (EventSystem.current.currentInputModule == null)
        {
            Debug.LogWarning("[UIDraggable] EventSystem 上没有输入模块，鼠标事件无效。请在 Hierarchy 选中 EventSystem，Inspector 里 Add Component → Standalone Input Module（或用 New Input System 的 Input System UI Input Module）。", this);
            return;
        }
        bool hasRaycast = GetComponent<Graphic>() != null && GetComponent<Graphic>().raycastTarget;
        if (!hasRaycast)
        {
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                if (g.raycastTarget) { hasRaycast = true; break; }
        }
        if (!hasRaycast)
            Debug.LogWarning("[UIDraggable] 本物体及子物体没有可接收射线的 Graphic（或未勾选 Raycast Target），拖拽不会触发。请在本物体上加一个 Image（可透明）并勾选 Raycast Target。", this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_canvas == null) return;
        Transform parent = _rectTransform.parent;
        if (parent == null) return;
        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, _eventCamera, out Vector2 localInParent))
        {
            GameObject clone = Instantiate(gameObject, parent);
            clone.name = gameObject.name + "(Clone)";
            clone.transform.SetAsLastSibling();
            _dragClone = clone.GetComponent<RectTransform>();
            _dragClone.localPosition = _rectTransform.localPosition;
            _dragClone.localRotation = _rectTransform.localRotation;
            _dragClone.localScale = _rectTransform.localScale;
            if (clone.TryGetComponent<UIDraggable>(out var cloneDraggable))
                cloneDraggable.enabled = false;

            _pointerStartInParent = localInParent;
            _pivotStartInParent = _dragClone.localPosition;
            if (_rotateAction != null)
                _rotateAction.Enable();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null || _dragClone == null) return;
        Transform parent = _rectTransform.parent;
        if (parent == null) return;
        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, _eventCamera, out Vector2 localInParent))
        {
            Vector2 delta = localInParent - _pointerStartInParent;
            _dragClone.localPosition = _pivotStartInParent + delta;
        }
    }

    private void Update()
    {
        if (_dragClone == null) return;
        if (_rotateAction != null && _rotateAction.IsPressed())
            _dragClone.Rotate(0f, 0f, _rotateSpeedDegPerSec * Time.deltaTime);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Camera cam = _eventCamera != null ? _eventCamera : Camera.main;
        Vector3 playerWorldPos = God.Instance != null && God.Instance.Player != null
            ? God.Instance.Player.transform.position
            : Vector3.zero;

        bool isNearPlayer = false;
        Vector2 relativeWorld = Vector2.zero;
        if (God.Instance != null && God.Instance.Player != null && cam != null && _playerNearWorldRadius > 0f)
        {
            // 用屏幕偏移 ÷ 相机在玩家处的“每世界单位像素数”，得到与 _playerNearWorldRadius 同单位的世界相对坐标
            Vector3 playerScreen = cam.WorldToScreenPoint(playerWorldPos);
            Vector2 dropScreen = eventData.position;
            Vector2 screenDelta = dropScreen - (Vector2)playerScreen;
            float pixelsPerWorldX = (cam.WorldToScreenPoint(playerWorldPos + Vector3.right) - playerScreen).x;
            float pixelsPerWorldY = (cam.WorldToScreenPoint(playerWorldPos + Vector3.up) - playerScreen).y;
            if (Mathf.Abs(pixelsPerWorldX) > 0.0001f && Mathf.Abs(pixelsPerWorldY) > 0.0001f)
            {
                relativeWorld.x = screenDelta.x / pixelsPerWorldX;
                relativeWorld.y = screenDelta.y / pixelsPerWorldY;
                // 与后面归一化矩形一致：relativeWorld 在 [-r, r] x [-r, r] 内即为在附近
                float r = _playerNearWorldRadius;
                isNearPlayer = Mathf.Abs(relativeWorld.x) <= r && Mathf.Abs(relativeWorld.y) <= r;
            }
        }

        if (isNearPlayer && cam != null && _dragClone != null && _playerNearWorldRadius > 0f)
        {
            // Screen Space - Overlay 下 UI 的 position 不是场景世界坐标，必须用 null 才能得到正确屏幕坐标
            Camera camForUI = (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;
            Vector3 playerScreen = cam.WorldToScreenPoint(playerWorldPos);
            Vector2 cloneScreen = RectTransformUtility.WorldToScreenPoint(camForUI, _dragClone.position);
            Vector2 screenDelta = cloneScreen - (Vector2)playerScreen;
            float pixelsPerWorldX = (cam.WorldToScreenPoint(playerWorldPos + Vector3.right) - playerScreen).x;
            float pixelsPerWorldY = (cam.WorldToScreenPoint(playerWorldPos + Vector3.up) - playerScreen).y;
            if (Mathf.Abs(pixelsPerWorldX) > 0.0001f && Mathf.Abs(pixelsPerWorldY) > 0.0001f)
            {
                relativeWorld.x = screenDelta.x / pixelsPerWorldX;
                relativeWorld.y = screenDelta.y / pixelsPerWorldY;
                float r = _playerNearWorldRadius;
                float normX = (relativeWorld.x + r) / (2f * r);
                float normY = (relativeWorld.y + r) / (2f * r);
                Vector2 normalized = new Vector2(normX, normY);

                float rotationDeg = _dragClone.localEulerAngles.z;
                Debug.Log($"[UIDraggable] 归一化坐标(左下0,0 右上1,1)={normalized}，拖拽物体旋转(度)={rotationDeg}");

                Composable composableConfig = GetComposable();
                ComposableManager manager = God.Instance?.Get<ComposableManager>();
                if (manager != null && composableConfig != null)
                    manager.GenPlayerNewComposable(composableConfig, normalized, rotationDeg);
            }
        }

        if (_rotateAction != null)
            _rotateAction.Disable();
        if (_dragClone != null)
        {
            Destroy(_dragClone.gameObject);
            _dragClone = null;
        }
    }
}
