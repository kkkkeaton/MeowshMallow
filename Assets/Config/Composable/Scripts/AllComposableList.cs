using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllComposableList", menuName = "Scriptable Objects/AllComposableList")]
public class AllComposableList : ScriptableObject
{
    public List<Composable> composables;
}
