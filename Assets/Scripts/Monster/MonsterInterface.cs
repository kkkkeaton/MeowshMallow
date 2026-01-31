/// <summary>怪物统一接口：所有怪物逻辑实体（如 MonsterBase）实现此接口，便于 Manager/战斗逻辑统一处理。</summary>
public interface IMonster
{
    /// <summary>怪物类型 ID。</summary>
    string GetId();
    /// <summary>受到伤害，内部扣血并在血量≤0 时触发死亡。</summary>
    void TakeDamage(float damage);
    /// <summary>死亡：触发事件并销毁 GameObject。</summary>
    void Die();
    /// <summary>是否仍存活。</summary>
    bool IsAlive();
}
