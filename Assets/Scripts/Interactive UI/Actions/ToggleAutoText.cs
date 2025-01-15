using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides a method to toggle automatic text advancing on the connected InfoUI
/// </summary>
public class ToggleAutoText : MonoBehaviour
{
    [Tooltip("InfoUI that will have its automatic text advancing toggled by this behaviour")]
    [SerializeField]
    InfoUI m_infoUI;

    /// <summary>
    /// Toggles automatic text advancing on the connected InfoUI
    /// </summary>
    public void ToggleAuto() {
        if (m_infoUI.Auto) {
            m_infoUI.Auto = false;
        } else {
            m_infoUI.Auto = true;
        }
    }
}
