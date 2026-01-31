using UnityEngine;
using System.Collections.Generic;

/// <summary>全局单例，集中注册与获取各管理类。通过 Add/Get 管理单例。</summary>
public class God : MonoBehaviour
{
    private static God _instance;
    private readonly Dictionary<System.Type, object> _managers = new Dictionary<System.Type, object>();
    private GameObject _player;

    public static God Instance => _instance;

    /// <summary>当前玩家物体。由 GameProcessManager 在 StartGame 时设置，未开始或未创建时为 null。其他脚本通过 God.Instance.Player 获取。</summary>
    public GameObject Player => _player;

    /// <summary>设置当前玩家物体（内部使用，由 GameProcessManager 在创建/替换玩家时调用）。</summary>
    public void SetPlayer(GameObject player) => _player = player;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>注册管理类，按类型存储。重复注册会覆盖。</summary>
    public void Add<T>(T manager) where T : class
    {
        if (manager != null)
            _managers[typeof(T)] = manager;
    }

    /// <summary>按类型获取已注册的管理类，未注册返回 null。</summary>
    public T Get<T>() where T : class
    {
        return _managers.TryGetValue(typeof(T), out var obj) ? obj as T : null;
    }
}
