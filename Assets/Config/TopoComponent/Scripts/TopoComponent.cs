using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TopoComponent", menuName = "Scriptable Objects/TopoComponent")]
public class TopoComponent : ScriptableObject
{
    [Header("基础配置")]
    [Tooltip("类型ID，用于标识该拓扑组件的类型")]
    public int typeId;

    [Tooltip("预制体引用（从 Project 拖入）")]
    public GameObject prefab;

    [Header("旋转配置")]
    [Tooltip("是否在匹配时考虑旋转角")]
    public bool considerRotation;

    [Tooltip("视为相似/等效的旋转角列表（欧拉角，单位：度）")]
    public List<float> rotationSimilarList = new List<float>();
}
