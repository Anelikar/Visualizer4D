using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that can be used to make a light flicker at random intervals
/// </summary>
public class FlickeringLight : MonoBehaviour
{
    [Tooltip("Minimum time before a flash")]
    [SerializeField]
    float m_minFlashInterval = 0.1f;
    [Tooltip("Maximum time before a flash")]
    [SerializeField]
    float m_maxFlashInterval = 0.5f;
    [Tooltip("Length of a flash")]
    [SerializeField]
    float m_flashTime = 0.05f;
    /// <summary>
    /// Minimum time before a flash
    /// </summary>
    public float MinFlashInterval {
        get => m_minFlashInterval;
        set => m_minFlashInterval = value;
    }
    /// <summary>
    /// Maximum time before a flash
    /// </summary>
    public float MaxFlashInterval {
        get => m_maxFlashInterval;
        set => m_maxFlashInterval = value;
    }
    /// <summary>
    /// Length of a flash
    /// </summary>
    public float FlashTime {
        get => m_flashTime;
        set => m_flashTime = value;
    }

    [Tooltip("Minimum light range while flashing")]
    [SerializeField]
    float m_minFlashRange = 10;
    [Tooltip("Maximum light range while flashing")]
    [SerializeField]
    float m_maxFlashRange = 10;
    /// <summary>
    /// Minimum light range while flashing
    /// </summary>
    public float MinFlashRange {
        get => m_minFlashRange;
        set => m_minFlashRange = value;
    }
    /// <summary>
    /// Maximum light range while flashing
    /// </summary>
    public float MaxFlashRange {
        get => m_maxFlashRange;
        set => m_maxFlashRange = value;
    }

    Light m_light;
    float m_baseLightRange;

    float m_currentTimer = 0;
    float m_currentInterval;
    bool m_isFlashing = false;

    void Awake() {
        m_light = GetComponent<Light>();
        UpdateBaseLightRange();

        m_currentInterval = Random.Range(m_minFlashInterval, m_maxFlashInterval);
    }

    void Update() {
        m_currentTimer += Time.deltaTime;
        if (m_isFlashing) {
            if (m_currentTimer > m_currentInterval + m_flashTime) {
                // Ending the flash
                m_light.range = m_baseLightRange;
                m_currentTimer = 0;
                m_currentInterval = Random.Range(m_minFlashInterval, m_maxFlashInterval);
                m_isFlashing = false;
            }
        } else if (m_currentTimer > m_currentInterval) {
            // Starting the flash
            m_light.range = Random.Range(m_minFlashRange, m_maxFlashRange);
            m_isFlashing = true;
        }
    }

    /// <summary>
    /// This should be called if the light range is changed externally
    /// </summary>
    public void UpdateBaseLightRange() {
        m_baseLightRange = m_light.range;
    }
}
