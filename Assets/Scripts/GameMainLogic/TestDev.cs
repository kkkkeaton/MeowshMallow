using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("Composable 生成测试")]
    [Tooltip("在检视器输入要生成的 Composable 类型 id，再点击下方按钮生成")]
    [SerializeField] private int testComposableTypeId = 1;

    [Header("GenPlayerNewComposable 测试")]
    [Tooltip("拖入要挂到玩家上的 Composable 配置资产")]
    [SerializeField] private Composable testGenComposableConfig;
    [Tooltip("挂载点位置，建议 0～1 范围内")]
    [SerializeField] private Vector2 testGenComposablePos = Vector2.zero;
    [Tooltip("旋转角，0～360")]
    [SerializeField] private float testGenComposableRot = 0f;

    private MonsterManager monsterManager;
    private ComposableManager composableManager;

    void Start()
    {
        monsterManager = FindFirstObjectByType<MonsterManager>();
        if (monsterManager == null && God.Instance != null)
            monsterManager = God.Instance.Get<MonsterManager>();

        composableManager = FindObjectOfType<ComposableManager>();
        if (composableManager == null && God.Instance != null)
            composableManager = God.Instance.Get<ComposableManager>();

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
        var player = FindFirstObjectByType<Player>();
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

    /// <summary>为 Editor 或反射获取 MonsterManager 提供公共方法。</summary>
    public MonsterManager GetMonsterManager()
    {
        return monsterManager;
    }

    /// <summary>为 Editor 或反射获取 ComposableManager 提供公共方法。</summary>
    public ComposableManager GetComposableManager()
    {
        return composableManager;
    }

    /// <summary>为 Editor 按钮使用的 Composable 类型 id。</summary>
    public int GetTestComposableTypeId() => testComposableTypeId;

    /// <summary>GenPlayerNewComposable 测试用：配置、位置、旋转。</summary>
    public void GetTestGenPlayerNewComposableParams(out Composable config, out Vector2 pos, out float rot)
    {
        config = testGenComposableConfig;
        pos = testGenComposablePos;
        rot = testGenComposableRot;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestDev))]
public class TestDevEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestDev testDev = (TestDev)target;

        if (GUILayout.Button("调用所有怪物的 DebugTopo"))
        {
            var monsterManager = testDev.GetMonsterManager();
            if (monsterManager == null)
            {
                Debug.LogWarning("[TestDevEditor] 未找到 MonsterManager。");
                return;
            }

            var aliveMonsters = monsterManager.GetAliveMonsters();
            foreach (var monster in aliveMonsters)
            {
                var monsterBase = monster.GetComponent<MonsterBase>();
                if (monsterBase != null)
                    monsterBase.DebugTopo();
            }
            Debug.Log($"[TestDevEditor] 已为 {aliveMonsters.Count} 个怪物调用 DebugTopo。");
        }

        if (GUILayout.Button("生成 Composable（使用上方类型 id）"))
        {
            var composableManager = testDev.GetComposableManager() ?? Object.FindObjectOfType<ComposableManager>();
            if (composableManager == null)
            {
                Debug.LogWarning("[TestDevEditor] 未找到 ComposableManager。请确保场景中存在 ComposableManager，或进入 Play 模式后再点按钮。");
                return;
            }
            int typeId = testDev.GetTestComposableTypeId();
            try
            {
                composableManager.GenerateItemByTypeId(typeId, obj =>
                {
                    if (obj != null)
                        Debug.Log($"[TestDevEditor] 已生成 Composable id={typeId}: {obj.name}");
                });
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TestDevEditor] 生成 Composable id={typeId} 失败: {e.Message}。请检查 Resources/AllComposableList 中是否存在该 id 的配置。");
            }
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("测试 GenPlayerNewComposable（使用上方配置/位置/旋转）"))
        {
            var composableManager = testDev.GetComposableManager() ?? Object.FindObjectOfType<ComposableManager>();
            if (composableManager == null)
            {
                Debug.LogWarning("[TestDevEditor] 未找到 ComposableManager。");
                return;
            }
            testDev.GetTestGenPlayerNewComposableParams(out var config, out var pos, out var rot);
            if (config == null)
            {
                Debug.LogWarning("[TestDevEditor] 请先在「GenPlayerNewComposable 测试」中拖入 Composable 配置。");
                return;
            }
            try
            {
                composableManager.GenPlayerNewComposable(config, pos, rot);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TestDevEditor] GenPlayerNewComposable 失败: {e.Message}");
            }
        }
    }
}
#endif
