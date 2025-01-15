using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for 4D models constructors
/// </summary>
public abstract class Model4D
{
    /// <summary>
    /// Vertices of the mesh
    /// </summary>
    public Vector4[] vertices;
    /// <summary>
    /// Tetrahedra that form the mesh
    /// </summary>
    public int[] cells;
}
