using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>场景总控单例，可通过 God.Instance 访问；提供按类型注册与获取服务的服务定位。</summary>
public class God : MonoBehaviour
{
    private static God _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>单例实例；若场景中不存在则返回 null。</summary>
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

    /// <summary>注册服务，以类型 T 为键；同一类型重复注册会覆盖。</summary>
    public void Register<T>(T service) where T : class
    {
        if (service == null) return;
        _services[typeof(T)] = service;
    }

    /// <summary>按类型获取已注册的服务，未注册返回 null。</summary>
    public T GetService<T>() where T : class
    {
        return _services.TryGetValue(typeof(T), out var obj) ? obj as T : null;
    }

    /// <summary>尝试获取服务，返回是否已注册。</summary>
    public bool TryGetService<T>(out T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj) && obj is T t)
        {
            service = t;
            return true;
        }
        service = null;
        return false;
    }
}
