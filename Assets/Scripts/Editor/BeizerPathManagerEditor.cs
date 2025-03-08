using PathTool;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeizerPathManager))]
public class BeizerPathManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        BeizerPathManager manager = (BeizerPathManager)target;

        for (int i = 0; i < manager.controlPoints.Count; i++)
        {
            // Draw a position handle for each control point
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(manager.controlPoints[i].position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager.controlPoints[i], "Move Control Point");
                manager.controlPoints[i].position = newPosition;
                manager.UpdateCurve();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BeizerPathManager manager = (BeizerPathManager)target;

        if (GUILayout.Button("Add Control Point"))
        {
            Undo.RecordObject(manager, "Add Control Point");
            Vector3 newPointPosition = manager.controlPoints.Count > 0
                ? manager.controlPoints[manager.controlPoints.Count - 1].position + Vector3.forward * 5f
                : manager.transform.position;
            manager.AddControlPoint(newPointPosition);
        }

        if (GUILayout.Button("Remove Last Control Point"))
        {
            Undo.RecordObject(manager, "Remove Last Control Point");
            manager.RemoveLastControlPoint();
        }
        if(GUILayout.Button("Reset Line Renderer"))
        {
            Undo.RecordObject(manager, "Reset Line Renderer");
            manager.RemoveAllControlPoints();
        }
        /*if (GUILayout.Button("Generate Platforms"))
        {
            Undo.RecordObject(manager, "Generate Platforms");
            manager.GeneratePlatforms();
        }*/
    }
}
