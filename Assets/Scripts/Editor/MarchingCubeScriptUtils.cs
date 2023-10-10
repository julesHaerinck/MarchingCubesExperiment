using UnityEngine;
using System.Collections;
using UnityEditor;
using MarchingCube;

[CustomEditor(typeof(MarchingCubesGeneration))]
public class MarchingCubeScriptUtils : Editor
{
    void OnSceneGUI()
    {
        MarchingCubesGeneration myTarget = (MarchingCubesGeneration)target;

        Handles.color = Color.green;
        Handles.DrawWireCube(myTarget.transform.position, myTarget.BoundingBox);

    }
}
