using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("放在 Player 附近判定（屏幕像素）")]
    [SerializeField] private float _playerDropRadius = 80f;

    private TopoComponent _topoComponent;

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

        if (_logSetupWarnings)
            CheckSetup();
    }

    /// <summary>设置当前存储的 TopoComponent。</summary>
    public void SetTopoComponent(TopoComponent topoComponent)
    {
        _topoComponent = topoComponent;
    }

    /// <summary>获取当前存储的 TopoComponent。</summary>
    public TopoComponent GetTopoComponent()
    {
        return _topoComponent;
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

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dropScreenPos = eventData.position;
        bool isNearPlayer = false;
        if (God.Instance != null && God.Instance.Player != null)
        {
            Camera cam = _eventCamera != null ? _eventCamera : Camera.main;
            if (cam != null)
            {
                Vector3 playerWorldPos = God.Instance.Player.transform.position;
                Vector3 playerScreenPos = cam.WorldToScreenPoint(playerWorldPos);
                isNearPlayer = Vector2.Distance(dropScreenPos, (Vector2)playerScreenPos) <= _playerDropRadius;
            }
        }

        if (_dragClone != null)
        {
            Destroy(_dragClone.gameObject);
            _dragClone = null;
        }

        if (isNearPlayer)
        {
            // 放在 Player 附近时的逻辑（在此处填写）
            Debug.Log("OnEndDrag called! isNearPlayer=" + isNearPlayer);
        }
    }
}
