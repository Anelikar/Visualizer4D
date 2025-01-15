using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A behavoiur that connects interactive UI controls with a XRSettingsManager
/// </summary>
public class GlobalSettingsConnection : MonoBehaviour
{
    [Tooltip("A reference to a XRSettingsManager")]
    [SerializeField]
    XRSettingsManager m_settingsManager;

    [Header("Controls")]
    [Tooltip("A reference to a XRInteractable lever that changhes the movement mode")]
    [SerializeField]
    LeverInteractable m_moveModeLever;
    [Tooltip("A reference to a XRInteractable lever that changhes the controller used for continious movement")]
    [SerializeField]
    LeverInteractable m_moveHandLever;
    [Tooltip("A reference to a XRInteractable knob that controls movement speed for continious movement")]
    [SerializeField]
    KnobInteractable m_moveSpeedKnob;
    [Tooltip("A reference to a XRInteractable lever that changhes the turning mode")]
    [SerializeField]
    LeverInteractable m_turnModeLever;
    [Tooltip("A reference to a XRInteractable knob that controls turning speed for continious turning")]
    [SerializeField]
    KnobInteractable m_turnSpeedContKnob;
    [Tooltip("A reference to a XRInteractable knob that controls turning speed for snap turning")]
    [SerializeField]
    KnobInteractable m_turnSpeedSnapKnob;

    [Header("Labels")]
    [Tooltip("A reference to one of the TMPTexts that labels the movement hand lever")]
    [SerializeField]
    TMP_Text m_moveHandLabel;
    [Tooltip("A reference to one of the TMPTexts that labels the movement hand lever")]
    [SerializeField]
    TMP_Text m_moveHandLeftLabel;
    [Tooltip("A reference to one of the TMPTexts that labels the movement hand lever")]
    [SerializeField]
    TMP_Text m_moveHandRightLabel;
    [Tooltip("A reference to a TMPText that labels the movement speed knob")]
    [SerializeField]
    TMP_Text m_moveSpeedLabel;
    [Tooltip("A reference to a TMPText that displays the current movement speed text")]
    [SerializeField]
    TMP_Text m_moveSpeedText;
    [Tooltip("A reference to a TMPText that displays the current turning speed text")]
    [SerializeField]
    TMP_Text m_turnSpeedText;

    void Awake() {
        // Getting XRSettingsManager singleton instance
        if (m_settingsManager == null) {
            m_settingsManager = XRSettingsManager.Instance;
        }

        // Synchronizing current settings with controls
        if (m_settingsManager.Movement == XRSettingsManager.MovementMode.smooth) {
            SetMoveContActive(true);
        } else {
            SetMoveContActive(false);
        }
        SetMoveSpeedText(m_settingsManager.MovementSpeed);

        if (m_settingsManager.Turn == XRSettingsManager.TurnMode.smooth) {
            SetTurnContActive(true);
            SetTurnSpeedText(m_settingsManager.TurnSpeedCont, false);
        } else {
            SetTurnContActive(false);
            SetTurnSpeedText(m_settingsManager.TurnSpeedSnap, true);
        }
    }

    void OnEnable() {
        m_moveModeLever.OnMinValue.AddListener(SetMoveCont);
        m_moveModeLever.OnMaxValue.AddListener(SetMoveTeleport);
        m_moveHandLever.OnMinValue.AddListener(SetMoveRight);
        m_moveHandLever.OnMaxValue.AddListener(SetMoveLeft);
        m_moveSpeedKnob.OnValueChange.AddListener(SetMoveSpeed);
        m_turnModeLever.OnMinValue.AddListener(SetTurnCont);
        m_turnModeLever.OnMaxValue.AddListener(SetTurnSnap);
        m_turnSpeedSnapKnob.OnValueChange.AddListener(SetTurnSpeedSnap);
        m_turnSpeedContKnob.OnValueChange.AddListener(SetTurnSpeedCont);
    }

    void OnDisable() {
        m_moveModeLever.OnMinValue.RemoveListener(SetMoveCont);
        m_moveModeLever.OnMaxValue.RemoveListener(SetMoveTeleport);
        m_moveHandLever.OnMinValue.RemoveListener(SetMoveRight);
        m_moveHandLever.OnMaxValue.RemoveListener(SetMoveLeft);
        m_moveSpeedKnob.OnValueChange.RemoveListener(SetMoveSpeed);
        m_turnModeLever.OnMinValue.RemoveListener(SetTurnCont);
        m_turnModeLever.OnMaxValue.RemoveListener(SetTurnSnap);
        m_turnSpeedSnapKnob.OnValueChange.RemoveListener(SetTurnSpeedSnap);
        m_turnSpeedContKnob.OnValueChange.RemoveListener(SetTurnSpeedCont);
    }

