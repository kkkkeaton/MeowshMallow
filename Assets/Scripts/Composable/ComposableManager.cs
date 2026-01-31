using UnityEngine;
using System.Collections.Generic;       
using System;

public class ComposableManager : MonoBehaviour
{
    Player player;
    int _genId = 1;

    string playerTopoString = "";

    MaskCore playerMaskCore;

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
            composableMono.Init(composable, genId);
            composableMonoDict[genId] = composableMono;
        }
        onGenerated?.Invoke(obj);
    }



    //pos 0，1之间，rot，0到360之间    
    public void GenPlayerNewComposable(Composable composableConfig,Vector2 pos,float rot)
    {

    }

    public void DeletePlayerComposable(int genId)
    {
        var composableMono = composableMonoDict[genId];
        if (composableMono != null)
        {
            composableMonoDict.Remove(genId);
            GameObject.Destroy(composableMono.gameObject);
        }
    }



}
