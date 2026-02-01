using UnityEngine;

[CreateAssetMenu(fileName = "Composable", menuName = "Scriptable Objects/Composable")]
public class Composable : ScriptableObject
{
    public int id;
    public string name;
    public string description;
    public GameObject prefab;

    public TopoComponent topoComponent;

    [Tooltip("物品类型 ID，用于按类型播放捡起/掉落音效")]
    public int itemTypeId;
}
