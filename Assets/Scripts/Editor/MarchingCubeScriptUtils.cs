using UnityEngine;
using System.Collections;
using UnityEditor;
using MarchingCube;

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
