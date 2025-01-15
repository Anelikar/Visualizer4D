using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A beheaviour that provides proof of concept rendering for the 4D shape
/// </summary>
public class SimpleShapeRenderer : MonoBehaviour
{
    [Tooltip("Prefab containing a LineRenderer used to render edges in the Line render type")]
    [SerializeField]
    Transform m_lineRendererPrefab;
    [Tooltip("Prefab containing a  Mesh and a MeshFilter used to render triangles in the Face render type")]
    [SerializeField]
    Transform m_faceRendererPrefab;
    /// <summary>
    /// Prefab containing a LineRenderer used to render edges in the Line render type
    /// </summary>
    public Transform LineRendererPrefab {
        get => m_lineRendererPrefab;
        set => m_lineRendererPrefab = value;
    }
    /// <summary>
    /// Prefab containing a  Mesh and a MeshFilter used to render triangles in the Face render type
    /// </summary>
    public Transform FaceRendererPrefab {
        get => m_faceRendererPrefab;
        set => m_faceRendererPrefab = value;
    }

    [Tooltip("Transform 4D of a rendered shape")]
    [SerializeField]
    Transform4D m_transform4D;
    /// <summary>
    /// Transform 4D of a rendered shape
    /// </summary>
    public Transform4D Transform4D {
        get => m_transform4D;
        set => m_transform4D = value;
    }

    public enum RenderType
    {
        Line,
        Triangle
    }

    [Tooltip("Line render type will render edges of the shape while Face will render its triangles.\n\nRenderer has to be reinitialised after changing this.")]
    [SerializeField]
    RenderType m_renderType = RenderType.Line;
    /// <summary>
    /// Line render type will render edges of the shape while Face will render its triangles.
    /// </summary>
    public RenderType RendererType {
        get => m_renderType;
        set {
            m_renderType = value;
            ClearRenderers();
            InstantiateRenderers();
        }
    }

    LineRenderer[] m_lineRenderers;
    MeshFilter[] m_meshFilters;
    Mesh[] m_faceMeshes;

    UnityEngine.Events.UnityAction<Vector4[]> m_updateRenderers = (x) => { };
    /// <summary>
    /// Updates renderers to the new vertices with transformded vertices passed as an argument
    /// </summary>
    public UnityEngine.Events.UnityAction<Vector4[]> UpdateRenderers => m_updateRenderers;

    const int edges_in_triangle = 3;
    const int tris_in_cube = 4;

    void OnEnable() {
        m_transform4D.OnStart.AddListener(InstantiateRenderers);
        m_transform4D.OnTransformChange.AddListener(m_updateRenderers);
    }

    void OnDisable() {
        m_transform4D.OnStart.RemoveListener(InstantiateRenderers);
        m_transform4D.OnTransformChange.RemoveListener(m_updateRenderers);
    }

    /// <summary>
    /// Inatantiates renderer gameobjects according to a render type
    /// </summary>
    /// <param name="mesh">The mesh of the rendered object</param>
    public void InstantiateRenderers() {
        // Since OnTransformChange has its listener added on enable, 
        // its listener will be a default empty function before this is called.
        // Because of that, it has to be removed and readded after m_updateRenderers
        // is set to an appropriate method.
        m_transform4D.OnTransformChange.RemoveListener(m_updateRenderers);

        switch (m_renderType) {
            case RenderType.Line:
                InstantiateLineRenderers(m_transform4D.Mesh.CellDataUniqueEdgesCount);
                m_updateRenderers = UpdateLineRenderers;
                break;
            case RenderType.Triangle:
                InstantiateFaceRenderers(m_transform4D.Mesh.TriangleCount);
                m_updateRenderers = UpdateFaceRenderers;
                break;
            default:
                break;
        }

        m_transform4D.OnTransformChange.AddListener(m_updateRenderers);
    }

    /// <summary>
    /// Instantiates line renderer gameobjects
    /// </summary>
    /// <param name="uniqueEdgesCount">Count of unique edges in a 4D mesh that will be rendered. Can be gotten by calling Mesh4D.CellDataUniqueEdgesCount</param>
    void InstantiateLineRenderers(int uniqueEdgesCount) {
        m_lineRenderers = new LineRenderer[uniqueEdgesCount];
        for (int i = 0; i < uniqueEdgesCount; i++) {
            // Instantinating line renderer
            m_lineRenderers[i] = Instantiate(m_lineRendererPrefab, transform).GetComponent<LineRenderer>();
            m_lineRenderers[i].widthMultiplier = 0.005f;

            // Applying color with hue lerped by the index of the edge
            Color color = GetColorLerp(i, uniqueEdgesCount);
            color.a = m_lineRenderers[i].material.color.a;
            m_lineRenderers[i].material.color = color;
        }
    }

