using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A behaviour that will play a sound when the connected interactable is grabbed or released
/// </summary>
public class GrabSounds : BaseSound
{
    [Tooltip("Connected XRIntaractable. Will be set to this object's interactable if left empty.")]
    [SerializeField]
    XRBaseInteractable m_interactable;
    [Tooltip("The sound will be pitched by tis value on grab")]
    [SerializeField]
    float m_grabPitch = 1.1f;
    [Tooltip("The sound will be pitched by tis value on release")]
    [SerializeField]
    float m_releasePitch = 0.9f;
    /// <summary>
    /// The sound will be pitched by tis value on grab
    /// </summary>
    public float GrabPitch {
        get => m_grabPitch;
        set => m_grabPitch = value;
    }
    /// <summary>
    /// The sound will be pitched by tis value on release
    /// </summary>
    public float ReleasePitch {
        get => m_releasePitch;
        set => m_releasePitch = value;
    }

    protected override void Awake() {
        base.Awake();

        if (m_interactable == null) {
            m_interactable = GetComponent<XRBaseInteractable>();
        }
    }

    void OnEnable() {
        if (m_interactable != null && m_quickSound != null) {
            m_interactable.selectEntered.AddListener(PlayGrab);
            m_interactable.selectExited.AddListener(PlayRelease);
        }
    }

    void OnDisable() {
        m_interactable.selectEntered.RemoveListener(PlayGrab);
        m_interactable.selectExited.RemoveListener(PlayRelease);
    }

    /// <summary>
    /// Plays a sound with a grab pitch
    /// </summary>
    /// <param name="_">Discarded</param>
    void PlayGrab(SelectEnterEventArgs _) {
        m_quickSound.PlayQuickSound(m_grabPitch);
    }

    /// <summary>
    /// Plays a sound with a release pitch
    /// </summary>
    /// <param name="_">Discarded</param>
    void PlayRelease(SelectExitEventArgs _) {
        m_quickSound.PlayQuickSound(m_releasePitch);
    }
}
