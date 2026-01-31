using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 挂在 UI 物体上，实现用鼠标拖拽时该物体跟随鼠标移动。
/// 需要物体或其父节点所在 Canvas 上有 Graphic（如 Image）或可接收射线检测的组件。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Camera _eventCamera;
    private Vector2 _pointerStartInParent; // 按下时鼠标在父节点局部空间的位置
    private Vector2 _pivotStartInParent;   // 按下时物体锚点在父节点局部空间的位置

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _eventCamera = _canvas.worldCamera;
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
            _pointerStartInParent = localInParent;
            _pivotStartInParent = _rectTransform.localPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null) return;
        Transform parent = _rectTransform.parent;
        if (parent == null) return;
        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, _eventCamera, out Vector2 localInParent))
        {
            Vector2 delta = localInParent - _pointerStartInParent;
            _rectTransform.localPosition = _pivotStartInParent + delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }
}
