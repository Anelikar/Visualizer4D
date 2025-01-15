using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds a "Build mesh" button to a ShapeConstructor
/// </summary>
[CustomEditor(typeof(ShapeConstructor))]
public class ShapeConstructorEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        ShapeConstructor constructor = (ShapeConstructor)target;
        if (GUILayout.Button("Build mesh")) {
            constructor.BuildMesh();
        }
    }
}