    // Default values for face renderer 3d meshes
    Vector3[] m_defaultVertices = { Vector3.zero, Vector3.zero, Vector3.zero };
    int[] m_defaultTriangles = { 0, 1, 2, 0, 2, 1 };
    Vector2[] m_defaultUVs = { Vector2.zero, Vector2.zero, Vector2.zero };

    /// <summary>
    /// Instantiates face renderer gameobjects
    /// </summary>
    /// <param name="triangleCount">Count of triangles in a 4D mesh that will be rendered. Can be gotten by calling Mesh4D.TriangleCount</param>
    void InstantiateFaceRenderers(int triangleCount) {
        m_meshFilters = new MeshFilter[triangleCount];
        m_faceMeshes = new Mesh[triangleCount];

        for (int i = 0; i < triangleCount; i++) {
            // Instantinating face renderer and initializing its mesh
            m_meshFilters[i] = Instantiate(m_faceRendererPrefab, transform).GetComponent<MeshFilter>();
            m_faceMeshes[i] = m_meshFilters[i].mesh;
            m_faceMeshes[i].Clear();
            m_faceMeshes[i].vertices = m_defaultVertices;
            m_faceMeshes[i].uv = m_defaultUVs;
            m_faceMeshes[i].triangles = m_defaultTriangles;

            // Applying color with hue lerped by the index of the triangle
            Color color = GetColorLerp(i, triangleCount);
            MeshRenderer renderer = m_meshFilters[i].GetComponent<MeshRenderer>();
            color.a = renderer.material.color.a;
            renderer.material.color = color;

            m_meshFilters[i].mesh = m_faceMeshes[i];
        }
    }

    /// <summary>
    /// Creates an RGB color from HSV where hue is interpolated from 0 to a by b and saturation and value are maxed
    /// </summary>
    /// <param name="a">Maximum hue</param>
    /// <param name="b">Desired hue</param>
    /// <returns>A new RGB color where hue is interpolated from 0 to a by b and saturation, value and alpha are maxed</returns>
    static Color GetColorLerp(int a, int b) {
        float h = Mathf.Lerp(0, 1f, a / (float)b);
        Color c = Color.HSVToRGB(h, 1, 1);
        return c;
    }

    /// <summary>
    /// Sets all line renderers end points to align with the edges of the transformed mesh
    /// </summary>
    /// <param name="vertices">Transformed vertices of a rendered shape</param>
    void UpdateLineRenderers(Vector4[] vertices) {
        int[] edges = m_transform4D.Mesh.Edges;
        for (int i = 0; i < m_lineRenderers.Length; i++) {
            m_lineRenderers[i].SetPositions(new Vector3[] { vertices[edges[i * 2]], vertices[edges[i * 2 + 1]] });
        }
    }

    /// <summary>
    /// Sets all face meshes vertices to align with the triangles of the transformed mesh
    /// </summary>
    /// <param name="vertices">Transformed vertices of a rendered shape</param>
    void UpdateFaceRenderers(Vector4[] vertices) {
        int[] triangles = m_transform4D.Mesh.Triangles;
        for (int i = 0; i < m_faceMeshes.Length; i++) {
            m_faceMeshes[i].vertices = new Vector3[] { vertices[triangles[i * 3]], vertices[triangles[i * 3 + 1]], vertices[triangles[i * 3 + 2]] };
            m_faceMeshes[i].RecalculateNormals();
            m_meshFilters[i].mesh = m_faceMeshes[i];
        }
    }

    /// <summary>
    /// Destroys all renderer gameobjects and removes refereces to them
    /// </summary>
    void ClearRenderers() {
        // Clearing line renderers
        for (int i = 0; i < m_lineRenderers.Length; i++) {
            Destroy(m_lineRenderers[i].gameObject);
            m_lineRenderers[i] = null;
        }
        m_lineRenderers = null;

        // Clearing face renderers
        for (int i = 0; i < m_faceMeshes.Length; i++) {
            Destroy(m_faceMeshes[i]);
            Destroy(m_meshFilters[i].gameObject);
            m_faceMeshes[i] = null;
            m_meshFilters[i] = null;
        }
        m_faceMeshes = null;
        m_meshFilters = null;
    }
}
