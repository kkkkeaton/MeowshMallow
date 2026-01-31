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
    public void Init(string id, float hp, float speed)
    {
        monsterId = id;
        maxHp = hp;
        moveSpeed = speed;
        currentHp = maxHp;
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

}
