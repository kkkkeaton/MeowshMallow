using UnityEngine;
using System.Collections.Generic;

/// <summary>怪物管理器：持有配置、调用工厂生成怪物、维护存活列表，并在怪物死亡时从列表中移除。</summary>
public class MonsterManager : MonoBehaviour
{
    [SerializeField] private MonsterConfig config; // 在 Inspector 中拖入 MonsterConfig.asset

    private readonly List<MonsterBase> aliveMonsters = new List<MonsterBase>();

    /// <summary>在指定位置生成指定 ID 的怪物，并加入存活列表、订阅死亡回调。失败返回 null。</summary>
    public MonsterBase SpawnMonster(string id, Vector2 position)
    {
        if (config == null) return null;
        var monster = MonsterFactory.Spawn(config, id, position, transform);
        if (monster == null) return null;

        aliveMonsters.Add(monster);
        monster.OnDeath += OnMonsterDeath;
        return monster;
    }

    /// <summary>怪物死亡时由 OnDeath 触发：取消订阅并从存活列表中移除。</summary>
    private void OnMonsterDeath(IMonster monster)
    {
        if (monster is MonsterBase mb)
        {
            mb.OnDeath -= OnMonsterDeath;
            aliveMonsters.Remove(mb);
        }
    }

    /// <summary>返回当前存活怪物列表的只读视图。</summary>
    public IReadOnlyList<MonsterBase> GetAliveMonsters() => aliveMonsters;
}
