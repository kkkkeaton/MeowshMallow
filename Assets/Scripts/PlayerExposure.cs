using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家暴露值：按区域 ID 或怪物类型 ID 维护暴露值；
/// 识破值满时由怪物增加对应类型的暴露值。提供增加与减少方法，供后续隐身、脱离等逻辑使用。
/// 挂在 Player GameObject 上，使用 Instance 供怪物访问。
/// </summary>
public class PlayerExposure : MonoBehaviour
{
    public static PlayerExposure Instance { get; private set; }

    /// <summary>按 key（区域 ID 或怪物类型 ID）的暴露值。</summary>
    private readonly Dictionary<string, float> _exposureByKey = new Dictionary<string, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>增加指定 key（如怪物类型 ID）的暴露值。</summary>
    public void AddExposure(string key, float amount)
    {
        if (string.IsNullOrEmpty(key) || amount <= 0f) return;
        if (!_exposureByKey.TryGetValue(key, out var current))
            current = 0f;
        _exposureByKey[key] = current + amount;
    }

    /// <summary>增加在指定怪物类型中的暴露值（key 为怪物类型 ID）。</summary>
    public void AddExposureForMonsterType(string monsterTypeId, float amount)
    {
        AddExposure(monsterTypeId, amount);
    }

    /// <summary>增加在指定区域中的暴露值（key 为区域 ID）。</summary>
    public void AddExposureForArea(string areaId, float amount)
    {
        AddExposure(areaId, amount);
    }

    /// <summary>减少指定 key 的暴露值，不低于 0。</summary>
    public void ReduceExposure(string key, float amount)
    {
        if (string.IsNullOrEmpty(key) || amount <= 0f) return;
        if (!_exposureByKey.TryGetValue(key, out var current)) return;
        _exposureByKey[key] = Mathf.Max(0f, current - amount);
        if (_exposureByKey[key] <= 0f)
            _exposureByKey.Remove(key);
    }

    /// <summary>减少指定怪物类型中的暴露值。</summary>
    public void ReduceExposureForMonsterType(string monsterTypeId, float amount)
    {
        ReduceExposure(monsterTypeId, amount);
    }

    /// <summary>减少指定区域中的暴露值。</summary>
    public void ReduceExposureForArea(string areaId, float amount)
    {
        ReduceExposure(areaId, amount);
    }

    /// <summary>按比例减少指定 key 的暴露值（例如 0.5 表示减半），不低于 0。</summary>
    public void ReduceExposureByRatio(string key, float ratio)
    {
        if (string.IsNullOrEmpty(key) || ratio <= 0f) return;
        if (!_exposureByKey.TryGetValue(key, out var current)) return;
        _exposureByKey[key] = Mathf.Max(0f, current * (1f - ratio));
        if (_exposureByKey[key] <= 0f)
            _exposureByKey.Remove(key);
    }

    /// <summary>将所有暴露值减少固定量，不低于 0。</summary>
    public void ReduceAllExposure(float amount)
    {
        if (amount <= 0f) return;
        var keys = new List<string>(_exposureByKey.Keys);
        foreach (var key in keys)
        {
            var current = _exposureByKey[key];
            var next = Mathf.Max(0f, current - amount);
            if (next <= 0f)
                _exposureByKey.Remove(key);
            else
                _exposureByKey[key] = next;
        }
    }

    /// <summary>将所有暴露值乘以 (1 - ratio)，不低于 0。</summary>
    public void ReduceAllExposureByRatio(float ratio)
    {
        if (ratio <= 0f) return;
        var keys = new List<string>(_exposureByKey.Keys);
        foreach (var key in keys)
        {
            var next = Mathf.Max(0f, _exposureByKey[key] * (1f - ratio));
            if (next <= 0f)
                _exposureByKey.Remove(key);
            else
                _exposureByKey[key] = next;
        }
    }

    /// <summary>获取指定 key 的暴露值，不存在返回 0。</summary>
    public float GetExposure(string key)
    {
        return _exposureByKey.TryGetValue(key, out var v) ? v : 0f;
    }

    /// <summary>获取指定怪物类型的暴露值。</summary>
    public float GetExposureForMonsterType(string monsterTypeId)
    {
        return GetExposure(monsterTypeId);
    }

    /// <summary>获取指定区域的暴露值。</summary>
    public float GetExposureForArea(string areaId)
    {
        return GetExposure(areaId);
    }
}
