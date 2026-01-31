using UnityEngine;

/// <summary>
/// 挂在 Player 上：检测拾取范围内有无 PickableItem、攻击范围内有无敌人（MonsterBase）。
/// 拾取范围与攻击范围取自 GlobalSetting。
/// </summary>
public class PlayerRangeDetector : MonoBehaviour
{
    private MonsterManager _monsterManager;
    private UIManager _uiManager;

    private void Awake()
    {
        if (God.Instance != null)
        {
            _monsterManager = God.Instance.Get<MonsterManager>();
            _uiManager = God.Instance.Get<UIManager>();
        }
        if (_monsterManager == null)
            _monsterManager = FindObjectOfType<MonsterManager>();
        if (_uiManager == null)
            _uiManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (_uiManager == null) return;
        _uiManager.SetPickableHintVisible(HasPickableInRange());
        _uiManager.SetAssassinationHintVisible(HasEnemyInAttackRange());
    }

    /// <summary>拾取范围内是否存在任意 PickableItem。</summary>
    public bool HasPickableInRange()
    {
        return GetClosestPickableInRange() != null;
    }

    /// <summary>返回拾取范围内最近的 PickableItem，无则返回 null。</summary>
    public PickableItem GetClosestPickableInRange()
    {
        float range = GlobalSetting.PICKUP_RANGE;
        if (range <= 0f) return null;

        Vector2 pos = transform.position;
        PickableItem[] items = FindObjectsOfType<PickableItem>();
        PickableItem closest = null;
        float closestSq = range * range;

        foreach (var item in items)
        {
            if (item == null) continue;
            float sqDist = (pos - (Vector2)item.transform.position).sqrMagnitude;
            if (sqDist <= closestSq)
            {
                closestSq = sqDist;
                closest = item;
            }
        }
        return closest;
    }

    /// <summary>攻击范围内是否存在任意存活敌人（MonsterBase）。</summary>
    public bool HasEnemyInAttackRange()
    {
        return GetClosestEnemyInAttackRange() != null;
    }

    /// <summary>返回攻击范围内最近的存活敌人（MonsterBase），无则返回 null。</summary>
    public MonsterBase GetClosestEnemyInAttackRange()
    {
        float range = GlobalSetting.ATTACK_RANGE;
        if (range <= 0f || _monsterManager == null) return null;

        Vector2 pos = transform.position;
        var alive = _monsterManager.GetAliveMonsters();
        MonsterBase closest = null;
        float closestSq = range * range;

        foreach (var monster in alive)
        {
            if (monster == null || !monster.IsAlive()) continue;
            float sqDist = (pos - (Vector2)monster.transform.position).sqrMagnitude;
            if (sqDist <= closestSq)
            {
                closestSq = sqDist;
                closest = monster;
            }
        }
        return closest;
    }
}
