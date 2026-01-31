using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 怪物配置资源：持有一组怪物数据资产的引用列表，按 ID 查询。
/// 每只怪单独存为 MonsterConfigData 资产，在此列表中引用；Manager/Factory 通过本配置按 ID 获取属性。
/// </summary>
[CreateAssetMenu(fileName = "MonsterConfig", menuName = "Scriptable Objects/MonsterConfig")]
public class MonsterConfig : ScriptableObject
{
    [Tooltip("怪物数据资产列表，每只怪一个 MonsterConfigData 资产")]
    [SerializeField] private List<MonsterConfigData> entries = new List<MonsterConfigData>();

    private Dictionary<string, MonsterConfigData> _cache; // 运行时按 id 建缓存，O(1) 查找

    /// <summary>首次按 ID 查询时构建字典缓存。</summary>
    private void BuildCache()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, MonsterConfigData>();
        foreach (var e in entries)
        {
            if (e == null) continue;
            if (!string.IsNullOrEmpty(e.Id) && !_cache.ContainsKey(e.Id))
                _cache[e.Id] = e;
        }
    }

    /// <summary>根据怪物 ID 获取预制体，未找到返回 null。</summary>
    public GameObject GetPrefab(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.Prefab : null;
    }

    /// <summary>根据怪物 ID 获取最大血量，未找到返回 0。</summary>
    public float GetMaxHp(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.MaxHp : 0f;
    }

    /// <summary>根据怪物 ID 获取移速，未找到返回 0。</summary>
    public float GetMoveSpeed(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.MoveSpeed : 0f;
    }

    /// <summary>根据怪物 ID 获取索敌检测距离，未找到返回 0。</summary>
    public float GetDetectionRange(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.DetectionRange : 0f;
    }

    /// <summary>根据怪物 ID 获取靠近玩家的目标距离，未找到返回 0。</summary>
    public float GetApproachDistance(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.ApproachDistance : 0f;
    }

    /// <summary>根据怪物 ID 获取识破值每秒增加量，未找到返回 0。</summary>
    public float GetDetectionFillRatePerSecond(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.DetectionFillRatePerSecond : 0f;
    }

    /// <summary>根据怪物 ID 获取识破值满值，未找到返回 100。</summary>
    public float GetDetectionMaxValue(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.DetectionMaxValue : 100f;
    }

    /// <summary>根据怪物 ID 获取完整配置条目，未找到返回 null。</summary>
    public MonsterConfigData GetEntry(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e : null;
    }

    /// <summary>获取怪物拓扑配置字符串</summary>
    public string GetTopoConfig(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.MonsterTopoConfig : "";
    }

}
