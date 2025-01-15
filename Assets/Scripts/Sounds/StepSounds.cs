using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that will imitate step sounds when the connected gameobject moves
/// </summary>
public class StepSounds : BaseSound
{
    [Tooltip("A sound will play every time this gameobject moves over this distance")]
    [SerializeField]
    float m_distanceToStep = 0.3f;
    [Tooltip("A sound will play after this time if this gameobject moved, but didn't travel the full distance for a step")]
    [SerializeField]
    float m_timeToStopStep = 1f;
    [Tooltip("A deadzone that will not activate the time counter")]
    [SerializeField]
    float m_driftAllowance = 0.01f;
    /// <summary>
    /// A sound will play every time this gameobject moves over this distance
    /// </summary>
    public float DistanceToStep {
        get => m_distanceToStep;
        set => m_distanceToStep = value;
    }
    /// <summary>
    /// A sound will play after this time if this gameobject moved, but didn't travel the full distance for a step
    /// </summary>
    public float TimeToStopStep {
        get => m_timeToStopStep;
        set => m_timeToStopStep = value;
    }
    /// <summary>
    /// A deadzone that will not activate the time counter
    /// </summary>
    public float DriftAllowance {
        get => m_driftAllowance;
        set => m_driftAllowance = value;
    }

    float m_distanceSinceStep;
    Vector3 m_prevPosition;

    bool m_hasMoved = false;
    float m_timeSinceMove;

    protected override void Awake() {
        base.Awake();

        m_prevPosition = transform.position;
    }

    void OnEnable() {
        m_prevPosition = transform.position;
        m_distanceSinceStep = 0;
        m_timeSinceMove = 0;
        m_hasMoved = false;
    }

    void FixedUpdate() {
        float positionDelta = (transform.position - m_prevPosition).magnitude;
        if (positionDelta > m_driftAllowance) {
            // Adding the distance and starting the time counter
            m_hasMoved = true;
            m_distanceSinceStep += positionDelta;
            m_prevPosition = transform.position;

            // Checking the distance counter
            if (m_distanceSinceStep >= m_distanceToStep) {
                Step();
            }
        }
        
        if (m_hasMoved) {
            // Updating the time counter
            m_timeSinceMove += Time.deltaTime;

            // Checking the time counter
            if (m_timeSinceMove >= m_timeToStopStep) {
                Step();
            }
        }
    }

    /// <summary>
    /// Plays a sound and resets distance and time counters
    /// </summary>
    void Step() {
        m_quickSound.PlayQuickSound();
        m_distanceSinceStep = 0;
        m_timeSinceMove = 0;
        m_hasMoved = false;
    }
}
