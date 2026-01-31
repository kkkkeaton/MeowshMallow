using UnityEngine;
using System;

/// <summary>2D 怪物实体：挂在与 Prefab 同级的 GameObject 上，实现 IMonster，负责血量、受击、死亡与死亡事件。</summary>
public class MonsterBase : MonoBehaviour, IMonster,IMaskInfoAgent
{
    [SerializeField] private string monsterId;   // 怪物类型 ID（可由工厂 Init 注入）
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float moveSpeed = 1f;

    private MaskCore _maskCore; 

    private float currentHp;  // 当前血量
    private bool alive = true;

    /// <summary>死亡时触发，参数为自身 IMonster，供 MonsterManager 等订阅并做清理/统计。</summary>
    public event Action<IMonster> OnDeath;

    public string GetId() => monsterId;
    public bool IsAlive() => alive;

    /// <summary>由工厂在生成后调用，注入配置中的 id、maxHp、moveSpeed。</summary>
    public void Init(string id, float hp, float speed, string topo)
    {
        monsterId = id;
        maxHp = hp;
        moveSpeed = speed;
        currentHp = maxHp;
        _maskCore ??= new MaskCore();
        _maskCore.Parse(topo);
        alive = true;
    }

    public void TakeDamage(float damage)
    {
        if (!alive) return;
        currentHp -= damage;
        if (currentHp <= 0f)
            Die();
    }

    public void Die()
    {
        if (!alive) return;
        alive = false;
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }


    public virtual MaskCore GetMaskInfo()
    {
        return _maskCore;
    }


    public virtual float JudgeMaskInfo(MaskCore judgeMask)
    {
        var temp = GetMaskInfo();
        return temp.Compare(judgeMask);
    }

    //调用此函数，自动输出自己的拓扑结构中的内容，要求说清楚自己有几张图，每张图有几个元素，每个元素有哪些属性，每个属性是怎样的，输出到控制台里
    public void DebugTopo()
    {
        if (_maskCore == null)
        {
            UnityEngine.Debug.Log("[DebugTopo] 拓扑未初始化");
            return;
        }
        for (int i = 0; i < _maskCore.UnitCount; i++)
        {
            UnityEngine.Debug.Log($"图{i + 1}：");
            var unit = _maskCore.GetUnit(i);
            for (int j = 0; j < unit.ElementCount; j++)
            {
                var ele = unit.GetElement(j);
                UnityEngine.Debug.Log($"\t元素{j + 1}：");
                UnityEngine.Debug.Log($"\t\t类别 {ele.type}");
                UnityEngine.Debug.Log($"\t\t位置 ({ele.pos.x}, {ele.pos.y})");
                UnityEngine.Debug.Log($"\t\t是否考虑旋转角（0或1） {(ele.considerRotFlag ? 1 : 0)}");
                UnityEngine.Debug.Log($"\t\t旋转角（0~360） {ele.rot}");
                var rotListStr = ele.rotList != null ? string.Join(", ", ele.rotList) : "";
                UnityEngine.Debug.Log($"\t\t旋转角相似列表 {rotListStr}");
            }
        }
    }

}
