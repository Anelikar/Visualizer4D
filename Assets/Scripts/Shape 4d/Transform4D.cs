using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A behaviour that transforms connected 4D mesh and projects it as a 3D "shadow"
/// </summary>
public class Transform4D : MonoBehaviour
{
    /// <summary>
    /// Representation of euler angles in 4D
    /// </summary>
    [System.Serializable]
    public struct Euler4
    {
        [Range(-180, +180)]
        public float XY; // Z (W)
        [Range(-180, +180)]
        public float YZ; // X (w)
        [Range(-180, +180)]
        public float XZ; // Y (W)
        [Range(-180, +180)]
        public float XW; // Y Z
        [Range(-180, +180)]
        public float YW; // X Z
        [Range(-180, +180)]
        public float ZW;

        /// <summary>
        /// Represents a [0, 0, 0, 0, 0, 0] vector
        /// </summary>
        public static readonly Euler4 Zero = new Euler4(0, 0, 0, 0, 0, 0);

        public Euler4(float xy, float yz, float xz, float xw, float yw, float zw) {
            XY = xy;
            YZ = yz;
            XZ = xz;
            XW = xw;
            YW = yw;
            ZW = zw;
        }

        public static bool operator != (Euler4 first, Euler4 second) {
            return !(first == second);
        }

        public static bool operator == (Euler4 first, Euler4 second) {
            if (first == null && second == null)
                return true;
            if (first == null || second == null)
                return false;
            if (first.XY != second.XY ||
                first.YZ != second.YZ ||
                first.XZ != second.XZ ||
                first.XW != second.XW ||
                first.YW != second.YW ||
                first.ZW != second.ZW)
                return false;
            return true;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 486187739 + XY.GetHashCode();
                hash = hash * 486187739 + YZ.GetHashCode();
                hash = hash * 486187739 + XZ.GetHashCode();
                hash = hash * 486187739 + XW.GetHashCode();
                hash = hash * 486187739 + YW.GetHashCode();
                hash = hash * 486187739 + ZW.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) {
            if (!(obj is Euler4))
                return false;
            return this == (Euler4)obj;
        }

        public static Euler4 operator - (Euler4 first, Euler4 second) {
            Euler4 newEuler = new Euler4();
            newEuler.XY = first.XY - second.XY;
            newEuler.YZ = first.YZ - second.YZ;
            newEuler.XZ = first.XZ - second.XZ;
            newEuler.XW = first.XW - second.XW;
            newEuler.YW = first.YW - second.YW;
            newEuler.ZW = first.ZW - second.ZW;
            return newEuler;
        }
    }

    [Tooltip("4D mesh that will be transformed")]
    [SerializeField]
    Mesh4D m_mesh;
    /// <summary>
    /// 4D mesh that will be transformed
    /// </summary>
    public Mesh4D Mesh {
        get => m_mesh;
        set {
            m_mesh = value;
            m_vertices = m_mesh.Vertices;
        }
    }

    Vector4[] m_vertices;

    /// <summary>
    /// Position of the 4D transform
    /// </summary>
    [Header("Transform")]
    public Vector4 Position;
    /// <summary>
    /// Rotation of the 4D transform represented as euled angles in degrees
    /// </summary>
    public Euler4 Rotation;
    /// <summary>
    /// Scale of the 4D transform
    /// </summary>
    public Vector4 Scale = new Vector4(1, 1, 1, 1);

    /// <summary>
    /// Distance of the "light source" on the W axis used to cast a 3D shadow in projection
    /// </summary>
    public float ProjectionLightDistance = 1.5f;
    /// <summary>
    /// Final 3D translation of the transform
    /// </summary>
    public Vector3 Position3D;

    Vector4 m_prevPosition;
    Euler4 m_prevRotation;
    Vector4 m_prevScale;
    Vector3 m_prevPosition3D;

    Matrix4x4 m_rotationMatrix;
    Matrix4x4 m_rotationInverse;

