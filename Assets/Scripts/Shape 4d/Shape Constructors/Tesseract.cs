using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A constructor for vertices and cells of the tesseract. A tesseract is basically a 4D cube
/// </summary>
public class Tesseract : Model4D
{
    const int cubes_count = 8;
    const int cells_in_cube = 5;
    const int verts_in_cell = 4;
    const int verts_in_cube = 8;

    /// <summary>
    /// Sets vertices of the tesseract with a side length 1, starting on [0,0,0,0] and going into a positive direction on all axis
    /// </summary>
    void setVertices() {
        vertices = new Vector4[] {
            // Each block of 4 is the square on the XY plane shifted by some axis
            // Unshifted
            new Vector4(0, 0, 0, 0),
            new Vector4(1, 0, 0, 0),
            new Vector4(1, 1, 0, 0),
            new Vector4(0, 1, 0, 0),
            // Shifted into Z+1
            new Vector4(0, 0, 1, 0),
            new Vector4(1, 0, 1, 0),
            new Vector4(1, 1, 1, 0),
            new Vector4(0, 1, 1, 0),
            // Shifted into W+1
            new Vector4(0, 0, 0, 1),
            new Vector4(1, 0, 0, 1),
            new Vector4(1, 1, 0, 1),
            new Vector4(0, 1, 0, 1),
            // Shifted into Z+1 and W+1
            new Vector4(0, 0, 1, 1),
            new Vector4(1, 0, 1, 1),
            new Vector4(1, 1, 1, 1),
            new Vector4(0, 1, 1, 1)
        };
    }

    /// <summary>
    /// Sets vertices of the tesseract with a side length 1, starting on [-0.5,-0.5,-0.5,-0.5] and going into a positive direction on all axis
    /// </summary>
    void setVerticesCentered() {
        vertices = new Vector4[]{
            // Each block of 4 is the square on the XY plane shifted by some axis
            // The entire tesseract starts on [-0.5,-0.5,-0.5,-0.5] so the resulting vertices are centered around 0
            // Unshifted
            new Vector4(-0.5f, -0.5f, -0.5f, -0.5f),
            new Vector4(0.5f, -0.5f, -0.5f, -0.5f),
            new Vector4(0.5f, 0.5f, -0.5f, -0.5f),
            new Vector4(-0.5f, 0.5f, -0.5f, -0.5f),
            // Shifted into Z+1
            new Vector4(-0.5f, -0.5f, 0.5f, -0.5f),
            new Vector4(0.5f, -0.5f, 0.5f, -0.5f),
            new Vector4(0.5f, 0.5f, 0.5f, -0.5f),
            new Vector4(-0.5f, 0.5f, 0.5f, -0.5f),
            // Shifted into W+1
            new Vector4(-0.5f, -0.5f, -0.5f, 0.5f),
            new Vector4(0.5f, -0.5f, -0.5f, 0.5f),
            new Vector4(0.5f, 0.5f, -0.5f, 0.5f),
            new Vector4(-0.5f, 0.5f, -0.5f, 0.5f),
            // Shifted into Z+1 and W+1
            new Vector4(-0.5f, -0.5f, 0.5f, 0.5f),
            new Vector4(0.5f, -0.5f, 0.5f, 0.5f),
            new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
            new Vector4(-0.5f, 0.5f, 0.5f, 0.5f)
        };
    }

    /// <summary>
    /// Sets cells of the tesseract by splitting each of its cubes into 5 non-intersecting tetrahedra
    /// </summary>
    void setCells() {
        int[,] cubes = getTesseractCubes();

        // Constructing a list of all tetrahedra in all of the cubes
        List<int[,]> tetrahedra = new List<int[,]>();
        for (int i = 0; i < cubes.GetLength(0); i++) {
            tetrahedra.Add(splitCube(getCube(cubes, i)));
        }

        // Converting the cell list into a one-dimentional array
        cells = new int[cubes_count * cells_in_cube * verts_in_cell];
        int[,] cell;
        for (int i = 0; i < tetrahedra.Count; i++) {
            cell = tetrahedra[i];
            for (int j = 0; j < cell.GetLength(0); j++) {
                for (int k = 0; k < cell.GetLength(1); k++) {
                    cells[i * cell.GetLength(0) * cell.GetLength(1) + j * cell.GetLength(1) + k] = cell[j, k];
                }
            }
        }
    }

