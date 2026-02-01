using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 相机控制：挂在带 Cinemachine Camera 的物体上，提供单例与绑定跟踪目标方法。
/// 角色创建后调用 Instance.BindTarget(playerTransform) 即可让相机跟随角色。
/// 地图边界：在 Cinemachine Camera 上已添加 CinemachineConfiner2D 后，由 GameProcessManager 加载地图后调用 SetConfinerBoundary 从地图的 border 物体获取 Collider2D 并设置。
/// </summary>
public class CameraController : MonoBehaviour
{
    private static CameraController _instance;

    public static CameraController Instance => _instance;

    private CinemachineCamera _cinemachineCamera;

    [Header("拖拽时镜头")]
    [Tooltip("正常游玩时的 Orthographic Size（恢复用）")]
    [SerializeField] private float _defaultLensSize = 8f;
    [Tooltip("玩家拖拽部件时的 Orthographic Size（拉近）")]
    [SerializeField] private float _dragLensSize = 3f;
    [Tooltip("镜头缩放过渡时长（秒）")]
    [SerializeField] private float _zoomTransitionDuration = 0.25f;

    private Coroutine _zoomCoroutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        _cinemachineCamera = GetComponent<CinemachineCamera>();
        if (_cinemachineCamera == null)
            _cinemachineCamera = GetComponentInChildren<CinemachineCamera>();

        // 若未在 Inspector 改过，可用当前镜头值作为默认
        if (_cinemachineCamera != null && _defaultLensSize <= 0f)
            _defaultLensSize = _cinemachineCamera.Lens.OrthographicSize;
    }

    /// <summary>
    /// 设置 Confiner2D 的边界形状（由 GameProcessManager 加载地图后从地图的 border 物体获取 Collider2D 并调用）。
    /// </summary>
    public void SetConfinerBoundary(Collider2D boundingShape2D)
    {
        if (boundingShape2D == null) return;
        var go = _cinemachineCamera != null ? _cinemachineCamera.gameObject : gameObject;
        var confiner = go.GetComponent<CinemachineConfiner2D>();
        if (confiner == null) return;
        confiner.BoundingShape2D = boundingShape2D;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>
    /// 将相机的跟踪与注视目标绑定为指定物体（通常为角色）。
    /// </summary>
    /// <param name="target">要跟随的 Transform，传 null 则清除绑定。</param>
    public void BindTarget(Transform target)
    {
        if (_cinemachineCamera == null) return;

        _cinemachineCamera.Follow = target;
        _cinemachineCamera.LookAt = target;
    }

    /// <summary>
    /// 设置镜头 Orthographic Size（用于拖拽时拉近、松手后恢复）。
    /// </summary>
    public void SetLensOrthographicSize(float size)
    {
        if (_cinemachineCamera == null) return;
        var lens = _cinemachineCamera.Lens;
        lens.OrthographicSize = size;
        _cinemachineCamera.Lens = lens;
    }

    /// <summary>
    /// 拖拽开始时调用：镜头缩至 _dragLensSize（拉近）。拖拽结束时调用：恢复至 _defaultLensSize。带平滑过渡。
    /// </summary>
    /// <param name="isDragging">true=正在拖拽，false=拖拽结束。</param>
    public void SetDragZoom(bool isDragging)
    {
        float targetSize = isDragging ? _dragLensSize : _defaultLensSize;
        if (_zoomTransitionDuration <= 0f)
        {
            SetLensOrthographicSize(targetSize);
            return;
        }
        if (_zoomCoroutine != null)
            StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(ZoomTransitionRoutine(targetSize));
    }

    private IEnumerator ZoomTransitionRoutine(float targetSize)
    {
        if (_cinemachineCamera == null) yield break;
        float duration = Mathf.Max(0.01f, _zoomTransitionDuration);
        float startSize = _cinemachineCamera.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t); // SmoothStep
            float size = Mathf.Lerp(startSize, targetSize, t);
            SetLensOrthographicSize(size);
            yield return null;
        }

        SetLensOrthographicSize(targetSize);
        _zoomCoroutine = null;
    }
}
