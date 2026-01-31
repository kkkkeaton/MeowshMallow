using UnityEngine;

/// <summary>游戏启动入口：常驻场景，并实例化 God 管理器且一并常驻。</summary>
public class Launcher : MonoBehaviour
{
    [SerializeField] private GameObject godManagerObj;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (godManagerObj == null) return;

        var instance = Instantiate(godManagerObj);
        instance.name = "GodManager";
        DontDestroyOnLoad(instance);
    }

    private void Start()
    {
        var gameProcess = God.Instance?.Get<GameProcessManager>();
        if (gameProcess != null)
            gameProcess.StartGame();
    }
}
