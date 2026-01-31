using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 相机控制：挂在带 Cinemachine Camera 的物体上，提供单例与绑定跟踪目标方法。
/// 角色创建后调用 Instance.BindTarget(playerTransform) 即可让相机跟随角色。
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
