using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A constructor for vertices and cells of the 5-cell shape. A 5-cell is basically a 4D tetrahedron
/// </summary>
public class Cell5 : Model4D
{
    /// <summary>
    /// Sets vertices of the 5-cell with a side length 1, starting on [0,0,0,0] and going into a positive direction on all axis
    /// </summary>
    void setVertices() {
        vertices = new Vector4[] {
            new Vector4(0, 0, 0, 0),
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1),
        };
    }

    /// <summary>
    /// Sets vertices of the 5-cell with a side length 1 in such a way that it is centered around 0
    /// </summary>
    void setVerticesCentered() {
        // Pre-calculating common positions
        float sqrt10 = 1 / Mathf.Sqrt(10);
        float sqrt6 = 1 / Mathf.Sqrt(6);
        float sqrt3 = 1 / Mathf.Sqrt(3);

        // The vertices are the same as in setVertices(), just shifted in a way that the resulting 5-cell is centered around 0
        vertices = new Vector4[] {
            new Vector4(sqrt10, sqrt6, sqrt3, 1),
            new Vector4(sqrt10, sqrt6, sqrt3, -1),
            new Vector4(sqrt10, sqrt6, -2 * sqrt3, 0),
            new Vector4(sqrt10, -1 * Mathf.Sqrt(3 / 2f), 0, 0),
            new Vector4(-2 * Mathf.Sqrt(2 / 5f), 0, 0, 0),
        };
    }

    /// <summary>
    /// Sets the tetrahedral cells of a 5-cell shape
    /// </summary>
    void setCells() {
        cells = new int[] {
                0, 1, 2, 3,
                0, 2, 3, 4,
                0, 1, 3, 4,
                0, 1, 2, 4,
                1, 2, 3, 4,
            };
    }

    public Cell5(bool centered = true) {
        if (centered)
            setVerticesCentered();
        else
            setVertices();

        setCells();
    }
}
