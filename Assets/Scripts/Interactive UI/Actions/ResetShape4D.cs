using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The behaviour that provides a method to set the rotation of the 4d shape to 0 and stop the constant rotation
/// </summary>
public class ResetShape4D : MonoBehaviour
{
    [Tooltip("4d transform that will be reset")]
    [SerializeField]
    Transform4D m_transform4d;

    [Tooltip("ConstantRotation behaviour that will be reset")]
    [SerializeField]
    ConstantRotation4D m_constRotation;

    [Tooltip("Interactive UI knobs controller that will be reset")]
    [SerializeField]
    Shape4dConnection m_shape4DConnection;

    /// <summary>
    /// Sets the rotation of the 4d shape to 0 and stops the constant rotation
    /// </summary>
    public void ResetShape() {
        m_constRotation.StopConstRotation();
        m_transform4d.Rotation = Transform4D.Euler4.Zero;
        m_shape4DConnection.ResetKnobs();
    }
}