    /// <summary>
    /// Enables everything for teleportation movement mode and disables everything for continious movement
    /// </summary>
    public void SetMoveTeleport() {
        m_settingsManager.Movement = XRSettingsManager.MovementMode.teleportation;
        SetMoveContActive(false);
    }

    /// <summary>
    /// Enables everything for continious movement mode and disables everything for teleport movement
    /// </summary>
    public void SetMoveCont() {
        m_settingsManager.Movement = XRSettingsManager.MovementMode.smooth;
        SetMoveContActive(true);
    }

    /// <summary>
    /// Changhes the controller used for continious movement to the left one
    /// </summary>
    public void SetMoveLeft() {
        m_settingsManager.MovementHand = XRSettingsManager.Hand.left;
    }

    /// <summary>
    /// Changhes the controller used for continious movement to the right one
    /// </summary>
    public void SetMoveRight() {
        m_settingsManager.MovementHand = XRSettingsManager.Hand.right;
    }

    /// <summary>
    /// Sets new continious movement speed value and updates lables
    /// </summary>
    /// <param name="value">New continious movement speed value</param>
    public void SetMoveSpeed(float value) {
        m_settingsManager.MovementSpeed = value;
        SetMoveSpeedText(value);
    }

    /// <summary>
    /// Enables everything for snap turning mode and disables everything for continious turning
    /// </summary>
    public void SetTurnSnap() {
        m_settingsManager.Turn = XRSettingsManager.TurnMode.snap;
        SetTurnContActive(false);
        SetTurnSpeedText(m_settingsManager.TurnSpeedSnap, true);
    }

    /// <summary>
    /// Enables everything for continious turning mode and disables everything for snap turning
    /// </summary>
    public void SetTurnCont() {
        m_settingsManager.Turn = XRSettingsManager.TurnMode.smooth;
        SetTurnContActive(true);
        SetTurnSpeedText(m_settingsManager.TurnSpeedCont, false);
    }

    /// <summary>
    /// Sets new snap turning speed value and updates lables
    /// </summary>
    /// <param name="value">New snap turning speed value</param>
    public void SetTurnSpeedSnap(float value) {
        m_settingsManager.TurnSpeedSnap = value;
        SetTurnSpeedText(value, true);
    }

    /// <summary>
    /// Sets new continious turning speed value and updates lables
    /// </summary>
    /// <param name="value">New continious turning speed value</param>
    public void SetTurnSpeedCont(float value) {
        m_settingsManager.TurnSpeedCont = value;
        SetTurnSpeedText(value, false);
    }

    /// <summary>
    /// Enables or disables labels and controls for continious movement mode
    /// </summary>
    /// <param name="active">Should the elemonts be active?</param>
    void SetMoveContActive(bool active) {
        m_moveHandLever.gameObject.SetActive(active);
        m_moveHandLabel.gameObject.SetActive(active);
        m_moveHandLeftLabel.gameObject.SetActive(active);
        m_moveHandRightLabel.gameObject.SetActive(active);
        m_moveSpeedKnob.gameObject.SetActive(active);
        m_moveSpeedLabel.gameObject.SetActive(active);
        m_moveSpeedText.gameObject.SetActive(active);
    }

    /// <summary>
    /// Enables or disables controls for continious turning mode
    /// </summary>
    /// <param name="active">Should the elemonts be active?</param>
    void SetTurnContActive(bool active) {
        m_turnSpeedContKnob.gameObject.SetActive(active);
        m_turnSpeedSnapKnob.gameObject.SetActive(!active);
    }

    /// <summary>
    /// Sets the movement speed display to a new value
    /// </summary>
    /// <param name="value">New movement speed value</param>
    void SetMoveSpeedText(float value) {
        m_moveSpeedText.text = $"{value:f1} m/s";
    }

    /// <summary>
    /// Sets the turning speed display to a new value
    /// </summary>
    /// <param name="value">New turning speed value</param>
    /// <param name="snap">Should this update snap (true) or continious (false) display?</param>
    void SetTurnSpeedText(float value, bool snap) {
        if (snap) {
            m_turnSpeedText.text = $"{value:f1} deg/s";
        } else {
            m_turnSpeedText.text = $"{value:f1} deg";
        }
    }
}
