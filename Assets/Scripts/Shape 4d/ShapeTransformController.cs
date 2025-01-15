using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that makes a 4D transform follow a pivot and provides a method for gizmos to rotate it
/// </summary>
public class ShapeTransformController : MonoBehaviour
{
    [Tooltip("4D transform that will be affected by this controller")]
    [SerializeField]
    Transform4D m_transform4D;
    /// <summary>
    /// 4D transform that will be affected by this controller
    /// </summary>
    public Transform4D Transform_4D {
        get => m_transform4D;
        set => m_transform4D = value;
    }

    [Tooltip("Reference to a ConstantRotation. Can be left empty")]
    [SerializeField]
    ConstantRotation4D m_constRotation;
    /// <summary>
    /// Reference to a ConstantRotation. Can be left empty
    /// </summary>
    public ConstantRotation4D ConstRotation {
        get => m_constRotation;
        set => m_constRotation = value;
    }

    [Tooltip("A pivot that will be followed by the 4D transform")]
    [SerializeField]
    Transform m_pivot;
    /// <summary>
    /// A pivot that will be followed by the 4D transform
    /// </summary>
    public Transform Pivot {
        get => m_pivot;
        set => m_pivot = value;
    }
    Vector3 m_previousPosition;

    void Start() {
        m_transform4D.Position3D = Pivot.position;
        m_previousPosition = Pivot.position;
    }

    void Update() {
        // Following the pivot
        if (Pivot.position != m_previousPosition) {
            m_transform4D.Position3D = Pivot.position;
            m_previousPosition = Pivot.position;
        }
    }

    /// <summary>
    /// Rotates the transform by provided euler angles and stops constant rotation if it's present
    /// </summary>
    /// <param name="EulerAngles">Euler angles rotation in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles rotation in XW, YW and ZW planes</param>
    public void RotateShape(Vector3 EulerAngles, Vector3 wEulerAngles) {
        if (m_constRotation != null) {
            m_constRotation.StopConstRotation();
        }

        m_transform4D.Rotation.YZ += EulerAngles.x;
        m_transform4D.Rotation.XZ += EulerAngles.y;
        m_transform4D.Rotation.XY += EulerAngles.z;
        m_transform4D.Rotation.XW += wEulerAngles.x;
        m_transform4D.Rotation.YW += wEulerAngles.y;
        m_transform4D.Rotation.ZW += wEulerAngles.z;
    }
}
