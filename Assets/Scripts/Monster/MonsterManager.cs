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

    /// <summary>通知所有存活的怪物重新检测玩家伪装（当玩家放置部件或改变颜色时由 ComposableManager 等触发）。若重检后范围内无怪识破玩家，则视为伪装成功并调用 EnterDisguiseSuccess。</summary>
    public void CheckAllMonstersDisguise()
    {
        foreach (var monster in aliveMonsters)
        {
            if (monster == null) continue;
            var ai = monster.GetComponent<MonsterAI>();
            ai?.CheckPlayerDisguise();
        }
        var process = God.Instance?.Get<GameProcessManager>();
        if (process != null && process.MonstersSpottingPlayerCount == 0)
            process.EnterDisguiseSuccess();
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

    /// <summary>怪物死亡时由 OnDeath 触发：播放死亡音效、取消订阅并从存活列表中移除；若该怪正在识破玩家，从识破列表移除。</summary>
    private void OnMonsterDeath(IMonster monster)
    {
        if (monster is MonsterBase mb)
        {
            var deathClip = config?.GetDeathSfx(mb.GetId());
            if (deathClip != null)
                God.Instance?.Get<AudioManager>()?.PlaySfx(deathClip);
            mb.OnDeath -= OnMonsterDeath;
            aliveMonsters.Remove(mb);
            God.Instance?.Get<GameProcessManager>()?.UnregisterSpotting(mb);
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

    /// <summary>在指定位置生成一个 PickableItem 掉落物，直接传入 Composable，并按类型播放掉落音效。</summary>
    public void SpawnDrop(Vector2 position, Composable dropComposable)
    {
        if (dropComposable == null || pickableItemDropPrefab == null) return;
        if (pickableItemDropPrefab.GetComponent<PickableItem>() == null) return;

        God.Instance?.Get<AudioManager>()?.PlayDropSfxForItemType(dropComposable.itemTypeId);

        GameObject instance = Instantiate(pickableItemDropPrefab, position, Quaternion.identity, transform);
        var item = instance.GetComponent<PickableItem>();
        if (item != null)
            item.InitWithComposable(dropComposable);
    }

    /// <summary>返回当前存活怪物列表的只读视图。</summary>
    public IReadOnlyList<MonsterBase> GetAliveMonsters() => aliveMonsters;

    /// <summary>清空本局所有存活怪物及本管理器下生成的掉落物（销毁物体并从列表移除）。游戏重新开始时由 GameProcessManager 调用。</summary>
    public void ClearAllMonsters()
    {
        foreach (var m in aliveMonsters)
        {
            if (m != null)
                m.OnDeath -= OnMonsterDeath;
        }
        aliveMonsters.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    public bool CheckIsSameKind<T1, T2>(T1 judger, T2 beJudged, float threshold) where T1 : IMaskInfoJudger where T2 : IMaskInfoProvider
    {
        if (judger == null || beJudged == null) return false;
        var score = judger.JudgeMaskInfo(beJudged.GetMaskInfo());
        Debug.Log($"[MonsterManager] 判断是否同类分数: {score}");
        return score >= threshold;
    }
}
