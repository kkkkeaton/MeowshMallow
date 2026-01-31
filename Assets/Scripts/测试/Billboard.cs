using UnityEngine;

// 这是一个非常标准的 Billboard 脚本，挂在任何物体上即可
public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;
    public bool isActive = true;

    void Start()
    {
        // 如果没手动指定相机，自动获取主相机
        if (m_Camera == null) m_Camera = Camera.main;
    }

    void LateUpdate()
    {
        if (!isActive) return;
        if (m_Camera == null) return;

        // 核心代码：让物体的 Z 轴指向相机
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
                         m_Camera.transform.rotation * Vector3.up);
    }
}
