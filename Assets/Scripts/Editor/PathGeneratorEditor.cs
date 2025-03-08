using PathTool;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathGenerator))]
public class PathGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PathGenerator pathGenerator = (PathGenerator)target;
        if (GUILayout.Button("Generate Path"))
        {
            pathGenerator.GenerateSpline();

            EditorUtility.SetDirty(pathGenerator);
        }
        if(GUILayout.Button("Generate Platforms"))
        {
            pathGenerator.GeneratePlatform();
            EditorUtility.SetDirty(pathGenerator);
        }
    }
}
