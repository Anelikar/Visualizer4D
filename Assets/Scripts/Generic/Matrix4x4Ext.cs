using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An extention class providing methods to create matrices that rotate a vector in a given plane
/// </summary>
public static class Matrix4x4Ext
{
    /// <summary>
    /// Creates a matrix to rotate a vector in XY plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateXY(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, -s, 0, 0));
        m.SetColumn(1, new Vector4(s, c, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in XY plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateXY(this Matrix4x4 m, float a) {
        return m * RotateXY(a);
    }

    /// <summary>
    /// Creates a matrix to rotate a vector in YZ plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateYZ(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, c, -s, 0));
        m.SetColumn(2, new Vector4(0, s, c, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in YZ plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateYZ(this Matrix4x4 m, float a) {
        return m * RotateYZ(a);
    }

    /// <summary>
    /// Creates a matrix to rotate a vector in XZ plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateXZ(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, 0, s, 0));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(-s, 0, c, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in XZ plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateXZ(this Matrix4x4 m, float a) {
        return m * RotateXZ(a);
    }

    /// <summary>
    /// Creates a matrix to rotate a vector in XW plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateXW(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, 0, 0, -s));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(s, 0, 0, c));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in XW plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateXW(this Matrix4x4 m, float a) {
        return m * RotateXW(a);
    }

    /// <summary>
    /// Creates a matrix to rotate a vector in YW plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateYW(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, c, 0, s));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(0, -s, 0, c));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in YW plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateYW(this Matrix4x4 m, float a) {
        return m * RotateYW(a);
    }

    /// <summary>
    /// Creates a matrix to rotate a vector in ZW plane by a
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>Matrix to rotate a vector</returns>
    public static Matrix4x4 RotateZW(float a) {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, c, s));
        m.SetColumn(3, new Vector4(0, 0, -s, c));
        return m;
    }
    /// <summary>
    /// Multiplies this matrix by a rotation matrix in ZW plane
    /// </summary>
    /// <param name="a">Angle of rotation in radians</param>
    /// <returns>This matrix multiplied by a rotation matrix</returns>
    public static Matrix4x4 RotateZW(this Matrix4x4 m, float a) {
        return m * RotateZW(a);
    }
}
