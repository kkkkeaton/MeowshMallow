using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        Debug.Log("test Start");
        var spawnPos = new Vector2(0, 0);
        var monsterManager = FindObjectOfType<MonsterManager>();
        if (monsterManager == null)
        {
            Debug.LogError("MonsterManager not found");
            return;
        }
        var monster = monsterManager.SpawnMonster("1001", spawnPos);
        if (monster != null)
        {
            var ai = monster.GetComponent<MonsterAI>();
            if (ai != null)
                ai.SetDebugLog(true);
            Debug.Log($"[TestDev] 生成怪物 id= 于 {spawnPos}");
        }
        else
            Debug.LogWarning($"[TestDev] 生成失败");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
