using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A behaviour that plays a sound when the connected knob value is changed
/// </summary>
public class KnobSound : BaseSound
{
    [Tooltip("Connected KnobIntaractable. Will be set to this object's interactable if left empty.")]
    [SerializeField]
    KnobInteractable m_interactable;

    [Tooltip("Minimum knob value change for the sound to be played")]
    [SerializeField]
    float m_clickThreshold = 0.1f;
    /// <summary>
    /// Minimum knob value change for the sound to be played
    /// </summary>
    public float ClickThreshold {
        get => m_clickThreshold;
        set => m_clickThreshold = value;
    }

    float m_prevValue;

    protected override void Awake() {
        base.Awake();

        if (m_interactable == null) {
            m_interactable = GetComponent<KnobInteractable>();
        }
        m_prevValue = m_interactable.Value;
    }

    void OnEnable() {
        m_interactable.OnValueChange.AddListener(Play);
    }

    void OnDisable() {
        m_interactable.OnValueChange.RemoveListener(Play);
    }

    /// <summary>
    /// Plays a sound when the value is changed by more then the ClickThreshold
    /// </summary>
    /// <param name="value">Corrent value of the knob</param>
    public void Play(float value) {
        if (Mathf.Abs(value - m_prevValue) >= m_clickThreshold) {
            m_prevValue = value;
            m_quickSound.PlayQuickSound();
        }
    }
}
