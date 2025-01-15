using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides methods to play one-shot sounds from a list with provided parameters
/// </summary>
public class QuickSoundFromList : QuickSound
{
    [Tooltip("A list of clips that will be played")]
    [SerializeField]
    List<AudioClip> m_clips;
    /// <summary>
    /// A list of clips that will be played
    /// </summary>
    public List<AudioClip> Clips => m_clips;

    protected override void Awake() {
        base.Awake();
        if (m_clips == null) {
            m_clips = new List<AudioClip>();
        }
    }

    /// <summary>
    /// Plays a sound with a specified index once, with the provided pitch modifier
    /// </summary>
    /// <param name="basePitch">Pitch modifier of the sound</param>
    /// <param name="clipIndex">Index of the clip</param>
    public void PlayQuickSound(float basePitch, int clipIndex) {
        if (m_blockingMode && m_audioSource.isPlaying) {
            return;
        }
        if (m_audioSource == null) {
            Debug.LogError($"QuickSoundFromList at {gameObject.name} is missing an AudioSource.");
            return;
        }
        if (m_clips.Count == 0) {
            if (m_clip != null) {
                m_clips.Add(m_clip);
            } else {
                Debug.LogError($"QuickSoundFromList at {gameObject.name} is missing an AudioClip.");
                return;
            }
        }

        // Randimizing pitch
        float pitch = (Random.value * 2 - 1) * m_pitchVariance + basePitch;

        m_audioSource.pitch = Mathf.Clamp(pitch, -3, 3);
        m_audioSource.PlayOneShot(m_clips[clipIndex], m_volume);
    }

    /// <summary>
    /// Plays a sound with a specified index once
    /// </summary>
    /// <param name="clipIndex">Index of the clip</param>
    public void PlayQuickSoundFromList(int clipIndex) {
        PlayQuickSound(m_basePitch, clipIndex);
    }

    /// <summary>
    /// Plays a random sound from a list once with the provided pitch modifier
    /// </summary>
    /// <param name="basePitch">Pitch modifier of the sound</param>
    public override void PlayQuickSound(float basePitch) {
        int clipIndex = (int)Mathf.Round(Random.value * (m_clips.Count - 1));
        PlayQuickSound(basePitch, clipIndex);
    }

    /// <summary>
    /// Plays a random sound from a list once
    /// </summary>
    public override void PlayQuickSound() {
        PlayQuickSound(m_basePitch);
    }
}
