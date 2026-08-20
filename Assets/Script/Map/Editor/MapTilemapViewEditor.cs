using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(MapTilemapView))]
public class MapTilemapViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapTilemapView mapTilemapView = (MapTilemapView)target;

        if (GUILayout.Button("Load Map"))
        {
            mapTilemapView.LoadMap();
            SceneView.RepaintAll();

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(mapTilemapView.gameObject.scene);
            }
        }
    }
}
