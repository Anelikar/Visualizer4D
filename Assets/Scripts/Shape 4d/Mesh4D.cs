using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and holds 4D mesh data
/// </summary>
[CreateAssetMenu(fileName = "Mesh4D", menuName = "ScriptableObjects/Mesh4D", order = 1)]
public class Mesh4D : ScriptableObject
{
    [Tooltip("Vectices of this mesh. Constructed automatically and aren't supposed to be changed manually")]
    [SerializeField]
    Vector4[] m_vertices;
    /// <summary>
    /// Vectices of this mesh
    /// </summary>
    public Vector4[] Vertices { 
        get => m_vertices; 
        private set => m_vertices = value;
    }
    [Tooltip("Cells of this mesh. Constructed automatically and aren't supposed to be changed manually")]
    [SerializeField]
    int[] m_cells;
    /// <summary>
    /// Cells of this mesh
    /// </summary>
    public int[] Cells { 
        get => m_cells; 
        private set => m_cells = value;
    }
    [Tooltip("Triangles of this mesh. Constructed automatically and aren't supposed to be changed manually")]
    [SerializeField]
    int[] m_triangles;
    /// <summary>
    /// Triangles of this mesh
    /// </summary>
    public int[] Triangles {
        get => m_triangles;
        private set => m_triangles = value;
    }

    [Tooltip("Edges of this mesh. Constructed automatically and aren't supposed to be changed manually")]
    [SerializeField]
    int[] m_edges;
    /// <summary>
    /// Edges of this mesh.
    /// </summary>
    public int[] Edges {
        get => m_edges;
        private set => m_edges = value;
    }

    Vector4[] m_normals;
    /// <summary>
    /// Normals of this mesh
    /// </summary>
    public Vector4[] Normals {
        get => m_normals;
        private set => m_normals = value;
    }

    /// <summary>
    /// Vertex count of a mesh
    /// </summary>
    public int VertexCount {
        get => m_vertices.Length;
    }
    /// <summary>
    /// Cell count of a mesh
    /// </summary>
    public int CellCount {
        get => m_cells.Length / verts_in_cell;
    }
    /// <summary>
    /// Triangle count of a mesh
    /// </summary>
    public int TriangleCount { 
        get => m_triangles.Length / verts_in_triangle;
    }
    /// <summary>
    /// Edge count of a mesh
    /// </summary>
    public int EdgeCount { 
        get => m_edges.Length / verts_in_edge; 
    }

    /// <summary>
    /// Data structure that can hold all the nesessary cell data
    /// </summary>
    [System.Serializable]
    public class CellData
    {
        public List<int> Tris;
        public List<int> Edges;

        public List<int> UniqueTris;
        public List<int> UniqueEdges;

        public CellData() {
            Tris = new List<int>();
            Edges = new List<int>();

            UniqueTris = new List<int>();
            UniqueEdges = new List<int>();
        }
    }
    
    [Tooltip("This data is constructed while building a mesh and is meant to be readonly. Don't modify it unless you know what you are doing.")]
    public CellData[] cellData;

    /// <summary>
    /// Count of unique edges in all of the cells
    /// </summary>
    public int CellDataUniqueEdgesCount {
        get {
            int n = 0;
            foreach (var item in cellData) {
                n += item.UniqueEdges.Count;
            }
            return n;
        }
    }
    /// <summary>
    /// Unique edges in all of the cells
    /// </summary>
    public int[] CellDataUniqueEdges {
        get {
            List<int> l = new List<int>();
            foreach (var item in cellData) {
                l.AddRange(item.UniqueEdges);
            }
            return l.ToArray();
        }
    }

    /// <summary>
    /// Count of unique triangles in all of the cells
    /// </summary>
    public int CellDataUniqueTrisCount {
        get {
            int n = 0;
            foreach (var item in cellData) {
                n += item.UniqueTris.Count;
            }
            return n;
        }
    }
    /// <summary>
    /// Unique triangles in all of the cells
    /// </summary>
    public int[] CellDataUniqueTris {
        get {
            List<int> l = new List<int>();
            foreach (var item in cellData) {
                l.AddRange(item.UniqueTris);
            }
            return l.ToArray();
        }
    }

