using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomDebug))]
public class RoomDebugEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Room Debug Controls", EditorStyles.boldLabel);

        RoomDebug roomDebug = (RoomDebug)target;


        DrawActionButton(roomDebug, "Generate Rooms", "generateRooms");
        DrawActionButton(roomDebug, "Step Once", "stepOnce");
        DrawActionButton(roomDebug, "Start Auto Resolve", "startAutoResolve");
        DrawActionButton(roomDebug, "Stop Auto Resolve", "stopAutoResolve");
        DrawActionButton(roomDebug, "Clear Rooms", "ClearRooms");
    }

    private static void DrawActionButton(RoomDebug roomDebug, string label, string methodName, params object[] parameters)
    {
        MethodInfo method = typeof(RoomDebug).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        bool hasMethod = method != null;

        using (new EditorGUI.DisabledScope(!hasMethod))
        {
            if (GUILayout.Button(label))
            {
                method.Invoke(roomDebug, parameters);
                EditorUtility.SetDirty(roomDebug);
                SceneView.RepaintAll();
            }
        }

        if (!hasMethod)
        {
            EditorGUILayout.HelpBox($"RoomDebug.{methodName}() not found.", MessageType.Info);
        }
    }
}
