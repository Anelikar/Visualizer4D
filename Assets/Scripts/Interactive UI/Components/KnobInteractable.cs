using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// A behaviour that sets up an XR intaractable knob on an object
/// </summary>
public class KnobInteractable : XRBaseInteractable
{
    [Header("Knob properties")]
    [Tooltip("The knob that will be rotated by this behaviour. Will be set to this transfom if left empty")]
    [SerializeField]
    Transform m_knob;
    /// <summary>
    /// The knob that will be rotated by this behaviour. Will be set to this transfom if left empty
    /// </summary>
    public Transform Knob {
        get => m_knob;
        set {
            m_knob = value;
        }
    }

    public enum RotationAxis
    {
        x,
        y,
        z
    }
    [Tooltip("This is used to change the axis of rotation. The red line is drawn to show this axis in the editor")]
    [SerializeField]
    RotationAxis m_axis = RotationAxis.x;
    /// <summary>
    /// This is used to change the axis of rotation. The red line is drawn to show this axis in the editor
    /// </summary>
    public RotationAxis Axis {
        get => m_axis;
        set {
            m_axis = value;
            InitAxis();
        }
    }

    [Tooltip("Current knob value in a minValue..maxValue range")]
    [SerializeField]
    float m_value;
    /// <summary>
    /// Current knob value. It will be clamped in a minValue..maxValue range if the knob isn't freescroll. 
    /// The knob rotation will be updated according to it
    /// </summary>
    public float Value {
        get => m_value;
        set {
            if (!m_freescroll) {
                value = Mathf.Clamp(value, m_minValue, m_maxValue);
            }
            SetValue(value);
            RotateKnobByValue(value);
        }
    }

    [Tooltip("The minimum value the knob will stop at if it's not freescroll")]
    [SerializeField]
    float m_minValue = 0f;
    [Tooltip("The maximum value the knob will stop at if it's not freescroll")]
    [SerializeField]
    float m_maxValue = 1f;
    [Tooltip("The amount a 360 revolution will add to a value")]
    [SerializeField]
    float m_revolutionMultiplier = 1f;
    [SerializeField]
    [Tooltip("If true, the knob will move past min and max values")]
    bool m_freescroll = false;
    /// <summary>
    /// The minimum value the knob will stop at if it's not freescroll
    /// </summary>
    public float MinValue {
        get => m_minValue;
        set {
            if (value > m_maxValue) {
                m_minValue = m_maxValue;
                m_maxValue = value;
            } else {
                m_minValue = value;
            }
            UpdateBounds();
        }
    }
    /// <summary>
    /// The maximum value the knob will stop at if it's not freescroll
    /// </summary>
    public float MaxValue {
        get => m_maxValue;
        set {
            if (value < m_minValue) {
                m_maxValue = m_minValue;
                m_minValue = value;
            } else {
                m_maxValue = value;
            }
            UpdateBounds();
        }
    }
    /// <summary>
    /// The amount a 360 revolution will add to a value
    /// </summary>
    public float RevolutionMultiplier {
        get => m_revolutionMultiplier;
        set {
            if (value == 0) {
                m_revolutionMultiplier = 1;
            } else {
                m_revolutionMultiplier = value;
            }
            UpdateBounds();
        }
    }
    /// <summary>
    /// If true, the knob will move past min and max values
    /// </summary>
    public bool Freescroll {
        get => m_freescroll;
        set => m_freescroll = value;
    }

    [SerializeField]
    [Tooltip("Line renderer that draws a line from interactor to this interactable. Can be left empty.")]
    LineRenderer m_grabLine;
    /// <summary>
    /// Line renderer that draws a line from interactor to this interactable. Can be left empty.
    /// </summary>
    public LineRenderer GrabLine {
        get => m_grabLine;
        set => m_grabLine = GrabLine;
    }

    [Tooltip("This event is invoked every frame when the knob has its value changed")]
    [SerializeField]
    UnityEvent<float> m_onValueChange;
    /// <summary>
    /// This event is invoked every frame when the knob has its value changed
    /// </summary>
    public UnityEvent<float> OnValueChange => m_onValueChange;

    Vector3 m_knobBaseAxis;
    /// <summary>
    /// Base axis rotated by current rotation
    /// </summary>
    Vector3 KnobWorldAxis {
        get {
            return transform.rotation * m_knobBaseAxis;
        }
    }

