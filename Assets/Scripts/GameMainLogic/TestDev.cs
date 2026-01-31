using UnityEngine;

/// <summary>开发测试：按 G/H 键生成怪物，验证 MonsterManager、索敌与移动。</summary>
public class TestDev : MonoBehaviour
{
    [Header("怪物生成测试")]
    [SerializeField] private string testMonsterId = "1";
    [Tooltip("按 G 键：在玩家右侧此距离生成（近处，易触发索敌）")]
    [SerializeField] private float spawnOffsetNear = 3f;
    [Tooltip("按 H 键：在玩家右侧此距离生成（远处，需移动玩家进入检测范围才索敌）")]
    [SerializeField] private float spawnOffsetFar = 12f;
    [Tooltip("生成后是否为该怪物的 MonsterAI 开启调试日志（状态切换会打印到 Console）")]
    [SerializeField] private bool enableDebugLogOnSpawn = true;

    private MonsterManager monsterManager;

    void Start()
    {
        monsterManager = FindObjectOfType<MonsterManager>();
        if (monsterManager == null && God.Instance != null)
            monsterManager = God.Instance.Get<MonsterManager>();

        if (monsterManager == null)
            Debug.LogWarning("[TestDev] 未找到 MonsterManager，请确保场景中存在已配置 MonsterConfig 的 MonsterManager。");
        else
            Debug.Log("[TestDev] 索敌与移动测试：按 G 近处生成 / 按 H 远处生成。移动玩家进入怪物检测范围可验证索敌与移动。选中怪物可在 Scene 视图看到检测范围 Gizmos。");
    }

    void Update()
    {
        if (monsterManager == null) return;

        bool spawnNear = Input.GetKeyDown(KeyCode.G);
        bool spawnFar = Input.GetKeyDown(KeyCode.H);
        if (!spawnNear && !spawnFar) return;

        float offset = spawnNear ? spawnOffsetNear : spawnOffsetFar;
        var player = FindObjectOfType<Player>();
        var spawnPos = player != null
            ? (Vector2)player.transform.position + Vector2.right * offset
            : new Vector2(offset, 0f);

        var monster = monsterManager.SpawnMonster(testMonsterId, spawnPos);
        if (monster != null)
        {
            var ai = monster.GetComponent<MonsterAI>();
            if (ai != null && enableDebugLogOnSpawn)
                ai.SetDebugLog(true);
            Debug.Log($"[TestDev] 生成怪物 id={testMonsterId} 于 {spawnPos}（{(spawnNear ? "近" : "远")}），存活数={monsterManager.GetAliveMonsters().Count}。玩家 Tag 须为 \"Player\" 才会被索敌。");
        }
        else
            Debug.LogWarning($"[TestDev] 生成失败，请检查 MonsterConfig 中是否存在 id=\"{testMonsterId}\" 的怪物配置及预制体。");
    }
}
