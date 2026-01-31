using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Generator))]
public class GeneratorEditor : Editor
{
    SerializedProperty componentConfigs;
    SerializedProperty spawnTypeId;
    SerializedProperty spawnCount;
    SerializedProperty spawnAreaMin;
    SerializedProperty spawnAreaMax;
    SerializedProperty spawnParent;
    SerializedProperty topoCodeString;

    void OnEnable()
    {
        componentConfigs = serializedObject.FindProperty("componentConfigs");
        spawnTypeId = serializedObject.FindProperty("spawnTypeId");
        spawnCount = serializedObject.FindProperty("spawnCount");
        spawnAreaMin = serializedObject.FindProperty("spawnAreaMin");
        spawnAreaMax = serializedObject.FindProperty("spawnAreaMax");
        spawnParent = serializedObject.FindProperty("spawnParent");
        topoCodeString = serializedObject.FindProperty("topoCodeString");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var gen = (Generator)target;

        EditorGUILayout.LabelField("拓扑配置 (TopoComponent 列表)", EditorStyles.boldLabel);
        int count = componentConfigs.arraySize;
        for (int i = 0; i < count; i++)
        {
            var elem = componentConfigs.GetArrayElementAtIndex(i);
            var so = elem.objectReferenceValue as TopoComponent;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("  [" + i + "]");
            EditorGUILayout.PropertyField(elem, GUIContent.none);
            if (so != null)
            {
                EditorGUILayout.LabelField("typeId: " + so.typeId, GUILayout.Width(72));
                EditorGUILayout.LabelField(so.name, EditorStyles.miniLabel, GUILayout.MinWidth(60));
            }
            else
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        if (GUILayout.Button("+ 添加", GUILayout.Width(56)))
        {
            componentConfigs.arraySize++;
        }
        if (count > 0 && GUILayout.Button("- 移除末尾", GUILayout.Width(80)))
        {
            componentConfigs.arraySize--;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("生成参数", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spawnTypeId);
        EditorGUILayout.PropertyField(spawnCount);
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("生成区域", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spawnAreaMin);
        EditorGUILayout.PropertyField(spawnAreaMax);
        EditorGUILayout.Space(2);
        float sizeX = gen.spawnAreaMax.x - gen.spawnAreaMin.x;
        float sizeY = gen.spawnAreaMax.y - gen.spawnAreaMin.y;
        EditorGUILayout.HelpBox(
            "区域范围（XY 平面，归一化 0~1 对应）: 左下角 = spawnAreaMin，右上角 = spawnAreaMax\n" +
            "X " + gen.spawnAreaMin.x.ToString("F2") + " ~ " + gen.spawnAreaMax.x.ToString("F2") + "  |  Y " + gen.spawnAreaMin.y.ToString("F2") + " ~ " + gen.spawnAreaMax.y.ToString("F2") + "  |  平面 " + sizeX.ToString("F2") + " × " + sizeY.ToString("F2"),
            MessageType.None
        );
        EditorGUILayout.PropertyField(spawnParent);
        EditorGUILayout.Space(4);

        if (GUILayout.Button("生成（按 typeId + 数量）"))
        {
            gen.SpawnByTypeIdAndCount();
        }
        if (GUILayout.Button("一键清空场景中的 Topo 实例"))
        {
            gen.ClearAll();
        }
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("构型图代码", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(topoCodeString, new GUIContent("导入/导出文本框"), GUILayout.MinHeight(60));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("导出构型图（写入上方文本框）"))
        {
            string code = gen.ExportTopoCode();
            topoCodeString.stringValue = code;
        }
        if (GUILayout.Button("导入构型图（清空后按代码生成）"))
        {
            gen.ImportTopoCode();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
