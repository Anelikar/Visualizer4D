using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fires the OnToggled event after the CountPress was called enough times
/// </summary>
public class ToggleOnPressCount : MonoBehaviour
{
    [Tooltip("Number of times CountPress needs to be called for the toggle to happen")]
    [SerializeField]
    float m_pressCountThreshold = 5;

    [Tooltip("Maximim time since the last call to CountPress before the counter resets")]
    [SerializeField]
    float m_pressTimeThreshold = 1f;

    [Tooltip("The first time this event will be invoked when the CountPress is invoked a sufficient number of times.\n" +
        "The second time it will be invoked just after a single call")]
    [SerializeField]
    UnityEvent m_onToggled;
    public UnityEvent OnToggled => m_onToggled;

    int m_presses = 0;
    float m_lastPressTime = 0;
    bool m_displayActive = false;

    public void CountPress() {
        if (!m_displayActive) {
            if (Time.time - m_lastPressTime < m_pressTimeThreshold) {
                m_presses++;
            } else {
                m_presses = 1;
            }
            m_lastPressTime = Time.time;

            if (m_presses > m_pressCountThreshold) {
                m_onToggled.Invoke();
                m_displayActive = true;
                m_presses = 0;
            }
        } else {
            m_onToggled.Invoke();
            m_displayActive = false;
            m_presses = 0;
        }
    }
}
