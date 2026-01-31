using UnityEngine;
using System.Collections.Generic;

/// <summary>怪物管理器：持有配置、调用工厂生成怪物、维护存活列表，并在怪物死亡时从列表中移除；支持暗杀与掉落。</summary>
public class MonsterManager : MonoBehaviour
{
    [SerializeField] private MonsterConfig config; // 在 Inspector 中拖入 MonsterConfig.asset
    [Tooltip("掉落物 PickableItem 预制体（需挂 PickableItem，Composable 可空，运行时由 InitWithComposable 注入）")]
    [SerializeField] private GameObject pickableItemDropPrefab;

    private readonly List<MonsterBase> aliveMonsters = new List<MonsterBase>();

    private void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);
    }

    /// <summary>在指定位置生成指定 ID 的怪物，并加入存活列表、订阅死亡回调。失败返回 null。</summary>
    public MonsterBase SpawnMonster(string id, Vector2 position)
    {
        if (config == null) return null;
        var monster = MonsterFactory.Spawn(config, id, position, transform);
        if (monster == null) return null;

        aliveMonsters.Add(monster);
        monster.OnDeath += OnMonsterDeath;
        var ai = monster.GetComponent<MonsterAI>();
        if (ai != null)
            ai.SetConfig(config);
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

    /// <summary>暗杀指定怪物：立即死亡并在其位置掉落 PickableItem（若该怪物配置了 dropComposable）。</summary>
    public void Assassinate(MonsterBase monster)
    {
        if (monster == null || !monster.IsAlive() || config == null) return;

        string id = monster.GetId();
        Vector2 position = monster.transform.position;
        Composable dropComposable = config.GetDropComposable(id);

        monster.Die();

        if (dropComposable != null)
            SpawnDrop(position, dropComposable);
    }

    /// <summary>在指定位置生成一个 PickableItem 掉落物，直接传入 Composable。</summary>
    public void SpawnDrop(Vector2 position, Composable dropComposable)
    {
        if (dropComposable == null || pickableItemDropPrefab == null) return;
        if (pickableItemDropPrefab.GetComponent<PickableItem>() == null) return;

        GameObject instance = Instantiate(pickableItemDropPrefab, position, Quaternion.identity, transform);
        var item = instance.GetComponent<PickableItem>();
        if (item != null)
            item.InitWithComposable(dropComposable);
    }

    /// <summary>返回当前存活怪物列表的只读视图。</summary>
    public IReadOnlyList<MonsterBase> GetAliveMonsters() => aliveMonsters;

    public bool CheckIsSameKind<T1, T2>(T1 judger, T2 beJudged, float threshold) where T1 : IMaskInfoJudger where T2 : IMaskInfoProvider
    {
        if (judger == null || beJudged == null) return false;
        var score = judger.JudgeMaskInfo(beJudged.GetMaskInfo());
        return score >= threshold;
    }
}
