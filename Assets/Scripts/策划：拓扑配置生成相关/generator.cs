using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Generator : MonoBehaviour
{
    [Header("拓扑配置")]
    [Tooltip("TopoComponent SO 列表，按 typeId 查找")]
    public List<TopoComponent> componentConfigs = new List<TopoComponent>();

    [Header("生成参数")]
    [Tooltip("要生成的类型 ID")]
    public int spawnTypeId;
    [Tooltip("生成数量")]
    public int spawnCount = 1;

    [Header("生成区域")]
    [Tooltip("区域左下角（XY 平面，归一化 0,0 对应）")]
    public Vector3 spawnAreaMin = new Vector3(-10f, -10f, 0f);
    [Tooltip("区域右上角（XY 平面，归一化 1,1 对应）")]
    public Vector3 spawnAreaMax = new Vector3(10f, 10f, 0f);

    [Tooltip("生成实例的父节点，为空则用本物体")]
    public Transform spawnParent;

    [Header("构型图代码")]
    [TextArea(4, 16)]
    [Tooltip("导入/导出的构型图字符串")]
    public string topoCodeString = "";

    /// <summary>从 Marker 数据序列化为单元素字符串 1@类型-2@x^y-3@0/1-4@角度-5@角度1^角度2</summary>
    public static string ElementToSerializedString(int typeId, Vector2 pos, float rot, bool considerRotFlag, List<float> rotList)
    {
        var parts = new List<string>
        {
            "1@" + Mathf.Max(0, typeId),
            "2@" + Mathf.Clamp01(pos.x).ToString("G6") + "^" + Mathf.Clamp01(pos.y).ToString("G6"),
            "3@" + (considerRotFlag ? "1" : "0")
        };
        parts.Add("4@" + GenGoodRot(rot).ToString("G6"));
        if (rotList != null && rotList.Count > 0)
        {
            var valid = new List<string>();
            foreach (float v in rotList)
                if (v > 0f && v < 360f) valid.Add(v.ToString("G6"));
            if (valid.Count > 0)
                parts.Add("5@" + string.Join("^", valid));
        }
        return string.Join("-", parts);
    }

    static float GenGoodRot(float rot)
    {
        while (rot < 0f) rot += 360f;
        while (rot >= 360f) rot -= 360f;
        return rot;
    }

    TopoComponent GetConfigByTypeId(int typeId)
    {
        if (componentConfigs == null) return null;
        foreach (var c in componentConfigs)
        {
            if (c != null && c.typeId == typeId) return c;
        }
        return null;
    }

    Transform GetSpawnParent() => spawnParent != null ? spawnParent : transform;

    /// <summary>将归一化坐标 (0~1) 映射到世界坐标（XY 平面，2D）</summary>
    Vector3 NormalizedToWorld(Vector2 norm)
    {
        float x = spawnAreaMin.x + (spawnAreaMax.x - spawnAreaMin.x) * Mathf.Clamp01(norm.x);
        float y = spawnAreaMin.y + (spawnAreaMax.y - spawnAreaMin.y) * Mathf.Clamp01(norm.y);
        return new Vector3(x, y, spawnAreaMin.z);
    }

#if UNITY_EDITOR
    /// <summary>始终在 Scene 中绘制区域边界（XY 平面）：左下角=归一化(0,0)=spawnAreaMin，右上角=归一化(1,1)=spawnAreaMax</summary>
    void OnDrawGizmos()
    {
        Vector3 center = (spawnAreaMin + spawnAreaMax) * 0.5f;
        float sizeX = Mathf.Abs(spawnAreaMax.x - spawnAreaMin.x);
        float sizeY = Mathf.Abs(spawnAreaMax.y - spawnAreaMin.y);
        if (sizeX < 0.01f) sizeX = 0.01f;
        if (sizeY < 0.01f) sizeY = 0.01f;
        Vector3 size = new Vector3(sizeX, sizeY, 0.01f);
        Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.5f);
        Gizmos.DrawWireCube(center, size);
    }
