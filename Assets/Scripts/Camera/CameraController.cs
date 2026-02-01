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
}
