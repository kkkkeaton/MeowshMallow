using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时挂在每个拓扑实例上，存 typeId、归一化位置、旋转角、considerRot、rotList，与 Element 一一对应，供导出与识别。
/// </summary>
public class TopoInstanceMarker : MonoBehaviour
{
    [Tooltip("类型ID")]
    public int typeId;

    [Tooltip("归一化位置 (0~1)，对应构型图 2@x^y")]
    public Vector2 normalizedPos;

    [Tooltip("旋转角 (0~360)，单轴 Y")]
    public float rot;

    [Tooltip("是否考虑旋转角")]
    public bool considerRotFlag;

    [Tooltip("旋转角相似列表 (0~360，不含 0 和 360)")]
    public List<float> rotList = new List<float>();

    /// <summary>从 Element 写入 Marker，用于导入后同步数据。</summary>
    public void SetFromElement(Element ele)
    {
        typeId = ele.type;
        normalizedPos = ele.pos;
        rot = ele.rot;
        considerRotFlag = ele.considerRotFlag;
        rotList = ele.rotList != null ? new List<float>(ele.rotList) : new List<float> { 0f, 360f };
    }
}
