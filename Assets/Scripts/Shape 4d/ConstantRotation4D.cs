using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A behaviour that makes a referenced Transform4D rotate each frame by an amount described in the ConstantRotation Euler4
/// </summary>
public class ConstantRotation4D : MonoBehaviour
{
    [Tooltip("Reference to a Transform4D")]
    [SerializeField]
    Transform4D m_transform4D;
    /// <summary>
    /// Reference to a Transform4D
    /// </summary>
    public Transform4D Transform4D {
        get => m_transform4D;
        set => m_transform4D = value;
    }

    [Tooltip("Transform4D will be rotated by this amount each frame")]
    public Transform4D.Euler4 ConstantRotation;

    [Tooltip("Will be invoked when the constant rotation stops")]
    [SerializeField]
    UnityEvent m_onStopRotation;
    /// <summary>
    /// Will be invoked when the constant rotation stops
    /// </summary>
    public UnityEvent OnStopRotation => m_onStopRotation;

    void Awake() {
        if (ConstantRotation == null) {
            ConstantRotation = new Transform4D.Euler4();
        }
    }

    void Update() {
        m_transform4D.Rotation.YZ += Time.deltaTime * ConstantRotation.YZ;
        m_transform4D.Rotation.XZ += Time.deltaTime * ConstantRotation.XZ;
        m_transform4D.Rotation.XY += Time.deltaTime * ConstantRotation.XY;
        m_transform4D.Rotation.XW += Time.deltaTime * ConstantRotation.XW;
        m_transform4D.Rotation.YW += Time.deltaTime * ConstantRotation.YW;
        m_transform4D.Rotation.ZW += Time.deltaTime * ConstantRotation.ZW;
    }

    /// <summary>
    /// Sets the constant rotation to 0 and invokes OnStopRotation event
    /// </summary>
    public void StopConstRotation() {
        ConstantRotation = Transform4D.Euler4.Zero;
        m_onStopRotation.Invoke();
    }
}
