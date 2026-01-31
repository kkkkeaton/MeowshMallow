using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>怪物配置资源：在 Inspector 中编辑怪物类型与 Prefab、血量、移速等，供 Manager/Factory 按 ID 查询。</summary>
[CreateAssetMenu(fileName = "MonsterConfig", menuName = "Scriptable Objects/MonsterConfig")]
public class MonsterConfig : ScriptableObject
{
    /// <summary>单条怪物配置：ID、预制体、最大血量、移速（Unity 可序列化，用于 Inspector 列表）。</summary>
    [Serializable]
    public class MonsterConfigEntry
    {
        public string id;           // 怪物类型 ID
        public GameObject prefab;   // 2D 怪物预制体（需挂 MonsterBase）
        public float maxHp = 100f;  // 最大血量
        public float moveSpeed = 1f; // 移动速度
    }

    [SerializeField] private List<MonsterConfigEntry> entries = new List<MonsterConfigEntry>();
    private Dictionary<string, MonsterConfigEntry> _cache; // 运行时按 id 建缓存，O(1) 查找

    /// <summary>首次按 ID 查询时构建字典缓存。</summary>
    private void BuildCache()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, MonsterConfigEntry>();
        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(e.id) && !_cache.ContainsKey(e.id))
                _cache[e.id] = e;
        }
    }

    /// <summary>根据怪物 ID 获取预制体，未找到返回 null。</summary>
    public GameObject GetPrefab(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.prefab : null;
    }

    /// <summary>根据怪物 ID 获取最大血量，未找到返回 0。</summary>
    public float GetMaxHp(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.maxHp : 0f;
    }

    /// <summary>根据怪物 ID 获取移速，未找到返回 0。</summary>
    public float GetMoveSpeed(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e.moveSpeed : 0f;
    }

    /// <summary>根据怪物 ID 获取完整配置条目，未找到返回 null。</summary>
    public MonsterConfigEntry GetEntry(string id)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(id, out var e) ? e : null;
    }
}
