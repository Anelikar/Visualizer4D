using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A behaviour that constantlu calculates fps and ms passed and displays them on a TMP text
/// </summary>
public class DisplayFPS : MonoBehaviour
{
    [Tooltip("Good fps threshold")]
    [SerializeField]
    float m_goodFPS = 72;
    [Tooltip("Bad fps threshold")]
    [SerializeField]
    float m_badFPS = 50;

    [Tooltip("An interval at which the text output is updated")]
    [SerializeField]
    float m_updateInteval = 0.5f;

    [Tooltip("TMP text that will show current fps and ms passed")]
    [SerializeField]
    TextMeshProUGUI m_textOutput = null;

    float m_deltaTime = 0.0f;
    float m_milliseconds = 0.0f;
    int m_framesPerSecond = 0;

    void Awake() {
        if (m_textOutput == null) {
            m_textOutput = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void Update() {
        CalculateCurrentFPS();
    }

    void OnEnable() {
        StartCoroutine(ShowFPS());
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    /// <summary>
    /// Calculates current FPS and milliseconds passed from previous frame
    /// </summary>
    void CalculateCurrentFPS() {
        // Updating deltaTime 
        m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;

        // Converting timing to milliseconds
        m_milliseconds = (m_deltaTime * 1000.0f);

        // Calculating current fps
        m_framesPerSecond = (int)(1.0f / m_deltaTime);
    }

    /// <summary>
    /// A couroutine that updates textOutput on an updateInterval with current fps and ms passed
    /// </summary>
    IEnumerator ShowFPS() {
        while (true) {
            // Updating text color to highlight fps fluctuations
            if (m_framesPerSecond >= m_goodFPS) {
                m_textOutput.color = Color.green;
            } else if (m_framesPerSecond >= m_badFPS) {
                m_textOutput.color = Color.yellow;
            } else {
                m_textOutput.color = Color.red;
            }

            m_textOutput.text = "FPS:" + m_framesPerSecond + "\n" + "MS:" + m_milliseconds.ToString(".0");
            yield return new WaitForSeconds(m_updateInteval);
        }
    }
}
