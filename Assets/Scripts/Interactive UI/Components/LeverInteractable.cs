using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// A behaviour that sets up an XR intaractable lever on an object
/// </summary>
public class LeverInteractable : XRBaseInteractable
{
    [Header("Lever properties")]
    [SerializeField]
    [Tooltip("The lever that will be rotated by this behaviour. Will be set to this transfom if left empty")]
    Transform m_lever;
    /// <summary>
    /// The knob that will be rotated by this behaviour. Will be set to this transfom if left empty
    /// </summary>
    public Transform Lever {
        get => m_lever;
        set {
            m_lever = value;
        }
    }

    public enum RotationAxis
    {
        x,
        y,
        z
    }
    [SerializeField]
    [Tooltip("This is used to change the axis of rotation. The red line is drawn to show this axis in the editor")]
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

    [Tooltip("The minimum angle of the lever. The lever won't work properly if the angle between bounds is greater than 180 degrees")]
    [SerializeField]
    float m_lowerBound = -45f;
    [Tooltip("The maximum angle of the lever. The lever won't work properly if the angle between bounds is greater than 180 degrees")]
    [SerializeField]
    float m_upperBound = 45f;
    /// <summary>
    /// The minimum angle of the lever. The lever won't work properly if the angle between bounds is greater than 180 degrees
    /// </summary>
    public float LowerBound {
        get => m_lowerBound;
        set {
            if (value > m_upperBound) {
                m_lowerBound = m_upperBound;
                m_upperBound = value;
            } else {
                m_lowerBound = value;
            }
        }
    }
    /// <summary>
    /// The maximum angle of the lever. The lever won't work properly if the angle between bounds is greater than 180 degrees
    /// </summary>
    public float UpperBound {
        get => m_upperBound;
        set {
            if (value < m_lowerBound) {
                m_upperBound = m_lowerBound;
                m_lowerBound = value;
            } else {
                m_upperBound = value;
            }
        }
    }

    [Tooltip("Current lever value in a 0..1 range")]
    [SerializeField]
    [Range(0, 1)]
    float m_value;
    /// <summary>
    /// Current lever value. It will be clamped in a 0..1 range. 
    /// The lever rotation will be updated according to it
    /// </summary>
    public float Value {
        get => m_value;
        set {
            SetValue(Mathf.Clamp01(value));
            RotateLeverByValue(value);
        }
    }

    [Tooltip("Does this interactable behave like a switch?\n\n" +
        "Swithces can only have a value of 0 or 1 and they snap to these values on release. " +
        "They also call OnValueChange only when reaching these values.")]
    [SerializeField]
    bool m_isSwitch = false;
    /// <summary>
    /// Does this interactable behave like a switch?
    /// Swithces can only have a value of 0 or 1 and they snap to these values on release. 
    /// They also call OnValueChange only when reaching these values.
    /// </summary>
    public bool IsSwitch {
        get => m_isSwitch;
        set {
            m_isSwitch = value;
            if (m_isSwitch) {
                SnapSwitch();
            }
        }
    }


    [Tooltip("This event is invoked every frame when the lever has its value changed")]
    [SerializeField]
    UnityEvent<float> m_onValueChange;
    /// <summary>
    /// This event is invoked every frame when the lever has its value changed
    /// </summary>
    public UnityEvent<float> OnValueChange => m_onValueChange;

    [Tooltip("This event is invoked when the lever reaches the min value")]
    [SerializeField]
    UnityEvent m_onMinValue;
    /// <summary>
    /// This event is invoked when the lever reaches the min value
    /// </summary>
    public UnityEvent OnMinValue => m_onMinValue;
    bool m_hitMin = false;

    [Tooltip("This event is invoked when the lever reaches the max value")]
    [SerializeField]
    UnityEvent m_onMaxValue;
    /// <summary>
    /// This event is invoked when the lever reaches the max value
    /// </summary>
    public UnityEvent OnMaxValue => m_onMaxValue;
    bool m_hitMax = false;

    [Tooltip("Value range from the min and max in which the onMinValue and onMaxValue will be invoked")]
    [SerializeField]
    [Range(0, 0.5f)]
    float m_edgeDeadzone = 0.1f;
    /// <summary>
    /// Value range from the min and max in which the onMinValue and onMaxValue will be invoked. Will be clamped to 0..0.5
    /// </summary>
    public float EdgeDeadzone {
        get => m_edgeDeadzone;
        set {
            m_edgeDeadzone = Mathf.Clamp(value, 0, 0.5f);
        }
    }

    Vector3 m_leverBaseAxis;
    /// <summary>
    /// Base axis rotated by current rotation
    /// </summary>
    Vector3 LeverWorldAxis {
        get {
            return transform.rotation * m_leverBaseAxis;
        }
    }

    float m_prevValue;
    float m_prevAngle;
    IXRSelectInteractor m_selectInteractor;

