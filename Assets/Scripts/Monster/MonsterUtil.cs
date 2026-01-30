using UnityEngine;

/// <summary>怪物相关静态工具：从 Resources 加载配置、按 ID 取 Prefab 等。</summary>
public static class MonsterUtil
{
    private const string DefaultConfigPath = "MonsterConfig"; // 需将 MonsterConfig.asset 放在任意 Resources 文件夹下

    /// <summary>从 Resources 加载 MonsterConfig；不传路径时使用默认名 "MonsterConfig"。</summary>
    public static MonsterConfig LoadConfig(string pathInResources = null)
    {
        var path = string.IsNullOrEmpty(pathInResources) ? DefaultConfigPath : pathInResources;
        return Resources.Load<MonsterConfig>(path);
    }

    /// <summary>根据配置与怪物 ID 获取预制体，封装 config.GetPrefab(id)。</summary>
    public static GameObject GetPrefab(MonsterConfig config, string id)
    {
        return config != null ? config.GetPrefab(id) : null;
    }
}