    bool m_cullData;

    const int verts_in_cell = 4;
    const int edges_in_cell = 6;
    const int verts_in_edge = 2;
    const int triangles_in_cell = 4;
    const int verts_in_triangle = 3;

    public Mesh4D(Vector4[] vertices, int[] cells, bool cullData = false) {
        BuildMesh(vertices, cells, cullData);
    }

    /// <summary>
    /// Builds this mesh from the given vertices and cells arrays
    /// </summary>
    /// <param name="vertices">Vertex array for the new mesh</param>
    /// <param name="cells">Cell array for the new mesh</param>
    /// <param name="cullData">Should duplicated triangles and edges be removed?</param>
    public void BuildMesh(Vector4[] vertices, int[] cells, bool cullData = false) {
        // Checking if the cell array is of an appropriate length
        if (cells.Length % verts_in_cell != 0)
            throw new UnityException("Tried to initialise Mesh4D with a cell array that is not a multiple of 4");

        m_vertices = vertices;
        m_cells = cells;
        m_cullData = cullData;

        // Allocating the nessessary amount of cell data structures
        cellData = new CellData[CellCount];
        for (int i = 0; i < cellData.Length; i++) {
            cellData[i] = new CellData();
        }

        // Building triangles, edges and calculating normals
        m_triangles = BuildTriangles();
        m_edges = BuildEdges();
        m_normals = CalculateNormals();
    }

    /// <summary>
    /// Returns an array of points representing a cell with a given index
    /// </summary>
    /// <param name="index">Index of a cell</param>
    /// <returns>An array of points representing a cell</returns>
    public int[] GetCell(int index) {
        int[] cell = new int[4];

        for (int i = 0; i < 4; i++) {
            cell[i] = Cells[index * 4 + i];
        }

        return cell;
    }

    /// <summary>
    /// Returns an array of points represinting all of the triangles in a given cell
    /// </summary>
    /// <param name="cell">Cell that will be split into triangles</param>
    /// <returns>Array of points represinting all of the triangles in a cell</returns>
    public int[] GetCellTriangles(int[] cell) {
        int[] triangles = new int[3 * 4];

        int i = 0;
        triangles[i] = cell[0];
        triangles[i + 1] = cell[1];
        triangles[i + 2] = cell[2];
        i += 3;
        triangles[i] = cell[0];
        triangles[i + 1] = cell[2];
        triangles[i + 2] = cell[3];
        i += 3;
        // Triangles are reversed from this point to adhere to the right hand rule
        triangles[i] = cell[2];
        triangles[i + 1] = cell[1];
        triangles[i + 2] = cell[3];
        i += 3;
        triangles[i] = cell[0];
        triangles[i + 1] = cell[3];
        triangles[i + 2] = cell[1];

        return triangles;
    }

