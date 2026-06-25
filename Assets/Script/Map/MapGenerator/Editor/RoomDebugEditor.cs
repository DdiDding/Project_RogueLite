using System.Reflection;
using UnityEditor;
using UnityEngine;

/**
 * @class RoomDebugEditor
 * @brief RoomDebug를 Inspecter에서 버튼형식으로 제어할 수 있게 하는 에디터 클래스
 */
[CustomEditor(typeof(RoomDebug))]
public class RoomDebugEditor : Editor
{
    /**
     * @brief RoomDebug의 Inspector UI를 그린다.
     *
     * @details
     * 기본 Inspector를 먼저 출력한 뒤, 디버그용 제어 버튼을 추가한다.
     * 각 버튼은 RoomDebug에 정의된 메서드를 Reflection으로 찾아 실행한다.
     */
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

    /**
     * @brief RoomDebug의 특정 메서드를 실행하는 Inspector 버튼을 그린다.
     *
     * @details
     * methodName에 해당하는 인스턴스 메서드를 Reflection으로 검색한다.
     * public 메서드와 private 메서드를 모두 찾기 위해
     * BindingFlags.Public과 BindingFlags.NonPublic을 함께 사용한다.
     *
     * 메서드가 존재하지 않으면 버튼을 비활성화하고 HelpBox를 표시한다.
     * 버튼을 누르면 대상 메서드를 호출한 뒤, Inspector와 Scene View가 변경 사항을
     * 다시 반영할 수 있도록 SetDirty()와 RepaintAll()을 호출한다.
     *
     * 문자열 기반 Reflection은 메서드명 변경에 취약하므로,
     * 버튼 이름과 실제 메서드명을 수정할 때 함께 확인해야 한다.
     *
     * @param roomDebug 메서드를 호출할 RoomDebug 인스턴스.
     * @param label Inspector에 표시할 버튼 이름.
     * @param methodName 호출할 RoomDebug 메서드 이름.
     * @param parameters 메서드 호출 시 전달할 인자 목록.
     */
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