    // Min and max angles are calculated when chainging MinValue, MaxValue or RevolutionMultiplier
    float m_minAngle;
    float m_maxAngle;

    float m_prevValue;
    float m_prevAngle;
    IXRSelectInteractor m_selectInteractor;

    /// <summary>
    /// The angle offset used to make the knob be grabbable by the side
    /// </summary>
    float m_interactorOffset = 0;

    private void OnDrawGizmosSelected() {
        // Drawing guidlines for the editor setup
        if (m_knob != null) {
            Vector3 axis = InitAxis();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_knob.position, m_knob.position + axis);
        }
    }

    protected override void Awake() {
        base.Awake();

        // Initializing the knob if it's not set in editor
        if (m_knob == null) {
            m_knob = transform;
        }

        // Initializing axis and angle bounds
        InitAxis();
        UpdateBounds();

        // Clamping the initial value if the knob isn't freescroll
        if (!m_freescroll) {
            m_value = Mathf.Clamp(m_value, m_minValue, m_maxValue);
        }

        // Initializing prevValue and prevAngle
        m_prevValue = m_value;
        m_prevAngle = m_value / m_revolutionMultiplier * 360;

        // Rotating the knob into its initial angle from initial value
        RotateKnobByValue(m_value);

        // Initializing select visuals
        if (m_grabLine != null) {
            m_grabLine.enabled = false;
        }
    } 

    protected override void OnEnable() {
        base.OnEnable();

        selectEntered.AddListener(SelectEnter);
        selectExited.AddListener(SelectExit);
    }

    protected override void OnDisable() {
        base.OnDisable();

        selectEntered.RemoveListener(SelectEnter);
        selectExited.RemoveListener(SelectExit);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        if (m_selectInteractor != null && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed) {
            float angle = FindAngleToPoint(m_selectInteractor.transform.position, m_interactorOffset);

            // Clamping the angle if the knob isn't freescroll
            if (!m_freescroll) {
                angle = ClampToBounds(angle);
            }

            // Angle is converted to a value
            SetValue(angle / 360 * m_revolutionMultiplier);
            RotateKnob(angle);

            // Updating select visuals
            if (m_grabLine != null) {
                m_grabLine.SetPosition(0, transform.position);
                m_grabLine.SetPosition(1, m_selectInteractor.transform.position);
            }
        }
    }

    void SelectEnter(SelectEnterEventArgs args) {
        if (m_selectInteractor == null) {
            m_selectInteractor = args.interactorObject;

            // TODO see if it's nessessary to call on every select
            //UpdateBounds();

            // Finding initial knob offset from the grab point so that the knob can be grabbed by the side
            InitializeOffset(m_selectInteractor.transform.position);

            // Enabling select visuals
            if (m_grabLine != null) {
                m_grabLine.enabled = true;
                m_grabLine.SetPosition(0, transform.position);
                m_grabLine.SetPosition(1, m_selectInteractor.transform.position);
            }
        }
    }

    void SelectExit(SelectExitEventArgs args) {
        if (m_selectInteractor == args.interactorObject) {
            m_selectInteractor = null;

            // Disabling select visuals
            if (m_grabLine != null) {
                m_grabLine.enabled = false;
            }
        }
    }

    /// <summary>
    /// Sets up the knobBaseAxis vector from the axis enum
    /// </summary>
    /// <returns>knobBaseAxis vector</returns>
    Vector3 InitAxis() {
        switch (m_axis) {
            case RotationAxis.x:
                m_knobBaseAxis = Vector3.right;
                break;
            case RotationAxis.y:
                m_knobBaseAxis = Vector3.up;
                break;
            case RotationAxis.z:
                m_knobBaseAxis = Vector3.forward;
                break;
            default:
                m_knobBaseAxis = Vector3.right;
                break;
        }
        return m_knobBaseAxis;
    }

    /// <summary>
    /// Calculates min and max angles from min and max values and the revolutionMultiplier
    /// </summary>
    void UpdateBounds() {
        // Calculating min and max angles from min and max values
        m_minAngle = m_minValue * 360 / m_revolutionMultiplier;
        m_maxAngle = Mathf.Abs(m_maxValue - m_minValue) * 360 / m_revolutionMultiplier;
    }

    /// <summary>
    /// Initializes interactorOffset
    /// </summary>
    /// <param name="point">Position of the grab point (interactor) in worldspace</param>
    void InitializeOffset(Vector3 point) {
        // Projecting prevAngle into a 0..360 range
        float projectedPrevAngle = m_prevAngle % 360;
        if (projectedPrevAngle < 0) {
            projectedPrevAngle = 360 + projectedPrevAngle;
        }

        // Finding initial knob offset from the grab point so that the knob can be grabbed by the side
        m_interactorOffset = FindAngleToPoint(point, 0) - projectedPrevAngle;
    }

    /// <summary>
    /// Sets a new value without rotating the knob and calls onValueChange if nessessary
    /// </summary>
    /// <param name="value">The new value</param>
    void SetValue(float value) {
        if (m_prevValue != value) {
            m_value = value;
            m_onValueChange.Invoke(value);
            m_prevValue = m_value;
        }
    }

    /// <summary>
    /// Sets the knob rotation to a value
    /// </summary>
    /// <param name="value">The new knob value. Will be clamped to a minValue..maxValue range if not freescroll</param>
    void RotateKnobByValue(float value) {
        if (!m_freescroll) {
            value = Mathf.Clamp(value, m_minValue, m_maxValue);
        }
        RotateKnob(value / m_revolutionMultiplier * 360);
    }

    /// <summary>
    /// Sets the knob rotation to a new angle and saves its previous angle
    /// </summary>
    /// <param name="angle">New knob angle</param>
    void RotateKnob(float angle) {
        // New angle is converted into delta
        m_knob.RotateAround(m_knob.position, KnobWorldAxis, angle - m_prevAngle);
        m_prevAngle = angle;
    }

    /// <summary>
    /// Finds the angle between prevAngle and the vector from the position of the knob to a point in worldspace.
    /// </summary>
    /// <param name="point">The point in worldspace</param>
    /// <param name="offset">The angle offset of the knob</param>
    /// <returns>The angle between prevAngle and the vector from the position of the knob to a point in worldspace</returns>
    float FindAngleToPoint(Vector3 point, float offset) {
        // Converting a worldspace point into a localspace angle vector
        Vector3 toPoint = Quaternion.Inverse(transform.rotation) * (point - m_knob.transform.position);
        Vector2 angleVector;
        switch (m_axis) {
            case RotationAxis.x:
                angleVector = new Vector2(toPoint.z, toPoint.y).normalized;
                break;
            case RotationAxis.y:
                angleVector = new Vector2(toPoint.x, toPoint.z).normalized;
                break;
            case RotationAxis.z:
                angleVector = new Vector2(toPoint.x, toPoint.y).normalized;
                break;
            default:
                angleVector = new Vector2(toPoint.z, toPoint.y).normalized;
                break;
        }

        // Calculating a 0..180 angle and converting it into a 0..360 angle
        float angle = Mathf.Rad2Deg * Mathf.Acos(angleVector.y);
        if (angleVector.x < 0) {
            angle = 360 - angle;
        }

        // Applying the offset and projecting the angle back into the 0..360 range
        angle = (angle - offset) % 360;
        if (angle < 0) {
            angle = 360 + angle;
        }

        // Projecting prevAngle into the 0..360 range
        float projectedPrevAngle = m_prevAngle % 360;
        if (projectedPrevAngle < 0) {
            projectedPrevAngle = 360 + projectedPrevAngle;
        }

        // Finding the shortest delta
        // This manages the case of going through 0 and 360
        float delta = angle - projectedPrevAngle;
        if (Mathf.Abs(delta) > Mathf.Abs(delta - 360)) {
            delta -= 360;
        } else if (Mathf.Abs(delta) > Mathf.Abs(delta + 360)) {
            delta += 360;
        }

        return m_prevAngle + delta;
    }

    /// <summary>
    /// Stops the angle from exceeding the minAngle..maxAngle range while updating interactorOffset to stop the knob rotation if it does
    /// </summary>
    /// <param name="angle">Angle to process</param>
    /// <returns>Angle clamped in a minAngle..maxAngle range</returns>
    float ClampToBounds(float angle) {
        float clampedAngle = Mathf.Clamp(angle, m_minAngle, m_maxAngle);

        // Updating the offset so that the knob doesn't rotate beyond the bounds
        m_interactorOffset += angle - clampedAngle;

        return clampedAngle;
    }
}
