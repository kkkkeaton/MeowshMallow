using UnityEngine;

public class Player : MonoBehaviour, IMaskInfoProvider
{
    MaskCore playerMaskCore = new MaskCore();

    [SerializeField] string playerMaskCoreString = "1@1-2@0.662^0.679-3@1-4@229.639-5@180;1@1-2@0.833^0.742-3@1-4@162.48-5@180;1@1-2@0.894^0.751-3@1-4@210.739-5@180;1@2-2@0.77^0.602-3@1-4@111.049-5@180;1@2-2@0.607^0.663-3@1-4@155.775-5@180;1@1-2@0.456^0.409-3@0-4@0;1@2-2@0.847^0.461-3@0-4@0;1@2-2@0.605^0.105-3@0-4@0";

    /// <summary>玩家当前颜色 ID，初始为 1。</summary>
    private int _colorId = 1;

    public PlayerConfig playerConfig;
    public SpriteRenderer mainPartRenderer;

    public Transform ComposableParent;

    [Header("音效")]
    [Tooltip("变色时播放的音效")]
    [SerializeField] private AudioClip changeColorSfx;

    public virtual MaskCore GetMaskInfo()
    {
        return playerMaskCore;
    }

    public void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);
    }

    public void Start()
    {
        playerMaskCore.Parse(playerMaskCoreString);
        _colorId = 1;
        ApplyColorSprite();
    }

    /// <summary>获取当前颜色 ID。</summary>
    public int GetColorId() => _colorId;

    /// <summary>切换玩家颜色。</summary>
    public void ChangeColor(int colorId)
    {
        Debug.Log($"[Player] 切换玩家颜色为: {colorId}");
        _colorId = colorId;
        ApplyColorSprite();
        if (changeColorSfx != null)
            God.Instance?.Get<AudioManager>()?.PlaySfx(changeColorSfx);
    }

    /// <summary>根据当前颜色 ID 从 PlayerConfig 取 Sprite 并应用到 MainPart。</summary>
    private void ApplyColorSprite()
    {
        if (playerConfig == null || mainPartRenderer == null) return;
        var sprite = playerConfig.GetSpriteForColor(_colorId);
        if (sprite != null)
            mainPartRenderer.sprite = sprite;
    }


    public void AddOneElement2Main(Element element)
    {
        playerMaskCore.AddOneElement2Main(element);
    }

    public bool TryRemoveOneElementFromMain(int typeId,Vector2 pos,float rot)
    {
        return playerMaskCore.TryRemoveOneElementFromMain(typeId, pos, rot);
    }

        //调用此函数，自动输出自己的拓扑结构中的内容，要求说清楚自己有几张图，每张图有几个元素，每个元素有哪些属性，每个属性是怎样的，输出到控制台里
    public void DebugTopo()
    {
        if (playerMaskCore == null)
        {
            UnityEngine.Debug.Log("[DebugTopo] 拓扑未初始化");
            return;
        }
        var unit = playerMaskCore.MainMaskUnit;
        if (unit == null)
        {
            Debug.Log("[DebugTopo] MainMaskUnit 为空");
            return;
        }
        string str = "主图（MainMaskUnit）：";
        for (int j = 0; j < unit.ElementCount; j++)
        {
            var ele = unit.GetElement(j);
            str += $"\n\t元素{j + 1}：";
            str += $"\n\t\t类别 {ele.type}";
            str += $"\n\t\t位置 ({ele.pos.x}, {ele.pos.y})";
            str += $"\n\t\t是否考虑旋转角（0或1） {(ele.considerRotFlag ? 1 : 0)}";
            str += $"\n\t\t旋转角（0~360） {ele.rot}";
            var rotListStr = ele.rotList != null ? string.Join(", ", ele.rotList) : "";
            str += $"\n\t\t旋转角相似列表 {rotListStr}";
        }
        Debug.Log(str);
    }
}
