using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that privides a method that swaps active states on 2 gameobjects
/// </summary>
public class SwapActive : MonoBehaviour
{
    // Naming is legacy and is fairly hard to change as it will disconnect all references in the editor
    [Tooltip("Initally active gameobject that will have its active state toggled by this behaviour")]
    [SerializeField]
    GameObject m_controls1;
    [Tooltip("Initally inactive gameobject that will have its active state toggled by this behaviour")]
    [SerializeField]
    GameObject m_controls2;

    bool m_active1 = true;

    /// <summary>
    /// Swaps active states on connected gameobjects
    /// </summary>
    public void Swap() {
        if (m_active1) {
            m_controls1.SetActive(false);
            m_controls2.SetActive(true);
            m_active1 = false;
        } else {
            m_controls1.SetActive(true);
            m_controls2.SetActive(false);
            m_active1 = true;
        }
    }
}
