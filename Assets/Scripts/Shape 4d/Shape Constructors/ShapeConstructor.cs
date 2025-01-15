using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShapeConstructor", menuName = "ScriptableObjects/ShapeConstructor", order = 2)]
public class ShapeConstructor : ScriptableObject
{
    [Tooltip("Reference to a Mesh4D scriptable object")]
    [SerializeField]
    Mesh4D m_mesh4D;
    /// <summary>
    /// Reference to a Mesh4D scriptable object
    /// </summary>
    public Mesh4D MeshObj {
        get => m_mesh4D;
        set => m_mesh4D = value;
    }

    public enum Shape
    {
        Cell5,
        Tesseract
    }
    [Tooltip("Type of the shape to build")]
    [SerializeField]
    Shape m_shapeType;
    /// <summary>
    /// Type of the shape to build
    /// </summary>
    public Shape ShapeType {
        get => m_shapeType;
        set => m_shapeType = value;
    }

    [Tooltip("Should the duplicate cell data be culled after construction?")]
    [SerializeField]
    bool m_cullShape = false;
    /// <summary>
    /// Should the duplicate cell data be culled after construction?
    /// </summary>
    public bool CullShape {
        get => m_cullShape;
        set => m_cullShape = value;
    }

    [Tooltip("Should mesh data be logged after construction?")]
    [SerializeField]
    bool m_logMeshData = true;
    /// <summary>
    /// Should mesh data be logged after construction?
    /// </summary>
    public bool LogMeshData {
        get => m_logMeshData;
        set => m_logMeshData = value;
    }

    /// <summary>
    /// Builds a mesh on the MeshObj using settings on this object
    /// </summary>
    public void BuildMesh() {
        if (m_mesh4D != null) {
            BuildMesh(m_shapeType, ref m_mesh4D, m_cullShape);
        } else {
            throw new UnityException("Tried to setup shape for a null mesh4D object");
        }
    }

    /// <summary>
    /// Builds a mesh on the referenced object using given settings
    /// </summary>
    /// <param name="shape">Type of shape to build</param>
    /// <param name="mesh">Mesh reference that will be build</param>
    /// <param name="cullShape">Should the duplicate cell data be culled after construction?</param>
    public void BuildMesh(Shape shape, ref Mesh4D mesh, bool cullShape = false) {
        Model4D model;
        switch (shape) {
            case Shape.Cell5:
                model = new Cell5();
                break;
            case Shape.Tesseract:
                model = new Tesseract();
                break;
            default:
                throw new System.Exception("Unrecognised shape");
        }
        mesh.BuildMesh(model.vertices, model.cells, cullShape);

        if (m_logMeshData) {
            LogData(mesh);
        }
    }

    /// <summary>
    /// Logs mesh data for a given mesh
    /// </summary>
    /// <param name="mesh">Mesh to be logged</param>
    void LogData(Mesh4D mesh) {
        Debug.Log("Vertex count: " + mesh.VertexCount);
        Debug.Log("Cell count: " + mesh.CellCount);
        Debug.Log("Triangle count: " + mesh.TriangleCount);
        Debug.Log("Edge count: " + mesh.EdgeCount);
    }
}
