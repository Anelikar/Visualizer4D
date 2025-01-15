using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that will play a sound when the connected rigidbody collides with something.
/// </summary>
public class CollisionSound : BaseSound
{
    [Tooltip("A rigidbody that will be tested for collisions.\n" +
        "Will be set to this gameobject's rigidbody if left empty.")]
    [SerializeField]
    Rigidbody m_rigidbody;
    /// <summary>
    /// A rigidbody that will be tested for collisions. Will be set to this gameobject's rigidbody if left empty.
    /// </summary>
    public Rigidbody Rigidbody {
        get => m_rigidbody;
        set => m_rigidbody = value;
    }

    [Tooltip("Collision sound will be multiplied by this value")]
    [SerializeField]
    float m_volumeMultiplier = 1f;
    [Tooltip("Minimum time that has to pass between sound activations")]
    [SerializeField]
    float m_cooldownTime = 0.1f;
    /// <summary>
    /// Collision sound will be multiplied by this value
    /// </summary>
    public float VolumeMultiplier {
        get => m_volumeMultiplier;
        set => m_volumeMultiplier = value;
    }
    /// <summary>
    /// Minimum time that has to pass between sound activations
    /// </summary>
    public float CooldownTime {
        get => m_cooldownTime;
        set => m_cooldownTime = value;
    }

    float m_timeOfLastActivation;

    protected override void Awake() {
        base.Awake();

        if (m_rigidbody == null) {
            m_rigidbody = GetComponent<Rigidbody>();
        }
    }

    void OnCollisionEnter(Collision collision) {
        // The sound will only be played if sufficient time has passed since the last time
        if (Time.time - m_timeOfLastActivation > m_cooldownTime) {
            // Modifying the volume by the magnitude of the rigidbody and VolumeMultiplier
            m_quickSound.Volume = m_rigidbody.velocity.magnitude * m_volumeMultiplier;
            m_quickSound.PlayQuickSound();

            // Saving the time to use it as a start of the cooldown
            m_timeOfLastActivation = Time.time;
        }
    }
}
