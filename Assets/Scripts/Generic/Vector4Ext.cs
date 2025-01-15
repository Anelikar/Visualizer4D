using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An extention class that provides a method to calculate cross product between two vectors
/// </summary>
public static class Vector4Ext
{
    /// <summary>
    /// Calculates cross product between two vectors
    /// </summary>
    /// <returns>Cross product between vectorA and vectorB</returns>
    public static Vector4 CrossProduct(Vector4 vectorA, Vector4 vectorB) {

        float magA = vectorA.magnitude;
        float magB = vectorB.magnitude;
        float angle = 0;
        Vector4 perpendicular = new Vector4();
        return magA * magB * Mathf.Sin(angle) * perpendicular;
    }
    /// <summary>
    /// Calculates cross product between this and another vector
    /// </summary>
    /// <returns>Cross product between this and vectorB</returns>
    public static Vector4 Cross(this Vector4 vectorA, Vector4 vectorB) {
        return CrossProduct(vectorA, vectorB);
    }
}
