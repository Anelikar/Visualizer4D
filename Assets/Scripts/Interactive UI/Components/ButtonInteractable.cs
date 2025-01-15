using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// A behaviour that sets up an XR intaractable button on an object
/// </summary>
public class ButtonInteractable : XRBaseInteractable
{
    [Header("Button properties")]
    [Tooltip("The button that will be moved by this behaviour. Will be set to this transfom if left empty")]
    [SerializeField]
    Transform m_button;
    /// <summary>
    /// The button that will be moved by this behaviour. Will be set to this transfom if left empty. 
    /// Make sure that the button and the offset are in their inital relative positions
    /// as their offset will be calculated and saved when changing this
    /// </summary>
    public Transform Button {
        get => m_button;
        set {
            m_button = value;
            SaveButtonOffset();
        }
    }

    [Tooltip("A transform that has the same parent as the button. Button will be moved using its position and initial offset between them. Can be left empty to create an attach copying the button")]
    [SerializeField]
    Transform m_buttonAttach;
    /// <summary>
    /// A transform that has the same parent as the button. 
    /// Button will be moved using its position and initial offset between them. Can be left empty to create an attach copying the button. 
    /// Make sure that the button and the offset are in their inital relative positions
    /// as their offset will be calculated and saved when changing this
    /// </summary>
    public Transform ButtonAttach {
        get => m_buttonAttach;
        set {
            m_buttonAttach = value;
            SaveButtonOffset();
        }
    }

    public enum PressDirection
    {
        x,
        y,
        z,
        x_neg,
        y_neg,
        z_neg
    }
    [Tooltip("This is used to change the direction of the button press. The red line is drawn from the button in a direction of the press in the editor")]
    [SerializeField]
    PressDirection m_pressDirection = PressDirection.y;
    /// <summary>
    /// This is used to change the direction of the button press. The red line is drawn from the button in a direction of the press in the editor
    /// </summary>
    public PressDirection PressDir {
        get => m_pressDirection;
        set {
            m_pressDirection = value;
            InitAxis();
        }
    }

    [Tooltip("The maximum press distance")]
    [SerializeField]
    float m_pressDistance = 0.024f;
    /// <summary>
    /// The maximum press distance
    /// </summary>
    public float PressDistance {
        get => m_pressDistance;
        set => m_pressDistance = value;
    }

    [Tooltip("Current button value in a 0..1 range")]
    [SerializeField]
    [Range(0, 1)]
    float m_value;
    /// <summary>
    /// Current button value. It will be clamped in a 0..1 range and the button depth will be updated according to it
    /// </summary>
    public float Value {
        get => m_value;
        set {
            SetButtonDepth(value * m_pressDistance);
            SetValue(Mathf.Clamp01(value));
        }
    }

    [Tooltip("A value in a 0..1 range at which the OnPress event will be triggered")]
    [Range(0, 1)]
    [SerializeField]
    float m_pressTriggerValue = 0.95f;
    [Tooltip("A value in a 0..1 range at which the OnRelease event will be triggered")]
    [Range(0, 1)]
    [SerializeField]
    float m_releaseTriggerValue = 0.05f;
    /// <summary>
    /// Button value at which the OnPress event will be triggered. It will be clamped in a 0..1 range
    /// </summary>
    public float PressTriggerValue {
        get => m_pressTriggerValue;
        set {
            m_pressTriggerValue = Mathf.Clamp01(value);
        }
    }
    /// <summary>
    /// Button value at which the OnRelease event will be triggered. It will be clamped in a 0..1 range
    /// </summary>
    public float ReleaseTriggerValue {
        get => m_releaseTriggerValue;
        set {
            m_releaseTriggerValue = Mathf.Clamp01(value);
        }
    }

    [Tooltip("Cooldown between the button presses. It's used to avoid rapid button activations due to a natural hand tremor")]
    [SerializeField]
    float m_reactivationCooldown = 0.3f;
    /// <summary>
    /// Cooldown between the button presses. It's used to avoid rapid button activations due to a natural hand tremor
    /// </summary>
    public float ReactivationDelay {
        get => m_reactivationCooldown;
        set => m_reactivationCooldown = value;
    }
    bool m_onCooldown = false;

