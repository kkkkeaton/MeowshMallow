using UnityEngine;

/// <summary>怪物工厂：根据配置与 ID 实例化预制体并注入属性，不持有状态。</summary>
public static class MonsterFactory
{
    /// <summary>在指定位置生成指定 ID 的怪物；parent 可选，不传则生成在根下。返回 MonsterBase 组件，失败返回 null。</summary>
    public static MonsterBase Spawn(MonsterConfig config, string id, Vector2 position, Transform parent = null)
    {
        if (config == null) return null;
        var prefab = config.GetPrefab(id);
        if (prefab == null) return null;

        var pos = new Vector3(position.x, position.y, 0f);
        var instance = parent != null
            ? Object.Instantiate(prefab, pos, Quaternion.identity, parent)
            : Object.Instantiate(prefab, pos, Quaternion.identity);

        var monster = instance.GetComponent<MonsterBase>();
        Debug.Log($"MonsterFactory: {monster}");
        if (monster != null)
        {
            var maxHp = config.GetMaxHp(id);
            var moveSpeed = config.GetMoveSpeed(id);    
            var monserTopoConfig = config.GetTopoConfig(id);
            monster.Init(id, maxHp, moveSpeed, monserTopoConfig);
        }       

        return monster;
    }
}
