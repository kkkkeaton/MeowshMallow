using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家全局配置：颜色 ID 与 MainPart 显示用 Sprite 的映射，供 Player 切换外观使用。
/// 右键 Create > Scriptable Objects > PlayerConfig 创建，在 Inspector 中配置各颜色对应的 Sprite，再拖到 Player 的配置槽位。
/// </summary>
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [System.Serializable]
    public class ColorSpriteEntry
    {
        [Tooltip("颜色 ID（如 1=默认，2=水源染色）")]
        public int colorId;
        [Tooltip("该颜色下 MainPart 显示的 Sprite")]
        public Sprite sprite;
    }

    [Header("颜色→Sprite")]
    [Tooltip("颜色 ID 与 Sprite 的对应表")]
    [SerializeField] private List<ColorSpriteEntry> colorSprites = new List<ColorSpriteEntry>();

    private Dictionary<int, Sprite> _cache;

    private void BuildCache()
    {
        if (_cache != null) return;
        _cache = new Dictionary<int, Sprite>();
        if (colorSprites == null) return;
        foreach (var e in colorSprites)
        {
            if (e?.sprite != null && !_cache.ContainsKey(e.colorId))
                _cache[e.colorId] = e.sprite;
        }
    }

    /// <summary>根据颜色 ID 获取对应的 Sprite，未配置则返回 null。</summary>
    public Sprite GetSpriteForColor(int colorId)
    {
        BuildCache();
        return _cache != null && _cache.TryGetValue(colorId, out var s) ? s : null;
    }
}
