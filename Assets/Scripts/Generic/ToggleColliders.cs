using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides methods to toggle connected colliders
/// </summary>
public class ToggleColliders : MonoBehaviour
{
    [Tooltip("A collection of colliders that will be toggled. \n" +
        "Editing this list at runtime may desync the ToggleColliders method if the added collider is in a different state than the existing ones")]
    public Collider[] Colliders;

    bool m_enabled;

    void Awake() {
        m_enabled = true;
        if (Colliders != null) {
            // Checking if any of the colliders are disabled
            foreach (var item in Colliders) {
                if (!item.enabled) {
                    m_enabled = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Enables all connected colliders
    /// </summary>
    public void EnableColliders() {
        if (Colliders != null) {
            foreach (var item in Colliders) {
                item.enabled = true;
            }
            m_enabled = true;
        }
    }


    /// <summary>
    /// Disables all connected colliders
    /// </summary>
    public void DisableColliders() {
        if (Colliders != null) {
            foreach (var item in Colliders) {
                item.enabled = false;
            }
            m_enabled = true;
        }
    }

    /// <summary>
    /// Enables or disables all connected colliders based on the internal counter
    /// </summary>
    public void Toggle() {
        if (m_enabled) {
            DisableColliders();
        } else {
            EnableColliders();
        }
    }
}