    private void OnDrawGizmosSelected() {
        // Drawing guidlines for the editor setup
        if (m_lever != null) {
            Vector3 axis = InitAxis();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_lever.position, m_lever.position + axis);
        }
    }

    protected override void Awake() {
        base.Awake();

        // Initializing the lever if it's not set in editor
        if (m_lever == null) {
            m_lever = transform;
        }

        // Initializing axis
        InitAxis();

        // Setting the value to 0 or 1 for the switch
        // For non-switches just clamping the value
        if (m_isSwitch) {
            if (m_value < 0.5) {
                m_value = 0;
            } else {
                m_value = 1;
            }
        } else {
            m_value = Mathf.Clamp01(m_value);
        }

        // Initializing prevValue and prevAngle
        m_prevValue = m_value;
        m_prevAngle = 0;

        // Rotating the lever into its initial angle from initial value
        RotateLeverByValue(m_value);
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
            float angle = FindGrabAngle();

            // Angle is projected into the lowerBound..upperBound range
            SetValue((angle + Mathf.Abs(m_lowerBound)) / (Mathf.Abs(m_lowerBound) + Mathf.Abs(m_upperBound)));

            RotateLever(angle);
        }
    }

    void SelectEnter(SelectEnterEventArgs args) {
        if (m_selectInteractor == null) {
            m_selectInteractor = args.interactorObject;
        }
    }

    void SelectExit(SelectExitEventArgs args) {
        if (m_selectInteractor == args.interactorObject) {
            m_selectInteractor = null;

            // Snapping the lever to 0 or 1 if its a switch
            if (m_isSwitch) {
                SnapSwitch();
            }
        }
    }

    /// <summary>
    /// Sets up the leverBaseAxis vector from the axis enum
    /// </summary>
    /// <returns>leverBaseAxis vector</returns>
    Vector3 InitAxis() {
        switch (m_axis) {
            case RotationAxis.x:
                m_leverBaseAxis = Vector3.right;
                break;
            case RotationAxis.y:
                m_leverBaseAxis = Vector3.up;
                break;
            case RotationAxis.z:
                m_leverBaseAxis = Vector3.forward;
                break;
            default:
                m_leverBaseAxis = Vector3.right;
                break;
        }
        return m_leverBaseAxis;
    }

    /// <summary>
    /// Sets a new value without rotating the lever and calls value change events if nessessary
    /// </summary>
    /// <param name="value">The new value</param>
    void SetValue(float value) {
        if (m_prevValue != value) {
            m_value = value;
            if (m_isSwitch) {
                // Calling onValueChange only when hitting min and max
                if (value == 0) {
                    m_onValueChange.Invoke(0);
                    if (!m_hitMin) {
                        m_onMinValue.Invoke();
                        m_hitMin = true;
                    }
                } else if (value == 1) {
                    m_onValueChange.Invoke(1);
                    if (!m_hitMax) {
                        m_onMaxValue.Invoke();
                        m_hitMax = true;
                    }
                }
            } else {
                // Calling onValueChange every frame its changed
                m_onValueChange.Invoke(value);
                if (value == 0) {
                    if (!m_hitMin) {
                        m_onMinValue.Invoke();
                        m_hitMin = true;
                    }
                } else if (value == 1) {
                    if (!m_hitMax) {
                        m_onMaxValue.Invoke();
                        m_hitMax = true;
                    }
                }
            }

            // Updating hitMin and hitMax booleans
            if (m_hitMin) {
                if (value >= 0 + m_edgeDeadzone) {
                    m_hitMin = false;
                }
            }
            if (m_hitMax) {
                if (value <= 1 - m_edgeDeadzone) {
                    m_hitMax = false;
                }
            }
            m_prevValue = m_value;
        }
    }

    /// <summary>
    /// Sets the lever rotation to a value
    /// </summary>
    /// <param name="value">The new lever value</param>
    void RotateLeverByValue(float value) {
        float angle = Mathf.Lerp(m_lowerBound, m_upperBound, value);
        RotateLever(angle);
    }

    /// <summary>
    /// Sets the lever rotation to a new angle and saves its previous angle
    /// </summary>
    /// <param name="angle">New knob angle</param>
    void RotateLever(float angle) {
        // New angle is converted into delta
        m_lever.RotateAround(m_lever.position, LeverWorldAxis, angle - m_prevAngle);
        m_prevAngle = angle;
    }

    /// <summary>
    /// Finds an angle to an interactor on a rotation plane
    /// </summary>
    /// <returns>The grab angle clamped in a lowerBound..upperBound range</returns>
    float FindGrabAngle() {
        // Finding a vector form the pivot of the lever to a interactor
        Vector3 toSelect = Quaternion.Inverse(transform.rotation) * (m_selectInteractor.transform.position - m_lever.transform.position);

        // Projecting the vector onto a lever rotation plane and normalizing it
        Vector2 angleVector;
        switch (m_axis) {
            case RotationAxis.x:
                angleVector = new Vector2(toSelect.z, toSelect.y).normalized;
                break;
            case RotationAxis.y:
                angleVector = new Vector2(toSelect.x, toSelect.z).normalized;
                break;
            case RotationAxis.z:
                angleVector = new Vector2(toSelect.x, toSelect.y).normalized;
                break;
            default:
                angleVector = new Vector2(toSelect.z, toSelect.y).normalized;
                break;
        }

        // Finding an angle of the projected vector and clamping it
        // It is assumed that the lever bounds are within 180 degrees of eachother
        return Mathf.Clamp(Mathf.Rad2Deg * Mathf.Acos(angleVector.x) - 90, m_lowerBound, m_upperBound);
    }

    /// <summary>
    /// Snaps the value to 0 or 1 depending on which is closer and rotates the lever to it.
    /// </summary>
    void SnapSwitch() {
        if (m_value < 0.5) {
            Value = 0;
        } else {
            Value = 1;
        }
    }
}