    [Tooltip("The button will ignore this much press distance from the interactor. It is used to improve the feeling of firmly touching the button")]
    [SerializeField]
    float m_pressDeadzone = 0.003f;
    /// <summary>
    /// The button will ignore this much press distance from the interactor. It is used to improve the feeling of firmly touching the button
    /// </summary>
    public float PressDeadzone {
        get => m_pressDeadzone;
        set => m_pressDeadzone = value;
    }

    [Tooltip("This event is invoked every frame when the button has its value changed")]
    [SerializeField]
    UnityEvent<float> m_onValueChange;
    /// <summary>
    /// This event is invoked every frame when the button has its value changed
    /// </summary>
    public UnityEvent<float> OnValueChange => m_onValueChange;

    [Tooltip("This event is invoked every time the button reaches the PressDistance")]
    [SerializeField]
    UnityEvent m_onPress;
    /// <summary>
    /// This event is invoked every time the button reaches the PressDistance
    /// </summary>
    public UnityEvent OnPress => m_onPress;

    [Tooltip("This event is invoked when the button reaches the ReleaseDistance after being presseed")]
    [SerializeField]
    UnityEvent m_onRelease;
    /// <summary>
    /// This event is invoked when the button reaches the ReleaseDistance after being presseed
    /// </summary>
    public UnityEvent OnRelease => m_onRelease;

    Vector3 m_baseOffset;
    Vector3 m_buttonBaseAxis;
    /// <summary>
    /// Base axis rotated by current rotation
    /// </summary>
    Vector3 ButtonWorldAxis {
        get {
            return transform.rotation * m_buttonBaseAxis;
        }
    }
    float m_prevValue;
    /// <summary>
    /// Key: HoverInteractor. 
    /// Value: Interactor postion at the moment of HoverEnter
    /// </summary>
    Dictionary<IXRHoverInteractor, Vector3> m_hoverInteractors;
    bool m_pressed = false;

