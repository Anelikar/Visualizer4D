using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that connects interactive UI controls with a 4D shape
/// </summary>
public class Shape4dConnection : MonoBehaviour
{
    [Tooltip("A reference to the ShapeTransformController")]
    [SerializeField]
    ShapeTransformController m_shapeTransformController;
    [Tooltip("A reference to the ConstantRotation4D")]
    [SerializeField]
    ConstantRotation4D m_constRotation;
    [Tooltip("A reference to a parent gameobject that holds the controls for setting up the constant rotation")]
    [SerializeField]
    GameObject m_constRotationControls;

    [Header("Gizmos")]
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a YZ plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoYZ;
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a XW plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoXW;
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a XZ plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoXZ;
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a YW plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoYW;
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a XY plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoXY;
    [Tooltip("A reference to XR Interactable gizmo that controls shape rotation in a ZW plane")]
    [SerializeField]
    GizmoInteracrible m_gizmoZW;

    [Header("Controls")]
    [Header("Manual Rotation")]
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a YZ plane")]
    [SerializeField]
    KnobInteractable m_knobYZ;
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a XW plane")]
    [SerializeField]
    KnobInteractable m_knobXW;
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a XZ plane")]
    [SerializeField]
    KnobInteractable m_knobXZ;
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a YW plane")]
    [SerializeField]
    KnobInteractable m_knobYW;
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a XY plane")]
    [SerializeField]
    KnobInteractable m_knobXY;
    [Tooltip("A reference to XR Interactable knob that controls shape rotation in a ZW plane")]
    [SerializeField]
    KnobInteractable m_knobZW;

    [Header("Constant Rotation")]
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a YZ plane")]
    [SerializeField]
    KnobInteractable m_knobConstYZ;
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a XY plane")]
    [SerializeField]
    KnobInteractable m_knobConstXW;
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a XZ plane")]
    [SerializeField]
    KnobInteractable m_knobConstXZ;
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a YW plane")]
    [SerializeField]
    KnobInteractable m_knobConstYW;
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a XY plane")]
    [SerializeField]
    KnobInteractable m_knobConstXY;
    [Tooltip("A reference to XR Interactable knob that sets up a constant shape rotation in a ZW plane")]
    [SerializeField]
    KnobInteractable m_knobConstZW;

    /// <summary>
    /// Is the constant rotation active?
    /// </summary>
    bool m_hasConstRotation = false;

    float m_prevYZvalue;
    float m_prevXWvalue;
    float m_prevXZvalue;
    float m_prevYWvalue;
    float m_prevXYvalue;
    float m_prevZWvalue;

    void Start() {
        // Initializing previous values
        m_prevYZvalue = m_knobYZ.Value;
        m_prevXWvalue = m_knobXW.Value;
        m_prevXZvalue = m_knobXZ.Value;
        m_prevYWvalue = m_knobYW.Value;
        m_prevXYvalue = m_knobXY.Value;
        m_prevZWvalue = m_knobZW.Value;

        // Initializing constant rotation knobs if the shape has initial constant rotation
        if ((m_constRotation.ConstantRotation.YZ != 0) ||
            (m_constRotation.ConstantRotation.XW != 0) ||
            (m_constRotation.ConstantRotation.XZ != 0) ||
            (m_constRotation.ConstantRotation.YW != 0) ||
            (m_constRotation.ConstantRotation.XY != 0) ||
            (m_constRotation.ConstantRotation.ZW != 0)) {
            m_knobConstYZ.Value = m_constRotation.ConstantRotation.YZ;
            m_knobConstXW.Value = m_constRotation.ConstantRotation.XW;
            m_knobConstXZ.Value = m_constRotation.ConstantRotation.XZ;
            m_knobConstYW.Value = m_constRotation.ConstantRotation.YW;
            m_knobConstXY.Value = m_constRotation.ConstantRotation.XY;
            m_knobConstZW.Value = m_constRotation.ConstantRotation.ZW;

            m_hasConstRotation = true;
        }

        // Disabling constant rotation knobs
        if (m_constRotationControls != null) {
            m_constRotationControls.SetActive(false);
        }

        // These listeners have to be enabled even when the gameobject is inactive
        // to avoid knobs desyncing from the shape
        m_constRotation.OnStopRotation.AddListener(ResetConstKnobs);

        m_gizmoYZ.OnRotation.AddListener(RotateKnobYZ);
        m_gizmoXW.OnRotation.AddListener(RotateKnobXW);
        m_gizmoXZ.OnRotation.AddListener(RotateKnobXZ);
        m_gizmoYW.OnRotation.AddListener(RotateKnobYW);
        m_gizmoXY.OnRotation.AddListener(RotateKnobXY);
        m_gizmoZW.OnRotation.AddListener(RotateKnobZW);
    }

    void OnEnable() {
        m_knobYZ.OnValueChange.AddListener(RotateYZ);
        m_knobXW.OnValueChange.AddListener(RotateXW);
        m_knobXZ.OnValueChange.AddListener(RotateXZ);
        m_knobYW.OnValueChange.AddListener(RotateYW);
        m_knobXY.OnValueChange.AddListener(RotateXY);
        m_knobZW.OnValueChange.AddListener(RotateZW);

        m_knobConstYZ.OnValueChange.AddListener(SetConstYZ);
        m_knobConstXW.OnValueChange.AddListener(SetConstXW);
        m_knobConstXZ.OnValueChange.AddListener(SetConstXZ);
        m_knobConstYW.OnValueChange.AddListener(SetConstYW);
        m_knobConstXY.OnValueChange.AddListener(SetConstXY);
        m_knobConstZW.OnValueChange.AddListener(SetConstZW);
    }

