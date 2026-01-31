using UnityEngine;
using System.Collections.Generic;       
using System;

public class ComposableManager : MonoBehaviour
{
    Player player;
    int _genId = 1;

    string playerTopoString = "";

    [SerializeField] AllComposableList allComposableList;

    Dictionary<int, Composable> composableConfigDictCache = new Dictionary<int, Composable>();
    Dictionary<int, ComposableMono> composableMonoDict = new Dictionary<int, ComposableMono>();

    List<ComposableMono> playerComposableList = new List<ComposableMono>();
    public void Awake()
    {
        if (God.Instance != null)
            God.Instance.Add(this);

        player = FindFirstObjectByType<Player>();
    }

    private void GenerateItemByTypeId(int id, Action<GameObject> onGenerated)
    {
        if (allComposableList == null)
            allComposableList = Resources.Load<AllComposableList>("AllComposableList");
        if (allComposableList == null || allComposableList.composables == null)
        {
            onGenerated?.Invoke(null);
            return;
        }
        Composable composable;
        composableConfigDictCache.TryGetValue(id, out composable);
        if (composable == null)
        {
            composable = allComposableList.composables.Find(c => c != null && c.id == id);
            if (composable != null)
                composableConfigDictCache[id] = composable;
        }
        if (composable == null || composable.prefab == null)
        {
            onGenerated?.Invoke(null);
            return;
        }
        var obj = GameObject.Instantiate(composable.prefab);
        var composableMono = obj.GetComponent<ComposableMono>();
        int genId = _genId++;
        if (composableMono != null)
        {
            composableMono.Init(composable, genId,pos,rot);
            composableMonoDict[genId] = composableMono;
        }
        onGenerated?.Invoke(obj);
    }



    //pos 0，1之间，rot，0到360之间    
    //将生成的Composable添加到玩家的挂载点上
    public void GenPlayerNewComposable(Composable composableConfig,Vector2 pos,float rot)
    {
        GenerateItemByTypeId(composableConfig.id, (obj) =>
        {
            if (obj != null)
            {
                obj.transform.SetParent(player.composableParent);
                obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
                obj.transform.localRotation = Quaternion.Euler(0, 0, rot);

                var ele = new Element("");
                ele.type = composableConfig.topoComponent.typeId;
                ele.pos = pos;
                ele.rot = rot;
                player.AddOneElement2Main(ele);
                var str = "";
                str += "类型：" + ele.type + "-" + "位置：" + ele.pos.x + "," + ele.pos.y + "-" + "旋转：" + ele.rot;
                Debug.Log("添加Composable成功,genId:" + genId + "\n信息:\n" + str);
            }
        });
    }

    public void DeletePlayerComposable(int genId)
    {
        var composableMono = composableMonoDict[genId];
        Composable composable;
        Vector2 pos;
        float rot;
        composableMono.GetOriginalConfig(out composable, out pos, out rot);
        bool isSuccess = player.TryRemoveOneElementFromMain(composable.topoComponent.typeId, pos, rot);
        if (!isSuccess)
        {
            Debug.LogError("删除Composable失败,genId:" + genId);
            return;
        }
        if (composableMono != null)
        {
            composableMonoDict.Remove(genId);
            GameObject.Destroy(composableMono.gameObject);
            Debug.Log("删除Composable成功,genId:" + genId);
        }
    }



}