    /// <summary>
    /// Builds the triangles for the cell data
    /// </summary>
    /// <returns>An array of points representing the triangles of the cells</returns>
    int[] BuildTriangles() {
        int[] triangles = new int[CellCount * triangles_in_cell * verts_in_triangle];

        const int tri_index_offset = verts_in_triangle * triangles_in_cell;
        for (int i = 0; i < m_cells.Length; i += verts_in_cell) {
            int[] triIndexes = new int[triangles_in_cell];
            int cellIndex = i / verts_in_cell;

            // Each cell is made up of 4 triangles
            triangles[cellIndex * tri_index_offset + 0] = m_cells[i + 0];
            triangles[cellIndex * tri_index_offset + 1] = m_cells[i + 1];
            triangles[cellIndex * tri_index_offset + 2] = m_cells[i + 2];
            triIndexes[0] = cellIndex * triangles_in_cell + 0;

            triangles[cellIndex * tri_index_offset + 3] = m_cells[i + 0];
            triangles[cellIndex * tri_index_offset + 4] = m_cells[i + 2];
            triangles[cellIndex * tri_index_offset + 5] = m_cells[i + 3];
            triIndexes[1] = cellIndex * triangles_in_cell + 1;

            triangles[cellIndex * tri_index_offset + 6] = m_cells[i + 1];
            triangles[cellIndex * tri_index_offset + 7] = m_cells[i + 2];
            triangles[cellIndex * tri_index_offset + 8] = m_cells[i + 3];
            triIndexes[2] = cellIndex * triangles_in_cell + 2;

            triangles[cellIndex * tri_index_offset + 9] = m_cells[i + 0];
            triangles[cellIndex * tri_index_offset + 10] = m_cells[i + 1];
            triangles[cellIndex * tri_index_offset + 11] = m_cells[i + 3];
            triIndexes[3] = cellIndex * triangles_in_cell + 3;

            cellData[cellIndex].Tris.AddRange(triIndexes);
        }

        if (m_cullData)
            return GetUniqueTrianges(triangles);

        // Setting UniqueTris to be the same as Tris if the shape is not culled
        for (int i = 0; i < cellData.Length; i++) {
            cellData[i].UniqueTris = cellData[i].Tris;
        }
        return triangles;
    }

    /// <summary>
    /// Selects unique triangles from a given triangle point array
    /// </summary>
    /// <param name="triangles">A triangle point array to select unique triangles from</param>
    /// <returns>Point array representing unique triangles</returns>
    int[] GetUniqueTrianges(int[] triangles) {
        List<int> uniqueTriangles = new List<int>();
        List<int>[] cellDataTris = new List<int>[CellCount];
        for (int i = 0; i < cellDataTris.Length; i++) {
            cellDataTris[i] = new List<int>();
        }
        List<int>[] uniqueCellDataTris = new List<int>[CellCount];
        for (int i = 0; i < uniqueCellDataTris.Length; i++) {
            uniqueCellDataTris[i] = new List<int>();
        }

        int foundTriIndex;
        for (int i = 0; i < triangles.Length; i += verts_in_triangle) {
            foundTriIndex = -1;
            for (int j = 0; j < uniqueTriangles.Count; j += verts_in_triangle) {
                // Comparing two triangles to check if they are premutations of eachother
                // By row:
                // exact comparison 012 == 012
                // twist 012 == 021
                // twist 012 == 102
                // rotation 012 == 120
                // rotation 012 == 201
                // twist 012 == 210
                if (((triangles[i] == uniqueTriangles[j + 0]) && (triangles[i + 1] == uniqueTriangles[j + 1]) && (triangles[i + 2] == uniqueTriangles[j + 2])) ||
                    ((triangles[i] == uniqueTriangles[j + 0]) && (triangles[i + 1] == uniqueTriangles[j + 2]) && (triangles[i + 2] == uniqueTriangles[j + 1])) ||
                    ((triangles[i] == uniqueTriangles[j + 1]) && (triangles[i + 1] == uniqueTriangles[j + 0]) && (triangles[i + 2] == uniqueTriangles[j + 2])) ||
                    ((triangles[i] == uniqueTriangles[j + 1]) && (triangles[i + 1] == uniqueTriangles[j + 2]) && (triangles[i + 2] == uniqueTriangles[j + 0])) ||
                    ((triangles[i] == uniqueTriangles[j + 2]) && (triangles[i + 1] == uniqueTriangles[j + 0]) && (triangles[i + 2] == uniqueTriangles[j + 1])) ||
                    ((triangles[i] == uniqueTriangles[j + 2]) && (triangles[i + 1] == uniqueTriangles[j + 1]) && (triangles[i + 2] == uniqueTriangles[j + 0]))) {
                    foundTriIndex = j / verts_in_triangle;
                    break;
                }
            }

            int currentCellIndex = i / verts_in_triangle / triangles_in_cell;
            if (foundTriIndex == -1) {
                // Adding a new triangle
                uniqueTriangles.Add(triangles[i]);
                uniqueTriangles.Add(triangles[i + 1]);
                uniqueTriangles.Add(triangles[i + 2]);

                int currentTriIndex = (uniqueTriangles.Count - 1) / verts_in_triangle;
                cellDataTris[currentCellIndex].Add(currentTriIndex);
                uniqueCellDataTris[currentCellIndex].Add(currentTriIndex);
            } else {
                // Marking found triangle
                cellDataTris[currentCellIndex].Add(foundTriIndex);
            }
        }

        // Updating cell data arrays
        for (int i = 0; i < cellData.Length; i++) {
            cellData[i].Tris = cellDataTris[i];
            cellData[i].UniqueTris = uniqueCellDataTris[i];
        }
        return uniqueTriangles.ToArray();
    }