    /// <summary>
    /// Builds a 2-dimentional array of cubes of the tesseract in the format [index of a cube, index of a vertex of a cube]
    /// </summary>
    /// <returns>Cubes of the tesseract in the format [index of a cube, index of a vertex of a cube]</returns>
    int[,] getTesseractCubes() {
        // Base cube is a [n, n, n, 0] cube
        // Face cubes share one face with the base cube
        // Extra cube is a [n, n, n, 1] cube
        int[,] cubes = {
            {
                1, 2, 3, 0,
                5, 6, 7, 4,
            }, // Base cube
            {
                1, 2, 3, 0,
                1 + 8, 2 + 8, 3 + 8, 0 + 8,
            }, // Face cube Front
            {
                1, 5, 4, 0,
                1 + 8, 5 + 8, 4 + 8, 0 + 8,
            }, // Face cube Bottom
            {
                3, 7, 4, 0,
                3 + 8, 7 + 8, 4 + 8, 0 + 8,
            }, // Face cube Left
            {
                6, 5, 4, 7,
                6 + 8, 5 + 8, 4 + 8, 7 + 8,
            }, // Face cube Back
            {
                6, 7, 3, 2,
                6 + 8, 7 + 8, 3 + 8, 2 + 8,
            }, // Face cube Top
            {
                6, 5, 1, 2,
                6 + 8, 5 + 8, 1 + 8, 2 + 8,
            }, // Face cube Right
            {
                0 + 8, 1 + 8, 2 + 8, 3 + 8,
                4 + 8, 5 + 8, 6 + 8, 7 + 8,
            }, // Extra cube
        };
        return cubes;
    }

    /// <summary>
    /// Finds an array of vertex indices of a cube with a given index
    /// </summary>
    /// <param name="cubes">An array of cubes of a tesseract</param>
    /// <param name="index">Index of a cube</param>
    /// <returns>An array of vertex indices</returns>
    int[] getCube(int[,] cubes, int index) {
        int[] cube = new int[verts_in_cube];
        for (int i = 0; i < verts_in_cube; i++) {
            cube[i] = cubes[index, i];
        }
        return cube;
    }

    /// <summary>
    /// Splits the cube into 5 non-intersecting tetrahedra
    /// </summary>
    /// <param name="cube">A cube that will be split</param>
    /// <returns>Tetrahedral cells of the cube in the format [index of a cell, index of a vertex of a cell]</returns>
    int[,] splitCube(int[] cube) {
        // Side cells are equal tetrahedra with their apexes placed on the corners of a cube and bases formed from points of a cube connected by its edges to an apex
        // Center cell is a terrahedron that lies in the center without touching any of its edges with edges of a cube
        // The resulting split will cut all of the cube's faces with a single diagonal line and won't introduce any new vertices
        int[,] tetrahedra = {
            {
                cube[0], cube[1], cube[3], cube[4],
            }, // Side cell
            {
                cube[1], cube[2], cube[3], cube[6],
            }, // Side cell
            {
                cube[1], cube[4], cube[5], cube[6],
            }, // Side cell
            {
                cube[3], cube[4], cube[6], cube[7],
            }, // Side cell
            {
                cube[1], cube[3], cube[4], cube[6],
            }, // Center cell
        };
        return tetrahedra;
    }

    public Tesseract(bool centered = true) {
        if (centered)
            setVerticesCentered();
        else
            setVertices();

        setCells();
    }
}
