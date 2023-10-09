using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MarchingCubes))]
public class MarchingCubeScriptUtils : Editor
{
    void OnSceneGUI()
    {
        MarchingCubes myTarget = (MarchingCubes)target;

        Handles.color = Color.green;
        Handles.DrawWireCube(myTarget.transform.position, myTarget.BoundingBox);

    }
}