    /// <summary>
    /// Builds the edges for the cell data
    /// </summary>
    /// <returns>An array of points representing the edges of the cells</returns>
    int[] BuildEdges() {
        int[] edges = new int[CellCount * edges_in_cell * verts_in_edge];

        const int edge_index_offset = verts_in_edge * edges_in_cell;
        for (int i = 0; i < m_cells.Length; i += verts_in_cell) {
            int[] edgesIndexes = new int[edges_in_cell];
            int cellIndex = i / verts_in_cell;

            // Each cell is made up of 6 edges
            edges[cellIndex * edge_index_offset + 0] = m_cells[i + 0];
            edges[cellIndex * edge_index_offset + 1] =  m_cells[i + 1];
            edgesIndexes[0] = cellIndex * edges_in_cell + 0;

            edges[cellIndex * edge_index_offset + 2] = m_cells[i + 0];
            edges[cellIndex * edge_index_offset + 3] = m_cells[i + 2];
            edgesIndexes[1] = cellIndex * edges_in_cell + 1;

            edges[cellIndex * edge_index_offset + 4] = m_cells[i + 0];
            edges[cellIndex * edge_index_offset + 5] = m_cells[i + 3];
            edgesIndexes[2] = cellIndex * edges_in_cell + 2;

            edges[cellIndex * edge_index_offset + 6] = m_cells[i + 1];
            edges[cellIndex * edge_index_offset + 7] = m_cells[i + 2];
            edgesIndexes[3] = cellIndex * edges_in_cell + 3;

            edges[cellIndex * edge_index_offset + 8] = m_cells[i + 1];
            edges[cellIndex * edge_index_offset + 9] = m_cells[i + 3];
            edgesIndexes[4] = cellIndex * edges_in_cell + 4;

            edges[cellIndex * edge_index_offset + 10] = m_cells[i + 2];
            edges[cellIndex * edge_index_offset + 11] = m_cells[i + 3];
            edgesIndexes[5] = cellIndex * edges_in_cell + 5;

            cellData[cellIndex].Edges.AddRange(edgesIndexes);
        }

        if (m_cullData)
            return GetUniqueEdges(edges);

        // Setting UniqueEdges to be the same as Edges if the shape is not culled
        for (int i = 0; i < cellData.Length; i++) {
            cellData[i].UniqueEdges = cellData[i].Edges;
        }
        return edges;
    }