    [Header("Renderer events")]
    [Tooltip("Invoked immediatly on Start of this behaviour")]
    [SerializeField]
    UnityEvent m_onStart;
    /// <summary>
    /// Invoked immediatly on Start of this behaviour
    /// </summary>
    public UnityEvent OnStart => m_onStart;
    [Tooltip("Invoked every time the thansform changes. Transformed vertices are passed as an argument.")]
    [SerializeField]
    UnityEvent<Vector4[]> m_onTransformChange;
    /// <summary>
    /// Invoked every time the thansform changes. Transformed vertices are passed as an argument.
    /// </summary>
    public UnityEvent<Vector4[]> OnTransformChange => m_onTransformChange;

    void Start() {
        m_vertices = new Vector4[m_mesh.Vertices.Length];

        // Applying initial transformation
        m_onStart.Invoke();
        UpdateRotationMatrix();
        UpdateVertices();
        m_onTransformChange.Invoke(m_vertices);

        // Initializing previous transformation data
        m_prevPosition = Position;
        m_prevRotation = Rotation;
        m_prevScale = Scale;
        m_prevPosition3D = Position3D;
    }

    void Update() {
        if (Position != m_prevPosition || Rotation != m_prevRotation || Scale != m_prevScale || m_prevPosition3D != Position3D) {
            // Transforming vertices
            UpdateRotationMatrix();
            UpdateVertices();
            m_onTransformChange.Invoke(m_vertices);

            // Updating previous transformation
            m_prevPosition = Position;
            m_prevRotation = Rotation;
            m_prevScale = Scale;
            m_prevPosition3D = Position3D;
        }
    }

    /// <summary>
    /// Creates a new 4D rotation matrix based on the euler angles rotation of the transform4D
    /// </summary>
    /// <returns>4D rotation matrix of this transform4D</returns>
    Matrix4x4 UpdateRotationMatrix() {
        m_rotationMatrix =
            Matrix4x4.identity
            .RotateXY(Rotation.XY * Mathf.Deg2Rad)
            .RotateYZ(Rotation.YZ * Mathf.Deg2Rad)
            .RotateXZ(Rotation.XZ * Mathf.Deg2Rad)
            .RotateXW(Rotation.XW * Mathf.Deg2Rad)
            .RotateYW(Rotation.YW * Mathf.Deg2Rad)
            .RotateZW(Rotation.ZW * Mathf.Deg2Rad);
        m_rotationInverse = m_rotationMatrix.inverse;
        return m_rotationMatrix;
    }

    /// <summary>
    /// Applies a TRS transformation to a 4D mesh, projects it into 3D and translates the final shape.
    /// This transformation is saved into m_vertices
    /// </summary>
    void UpdateVertices() {
        for (int i = 0; i < m_mesh.Vertices.Length; i++) {
            // Transforming 4D vertices and projecting them into 3D
            m_vertices[i] = Transform(m_mesh.Vertices[i]);
            m_vertices[i] = Project3D(m_vertices[i]);

            // Translating final 3D shape
            m_vertices[i] += (Vector4)Position3D;
        }
    }

    /// <summary>
    /// Applies a TRS tranformation to a 4D vertex
    /// </summary>
    /// <param name="v">A vertex that will be tranformed</param>
    /// <returns>The transformed vertex</returns>
    public Vector4 Transform(Vector4 v) {
        // Rotation
        v = m_rotationMatrix * v;

        // Scale
        v.x *= Scale.x;
        v.y *= Scale.y;
        v.z *= Scale.z;
        v.w *= Scale.w;

        // Translation
        v += Position;

        return v;
    }

    /// <summary>
    /// Applies a "shadow cast" projection to a 4D vertex
    /// </summary>
    /// <param name="v">A vertex that will be projected</param>
    /// <returns>The projected vertex</returns>
    Vector4 Project3D(Vector4 v) {
        float projectionValue = 1 / (ProjectionLightDistance - v.w);
        Matrix4x4 ProjectionMatrix = new Matrix4x4(
            new Vector4(projectionValue, 0, 0, 0),
            new Vector4(0, projectionValue, 0, 0),
            new Vector4(0, 0, projectionValue, 0),
            new Vector4(0, 0, 0, 0));
        v = ProjectionMatrix * v;
        return v;
    }
}
