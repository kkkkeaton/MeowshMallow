using UnityEngine;

/// <summary>
/// 地图刷怪点：当玩家初次进入探测范围时，在生成范围内随机位置刷出指定数量的怪。
/// 生成 id、数量、探测范围、生成范围均可配置。需场景中存在 MonsterManager。
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("刷怪配置")]
    [Tooltip("要生成的怪物类型 ID（需在 MonsterConfig 中存在）")]
    [SerializeField] private string monsterId = "1";
    [Tooltip("触发时生成的怪物数量")]
    [SerializeField] private int spawnCount = 3;
    [Tooltip("玩家进入此范围时触发刷怪（仅首次进入）")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("怪物在刷怪点周围此半径内随机位置生成")]
    [SerializeField] private float spawnRange = 4f;

    [Header("可选")]
    [Tooltip("用于查找玩家的 Tag")]
    [SerializeField] private string playerTag = "Player";

    private MonsterManager _monsterManager;
    private Transform _player;
    private bool _hasTriggered;

    private void Start()
    {
        if (string.IsNullOrEmpty(playerTag)) playerTag = "Player";
        TryFindPlayer();
        TryFindMonsterManager();
    }

    /// <summary>尝试查找玩家（地图与玩家异步生成时可能延后出现，会持续重试）。</summary>
    private void TryFindPlayer()
    {
        if (_player != null) return;
        var go = GameObject.FindWithTag(playerTag);
        if (go != null) _player = go.transform;
    }

    /// <summary>尝试查找 MonsterManager。</summary>
    private void TryFindMonsterManager()
    {
        if (_monsterManager != null) return;
        _monsterManager = FindObjectOfType<MonsterManager>();
        if (_monsterManager == null && God.Instance != null)
            _monsterManager = God.Instance.Get<MonsterManager>();
    }

    private void Update()
    {
        if (_player == null) TryFindPlayer();
        if (_monsterManager == null) TryFindMonsterManager();

        if (_hasTriggered || _player == null || _monsterManager == null) return;

        Vector2 myPos = transform.position;
        Vector2 playerPos = _player.position;
        float dist = Vector2.Distance(myPos, playerPos);
        if (dist > detectionRange) return;

        _hasTriggered = true;
        SpawnMonsters();
    }

    /// <summary>在生成范围内随机位置刷出配置数量的怪。</summary>
    private void SpawnMonsters()
    {
        Vector2 center = transform.position;
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRange;
            Vector2 pos = center + offset;
            var monster = _monsterManager.SpawnMonster(monsterId, pos);
            if (monster == null)
                Debug.LogWarning($"[SpawnPoint] {gameObject.name} 生成怪物 id=\"{monsterId}\" 失败，请检查 MonsterConfig。");
        }
    }

    /// <summary>重置触发状态，使玩家再次进入探测范围时可再次刷怪（用于测试或关卡重置）。</summary>
    public void ResetTrigger() => _hasTriggered = false;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        center.z = 0f;
        Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(center, detectionRange);
        UnityEditor.Handles.Label(center + Vector3.up * (detectionRange + 0.5f), $"探测 {detectionRange:F0}");
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(center, spawnRange);
        UnityEditor.Handles.Label(center + Vector3.down * (spawnRange + 0.5f), $"生成 {spawnRange:F0} x{spawnCount}");
    }
#endif
}