    /// <summary>
    /// Selects unique edges from a given edge point array
    /// </summary>
    /// <param name="edges">An edge point array to select unique edges from</param>
    /// <returns>Point array representing unique edges</returns>
    int[] GetUniqueEdges(int[] edges) {
        List<int> uniqueEdges = new List<int>();
        List<int>[] cellDataEdges = new List<int>[CellCount];
        for (int i = 0; i < cellDataEdges.Length; i++) {
            cellDataEdges[i] = new List<int>();
        }
        List<int>[] uniqueCellDataEdges = new List<int>[CellCount];
        for (int i = 0; i < uniqueCellDataEdges.Length; i++) {
            uniqueCellDataEdges[i] = new List<int>();
        }

        int foundEdgeIndex;
        for (int i = 0; i < edges.Length; i += verts_in_edge) {
            foundEdgeIndex = -1;
            for (int j = 0; j < uniqueEdges.Count; j += verts_in_edge) {
                // Comparing two edges to check if they are premutations of eachother
                // By row:
                // exact comparison 01 == 01
                // reverse 01 == 10
                if (((edges[i] == uniqueEdges[j + 0]) && (edges[i + 1] == uniqueEdges[j + 1])) ||
                    ((edges[i] == uniqueEdges[j + 1]) && (edges[i + 1] == uniqueEdges[j + 0]))) {
                    foundEdgeIndex = j / verts_in_edge;
                    break;
                }
            }

            int currentCellIndex = i / verts_in_edge / edges_in_cell;
            if (foundEdgeIndex == -1) {
                // Adding a new edge
                uniqueEdges.Add(edges[i]);
                uniqueEdges.Add(edges[i + 1]);

                int currentEdgeIndex = (uniqueEdges.Count - 1) / verts_in_edge;
                cellDataEdges[currentCellIndex].Add(currentEdgeIndex);
                uniqueCellDataEdges[currentCellIndex].Add(currentEdgeIndex);
            } else {
                // Marking found edge
                cellDataEdges[currentCellIndex].Add(foundEdgeIndex);
            }
        }

        // Updating cell data arrays
        for (int i = 0; i < cellData.Length; i++) {
            cellData[i].Edges = cellDataEdges[i];
            cellData[i].UniqueEdges = uniqueCellDataEdges[i];
        }
        return uniqueEdges.ToArray();
    }

    /// <summary>
    /// Calculates normals for each vertex based on triangles connected to them
    /// </summary>
    /// <returns>An array of vertex normals</returns>
    Vector4[] CalculateNormals() {
        Vector4[] vertexNormals = new Vector4[m_vertices.Length];

        // Looping over every triangle to calculate vertex normals based on face normals
        for (int i = 0; i < TriangleCount; i++) {
            int vertexIndexA = m_triangles[i * verts_in_triangle + 0];
            int vertexIndexB = m_triangles[i * verts_in_triangle + 1];
            int vertexIndexC = m_triangles[i * verts_in_triangle + 2];

            // Calculating the triangle's face normal
            Vector4 triangleNormal = FaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Applying the face normal to its triangle's vertices
            // Vectors will be normalized later
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Normalizing vectors
        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }
        return vertexNormals;
    }

    /// <summary>
    /// Returns a normal vector with a direction perpendicular to a plane defined by 3 vertices in a m_vertices array
    /// </summary>
    /// <param name="indexA">Index of a vertex A</param>
    /// <param name="indexB">Index of a vertex B</param>
    /// <param name="indexC">Index of a vertex C</param>
    /// <returns>4d normal of a given plane</returns>
    Vector4 FaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector4 pointA = m_vertices[indexA];
        Vector4 pointB = m_vertices[indexB];
        Vector4 pointC = m_vertices[indexC];

        Vector4 sideAB = pointB - pointA;
        Vector4 sideAC = pointC - pointA;
        return Vector4Ext.CrossProduct(sideAB, sideAC).normalized;
    }

    /// <summary>
    /// Returns vertex normals for the triangle with a given index
    /// </summary>
    /// <param name="triangleIndex">Index of a triangle which vertex normal will be returned</param>
    /// <returns>An array of vertex normals</returns>
    public Vector4[] GetTriangleNormals(int triangleIndex) {
        return new Vector4[] { 
            m_normals[m_triangles[triangleIndex * verts_in_triangle + 0]], 
            m_normals[m_triangles[triangleIndex * verts_in_triangle + 1]], 
            m_normals[m_triangles[triangleIndex * verts_in_triangle + 2]] 
        };
    }
}
