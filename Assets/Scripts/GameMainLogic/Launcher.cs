using UnityEngine;

/// <summary>游戏启动入口：常驻场景，并实例化 God 管理器且一并常驻。</summary>
public class Launcher : MonoBehaviour
{
    [SerializeField] private GameObject godManagerObj;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (godManagerObj == null)
        {
            Debug.LogError("[Launcher] 未指定 GodManager 预制体（godManagerObj），God/AudioManager/GameProcessManager 等将不会创建，游戏无法正常进行。请在 Inspector 中拖入 GodManager 预制体。", this);
            return;
        }

        var instance = Instantiate(godManagerObj);
        instance.name = "GodManager";
        DontDestroyOnLoad(instance);
    }

    private void Start()
    {
    }
}