    void OnDisable() {
        m_knobYZ.OnValueChange.RemoveListener(RotateYZ);
        m_knobXW.OnValueChange.RemoveListener(RotateXW);
        m_knobXZ.OnValueChange.RemoveListener(RotateXZ);
        m_knobYW.OnValueChange.RemoveListener(RotateYW);
        m_knobXY.OnValueChange.RemoveListener(RotateXY);
        m_knobZW.OnValueChange.RemoveListener(RotateZW);

        m_knobConstYZ.OnValueChange.RemoveListener(SetConstYZ);
        m_knobConstXW.OnValueChange.RemoveListener(SetConstXW);
        m_knobConstXZ.OnValueChange.RemoveListener(SetConstXZ);
        m_knobConstYW.OnValueChange.RemoveListener(SetConstYW);
        m_knobConstXY.OnValueChange.RemoveListener(SetConstXY);
        m_knobConstZW.OnValueChange.RemoveListener(SetConstZW);
    }

    /// <summary>
    /// Rotates the shape in a YZ plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateYZ(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(new Vector3(-(value - m_prevYZvalue), 0, 0), Vector3.zero);
        m_prevYZvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Rotates the shape in a XW plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateXW(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(Vector3.zero, new Vector3(-(value - m_prevXWvalue), 0, 0));
        m_prevXWvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Rotates the shape in a XZ plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateXZ(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(new Vector3(0, value - m_prevXZvalue, 0), Vector3.zero);
        m_prevXZvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Rotates the shape in a YW plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateYW(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(Vector3.zero, new Vector3(0, value - m_prevYWvalue, 0));
        m_prevYWvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Rotates the shape in a XY plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateXY(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(new Vector3(0, 0, -(value - m_prevXYvalue)), Vector3.zero);
        m_prevXYvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Rotates the shape in a ZW plane and stops the constant rotation
    /// </summary>
    /// <param name="value">New angle of rotation</param>
    void RotateZW(float value) {
        // Converting angle to delta
        m_shapeTransformController.RotateShape(Vector3.zero, new Vector3(0, 0, -(value - m_prevZWvalue)));
        m_prevZWvalue = value;

        m_constRotation.StopConstRotation();
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a YZ plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstYZ(float value) {
        m_constRotation.ConstantRotation.YZ = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a XW plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstXW(float value) {
        m_constRotation.ConstantRotation.XW = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a XZ plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstXZ(float value) {
        m_constRotation.ConstantRotation.XZ = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a YW plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstYW(float value) {
        m_constRotation.ConstantRotation.YW = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a XY plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstXY(float value) {
        m_constRotation.ConstantRotation.XY = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets up the constant rotation of the shape in a ZW plane
    /// </summary>
    /// <param name="value">New speed of constant rotation</param>
    void SetConstZW(float value) {
        m_constRotation.ConstantRotation.ZW = value;
        m_hasConstRotation = true;
    }

    /// <summary>
    /// Sets all constant rotation knobs to 0
    /// </summary>
    public void ResetConstKnobs() {
        if (m_hasConstRotation) {
            m_knobConstYZ.Value = 0;
            m_knobConstXW.Value = 0;
            m_knobConstXZ.Value = 0;
            m_knobConstYW.Value = 0;
            m_knobConstXY.Value = 0;
            m_knobConstZW.Value = 0;

            m_hasConstRotation = false;
        }
    }

    /// <summary>
    /// Sets all non-constant rotation knobs to 0
    /// </summary>
    public void ResetKnobs() {
        // Setting previous values so that OnValueChange callbacks here don't rotate the shape again
        m_prevYZvalue = 0;
        m_prevXZvalue = 0;
        m_prevXYvalue = 0;
        m_prevXWvalue = 0;
        m_prevYWvalue = 0;
        m_prevZWvalue = 0;

        // Resetting knob values
        m_knobYZ.Value = 0;
        m_knobXZ.Value = 0;
        m_knobXY.Value = 0;
        m_knobXW.Value = 0;
        m_knobYW.Value = 0;
        m_knobZW.Value = 0;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobYZ(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevYZvalue + eulerAngles.x;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevYZvalue = newValue;

        // Updating knob value
        m_knobYZ.Value = newValue;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobXZ(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevXZvalue + eulerAngles.y;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevXZvalue = newValue;

        // Updating knob value
        m_knobXZ.Value = newValue;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobXY(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevXYvalue + eulerAngles.z;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevXYvalue = newValue;

        // Updating knob value
        m_knobXY.Value = newValue;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobXW(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevXWvalue + wEulerAngles.x;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevXWvalue = newValue;

        // Updating knob value
        m_knobXW.Value = newValue;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobYW(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevYWvalue + wEulerAngles.y;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevYWvalue = newValue;

        // Updating knob value
        m_knobYW.Value = newValue;
    }

    /// <summary>
    /// Rotates the knob by a 4D shape euler angles delta
    /// </summary>
    /// <param name="eulerAngles">Euler angles delta in YZ, XZ, and XY planes</param>
    /// <param name="wEulerAngles">Euler angles delta in XW, YW and ZW planes</param>
    public void RotateKnobZW(Vector3 eulerAngles, Vector3 wEulerAngles) {
        // Adding delta to a previous value
        float newValue = m_prevZWvalue + wEulerAngles.z;

        // Setting previous value so that OnValueChange callback here doesn't rotate the shape again
        m_prevZWvalue = newValue;

        // Updating knob value
        m_knobZW.Value = newValue;
    }
}
