using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 物品类型音效配置：按物品类型 ID 映射捡起/掉落音效。
/// 右键 Create > Scriptable Objects > ItemTypeAudioConfig 创建，在 AudioManager 中引用。
/// </summary>
[CreateAssetMenu(fileName = "ItemTypeAudioConfig", menuName = "Scriptable Objects/ItemTypeAudioConfig")]
public class ItemTypeAudioConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("物品类型 ID，与 Composable / PickableItem 的 itemTypeId 对应")]
        public int itemTypeId;
        [Tooltip("捡起时播放")]
        public AudioClip pickupSfx;
        [Tooltip("掉落时播放")]
        public AudioClip dropSfx;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    private Dictionary<int, Entry> _cache;

    private void BuildCache()
    {
        if (_cache != null) return;
        _cache = new Dictionary<int, Entry>();
        foreach (var e in entries)
        {
            if (e != null && !_cache.ContainsKey(e.itemTypeId))
                _cache[e.itemTypeId] = e;
        }
    }

    public AudioClip GetPickupSfx(int itemTypeId)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(itemTypeId, out var e) ? e.pickupSfx : null;
    }

    public AudioClip GetDropSfx(int itemTypeId)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(itemTypeId, out var e) ? e.dropSfx : null;
    }
}