#endif

    /// <summary>将世界坐标映射回归一化 (0~1)，用于导出时从 transform 写回 Marker（XY 平面）</summary>
    Vector2 WorldToNormalized(Vector3 world)
    {
        float dx = spawnAreaMax.x - spawnAreaMin.x;
        float dy = spawnAreaMax.y - spawnAreaMin.y;
        if (Mathf.Approximately(dx, 0f)) dx = 1f;
        if (Mathf.Approximately(dy, 0f)) dy = 1f;
        float nx = (world.x - spawnAreaMin.x) / dx;
        float ny = (world.y - spawnAreaMin.y) / dy;
        return new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));
    }

    /// <summary>按 typeId 和数量生成，在生成区域内按网格放置</summary>
    public void SpawnByTypeIdAndCount()
    {
        var config = GetConfigByTypeId(spawnTypeId);
        if (config == null)
        {
            Debug.LogError("[Generator] 未找到 typeId=" + spawnTypeId + " 的 TopoComponent 配置。");
            return;
        }
        if (config.prefab == null)
        {
            Debug.LogError("[Generator] TopoComponent typeId=" + spawnTypeId + " 未分配预制体。");
            return;
        }
        var prefab = config.prefab;
        Transform parent = GetSpawnParent();
        int count = Mathf.Max(0, spawnCount);
        int cols = count <= 1 ? 1 : Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = count <= 1 ? 1 : (count + cols - 1) / cols;
        for (int i = 0; i < count; i++)
        {
            float nx = cols > 1 ? (i % cols) / (float)(cols - 1) : 0.5f;
            float nz = rows > 1 ? (i / cols) / (float)(rows - 1) : 0.5f;
            var norm = new Vector2(nx, nz);
            float rot = 0f;
            if (config.considerRotation && config.rotationSimilarList != null && config.rotationSimilarList.Count > 0)
            {
                int idx = Random.Range(0, config.rotationSimilarList.Count);
                rot = config.rotationSimilarList[idx];
            }
            Vector3 worldPos = NormalizedToWorld(norm);
            Quaternion worldRot = Quaternion.Euler(0f, 0f, rot);
            var go = CreateInstance(prefab, worldPos, worldRot, parent);
            var marker = go.GetComponent<TopoInstanceMarker>();
            if (marker == null) marker = go.AddComponent<TopoInstanceMarker>();
            marker.typeId = config.typeId;
            marker.normalizedPos = norm;
            marker.rot = rot;
            marker.considerRotFlag = config.considerRotation;
            marker.rotList = config.rotationSimilarList != null ? new List<float>(config.rotationSimilarList) : new List<float> { 0f, 360f };
        }
    }

    /// <summary>在编辑模式或运行模式下创建预制体实例，编辑模式下可撤销</summary>
    static GameObject CreateInstance(GameObject prefab, Vector3 worldPos, Quaternion worldRot, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (go != null)
            {
                go.transform.position = worldPos;
                go.transform.rotation = worldRot;
                Undo.RegisterCreatedObjectUndo(go, "Topo Spawn");
            }
            return go;
        }
#endif
        return Instantiate(prefab, worldPos, worldRot, parent);
    }

    /// <summary>清空当前管理的所有拓扑实例</summary>
    public void ClearAll()
    {
        Transform parent = GetSpawnParent();
        var markers = parent.GetComponentsInChildren<TopoInstanceMarker>(true);
        for (int i = markers.Length - 1; i >= 0; i--)
        {
            if (markers[i] != null && markers[i].gameObject != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.DestroyObjectImmediate(markers[i].gameObject);
                else
#endif
                    DestroyImmediate(markers[i].gameObject);
            }
        }
    }

    /// <summary>从 Transform 的世界旋转取 Z 角（2D），归一化到 0~360</summary>
    static float GetWorldRotationZ(Transform t)
    {
        float z = t.rotation.eulerAngles.z;
        if (z < 0f) z += 360f;
        return z % 360f;
    }

    /// <summary>根据当前场景中本 Generator 管理的 Topo 实例，导出构型图字符串（单图，多元素用 ; 分隔）</summary>
    public string ExportTopoCode()
    {
        Transform parent = GetSpawnParent();
        var markers = parent.GetComponentsInChildren<TopoInstanceMarker>(true);
        var parts = new List<string>();
        foreach (var m in markers)
        {
            if (m == null) continue;
            Vector2 pos = WorldToNormalized(m.transform.position);
            float rot = GetWorldRotationZ(m.transform);
            m.rot = rot;
            parts.Add(ElementToSerializedString(m.typeId, pos, rot, m.considerRotFlag, m.rotList));
        }
        return string.Join(";", parts);
    }

    /// <summary>从 topoCodeString 解析并生成物体，先清空当前管理实例；解析失败则不清空、不导入</summary>
    public void ImportTopoCode()
    {
        if (string.IsNullOrWhiteSpace(topoCodeString))
        {
            Debug.LogWarning("[Generator] 构型图字符串为空，跳过导入。");
            return;
        }
        var mask = new MaskCore();
        try
        {
            mask.Parse(topoCodeString.Trim());
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Generator] 构型图解析失败: " + e.Message);
            return;
        }
        if (mask.UnitCount == 0)
        {
            Debug.LogWarning("[Generator] 构型图解析后无有效单元，跳过导入。");
            return;
        }
        ClearAll();
        Transform parent = GetSpawnParent();
        for (int u = 0; u < mask.UnitCount; u++)
        {
            var unit = mask.GetUnit(u);
            for (int e = 0; e < unit.ElementCount; e++)
            {
                var ele = unit.GetElement(e);
                var config = GetConfigByTypeId(ele.type);
                if (config == null)
                {
                    Debug.LogError("[Generator] 导入时未找到 typeId=" + ele.type + " 的 TopoComponent，跳过该元素。");
                    continue;
                }
                if (config.prefab == null)
                {
                    Debug.LogError("[Generator] 导入时 TopoComponent typeId=" + ele.type + " 未分配预制体，跳过该元素。");
                    continue;
                }
                var prefab = config.prefab;
                Vector3 worldPos = NormalizedToWorld(ele.pos);
                Quaternion worldRot = Quaternion.Euler(0f, 0f, ele.rot);
                var go = CreateInstance(prefab, worldPos, worldRot, parent);
                var marker = go.GetComponent<TopoInstanceMarker>();
                if (marker == null) marker = go.AddComponent<TopoInstanceMarker>();
                marker.SetFromElement(ele);
            }
        }
    }
}
