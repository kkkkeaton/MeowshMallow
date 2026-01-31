using UnityEngine;
using System.Collections.Generic;

/// <summary>全局单例，集中注册与获取各管理类。通过 Add/Get 管理单例。</summary>
public class God : MonoBehaviour
{
    private static God _instance;
    private readonly Dictionary<System.Type, object> _managers = new Dictionary<System.Type, object>();

    public static God Instance => _instance;

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
