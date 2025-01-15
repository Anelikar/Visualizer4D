using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides methods to play one-shot sounds with provided parameters
/// </summary>
public class QuickSound : MonoBehaviour
{
    [Tooltip("Audio source of the sound. Will be set to this object's AudioSource if left empty.")]
    [SerializeField]
    protected AudioSource m_audioSource;
    [Tooltip("A clip that will be played")]
    [SerializeField]
    protected AudioClip m_clip;
    /// <summary>
    /// Audio source of the sound. Will be set to this object's AudioSource if left empty.
    /// </summary>
    public AudioSource Source {
        get => m_audioSource;
        set => m_audioSource = value;
    }
    /// <summary>
    /// A clip that will be played
    /// </summary>
    public AudioClip Clip {
        get => m_clip;
        set => m_clip = value;
    }

    [Tooltip("Volume modifier of the sound")]
    [Range(0, 1)]
    [SerializeField]
    protected float m_volume = 1;
    [Tooltip("Random pitch variance of the sound")]
    [Range(0, 3)]
    [SerializeField]
    protected float m_pitchVariance = 0;
    [Tooltip("Base pitch modifier of the sound")]
    [Range(-3, 3)]
    [SerializeField]
    protected float m_basePitch = 1;
    /// <summary>
    /// Volume modifier of the sound. Will be clamped in a 0..1 range 
    /// </summary>
    public float Volume {
        get => m_volume;
        set => m_volume = Mathf.Clamp01(value);
    }
    /// <summary>
    /// Random pitch variance of the sound. Will be clamped in a 0..3 range 
    /// </summary>
    public float PitchVariance {
        get => m_pitchVariance;
        set => m_pitchVariance = Mathf.Clamp(value, 0, 2);
    }
    /// <summary>
    /// Base pitch modifier of the sound. Will be clamped in a 0..2 range 
    /// </summary>
    public float BasePitch {
        get => m_basePitch;
        set => m_basePitch = Mathf.Clamp(value, -3, 3);
    }

    [Tooltip("If true, clips will only play when the previous one is stopped.")]
    [SerializeField]
    protected bool m_blockingMode = false;
    /// <summary>
    /// If true, clips will only play when the previous one is stopped.
    /// </summary>
    public bool Blocking {
        get => m_blockingMode;
        set => m_blockingMode = value;
    }

    protected virtual void Awake() {
        if (m_audioSource == null) {
            m_audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Plays a sound once with the provided pitch modifier
    /// </summary>
    /// <param name="basePitch">Pitch modifier of the sound</param>
    public virtual void PlayQuickSound(float basePitch) {
        if (m_blockingMode && m_audioSource.isPlaying) {
            return;
        }
        if (m_audioSource == null) {
            Debug.LogError($"QuickSound at {gameObject.name} is missing an AudioSource.");
            return;
        }
        if (m_clip == null) {
            Debug.LogError($"QuickSound at {gameObject.name} is missing an AudioClip.");
            return;
        }

        // Randimizing pitch
        float pitch = (Random.value * 2 - 1) * m_pitchVariance + basePitch;

        m_audioSource.pitch = Mathf.Clamp(pitch, -3, 3);
        m_audioSource.PlayOneShot(m_clip, m_volume);
    }

    /// <summary>
    /// Plays a sound once
    /// </summary>
    public virtual void PlayQuickSound() {
        PlayQuickSound(m_basePitch);
    }
}
