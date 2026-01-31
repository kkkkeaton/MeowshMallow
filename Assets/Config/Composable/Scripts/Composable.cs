using UnityEngine;

[CreateAssetMenu(fileName = "Composable", menuName = "Scriptable Objects/Composable")]
public class Composable : ScriptableObject
{
    public int id;
    public string name;
    public string description;
    public GameObject prefab;

    public TopoComponent topoComponent;
}