    private void OnDrawGizmosSelected() {
        // Drawing guidlines for the editor setup
        if (m_button != null) {
            Vector3 axis = InitAxis();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_button.position, m_button.position + -axis);
        }
    }

    protected override void Awake() {
        base.Awake();

        // Initializing the button and the attach if they aren't set in editor
        if (m_button == null) {
            m_button = transform;
        }
        if (m_buttonAttach == null) {
            // Creating a new gameobject to be used as an attach and copying button's transform and parent to it
            GameObject attach = new GameObject("Button attach");
            attach.transform.parent = m_button.parent;
            attach.transform.position = m_button.position;
            attach.transform.rotation = m_button.rotation;
            attach.transform.localScale = m_button.localScale;
            m_buttonAttach = attach.transform;
        }

        m_hoverInteractors = new Dictionary<IXRHoverInteractor, Vector3>();
        InitAxis();
        SaveButtonOffset();
    }

    protected override void OnEnable() {
        base.OnEnable();

        hoverEntered.AddListener(HoverEnter);
        hoverExited.AddListener(HoverExit);
    }

    protected override void OnDisable() {
        base.OnDisable();

        hoverEntered.RemoveListener(HoverEnter);
        hoverExited.RemoveListener(HoverExit);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        if (m_hoverInteractors.Count > 0 && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed) {
            float pressDepth = GetPressDepth();
            SetButtonDepth(pressDepth);

            // Depth is converted to a value in a 0..1 range
            SetValue(pressDepth / m_pressDistance);
        }
    }

    void HoverEnter(HoverEnterEventArgs args) {
        m_hoverInteractors.Add(
            args.interactorObject,
            args.interactorObject.GetAttachTransform(this).position);
    }

    void HoverExit(HoverExitEventArgs args) {
        m_hoverInteractors.Remove(args.interactorObject);

        // Resetting the button if there is nothing interacting with it
        if (m_hoverInteractors.Count == 0) {
            SetValue(0);
            SetButtonDepth(0);
        }
    }

    /// <summary>
    /// Sets up the buttonBaseAxis vector from the pressDirection enum
    /// </summary>
    /// <returns>buttonBaseAxis vector</returns>
    Vector3 InitAxis() {
        switch (m_pressDirection) {
            case PressDirection.x:
                m_buttonBaseAxis = Vector3.right;
                break;
            case PressDirection.y:
                m_buttonBaseAxis = Vector3.up;
                break;
            case PressDirection.z:
                m_buttonBaseAxis = Vector3.forward;
                break;
            case PressDirection.x_neg:
                m_buttonBaseAxis = Vector3.left;
                break;
            case PressDirection.y_neg:
                m_buttonBaseAxis = Vector3.down;
                break;
            case PressDirection.z_neg:
                m_buttonBaseAxis = Vector3.back;
                break;
            default:
                m_buttonBaseAxis = Vector3.up;
                break;
        }
        return m_buttonBaseAxis;
    }

    /// <summary>
    /// Finds the deepest press among all active interactors and returns its depth clamped between 0 and pressDistance
    /// </summary>
    /// <returns>Farthest depth of press clamped between 0 and pressDistance</returns>
    float GetPressDepth() {
        // Cycling through all active intreactors to find the one with the deepest press
        float pressDepth = 0;
        foreach (var item in m_hoverInteractors) {
            Vector3 pressVector = item.Key.GetAttachTransform(this).position - item.Value;

            // Checking that the pressVector is pointing in the correct direction
            if (Vector3.Dot(pressVector, -ButtonWorldAxis) > 0) {
                // Calculating the magnitude of the press of the current interactor and applying the deadzone to it
                pressVector = Vector3.Project(pressVector, ButtonWorldAxis);
                float magnitude = pressVector.magnitude - m_pressDeadzone;

                // Updating the deepest press if appropriate
                if (magnitude > pressDepth) {
                    pressDepth = magnitude;
                }
            }
        }
        // The press depth is returned clamped between 0 and pressDistance
        return Mathf.Clamp(pressDepth, 0, m_pressDistance);
    }

    /// <summary>
    /// Sets the button value and calls value events if appropriate. Value is expected to be in a 0..1 range
    /// </summary>
    /// <param name="value">New value in a 0..1 range</param>
    void SetValue(float value) {
        if (m_prevValue != value) {
            m_value = value;
            m_onValueChange.Invoke(value);
            if (!m_pressed) {
                if (m_value > m_pressTriggerValue) {
                    ProcessPress();
                    m_pressed = true;
                }
            } else {
                if (m_value < m_releaseTriggerValue) {
                    m_onRelease.Invoke();
                    m_pressed = false;
                }
            }
            m_prevValue = m_value;
        }
    }

    /// <summary>
    /// Invokes the OnPress event while respecting the button press cooldown
    /// </summary>
    void ProcessPress() {
        // Cooldown is introduced to avoid rapid reactivations due to the natural hand tremor
        if (!m_onCooldown) {
            m_onPress.Invoke();
            m_onCooldown = true;
            StartCoroutine(StartCooldown());
        }
    }

    /// <summary>
    /// Sets onCooldown to false after a delay of reactivationCooldown
    /// </summary>
    IEnumerator StartCooldown() {
        yield return new WaitForSeconds(m_reactivationCooldown);
        m_onCooldown = false;
    }

    /// <summary>
    /// Positions the button by depth along the movement axis
    /// </summary>
    /// <param name="depth">Depth of the press along the movement axis</param>
    void SetButtonDepth(float depth) {
        Vector3 pressVector = ButtonWorldAxis * depth;
        m_button.position = m_buttonAttach.position + m_baseOffset - pressVector;
    }

    /// <summary>
    /// Calculates and saves the offset of the button from the button base. Make sure that both of them are in their initial relative positions when calling this.
    /// </summary>
    public void SaveButtonOffset() {
        m_baseOffset = m_button.position - m_buttonAttach.position;
    }
}
