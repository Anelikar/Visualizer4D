using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides a method to toggle between two materials on a renderer
/// </summary>
public class ToggleMaterial : MonoBehaviour
{
    [Tooltip("Renderer that will have its materials toggled")]
    [SerializeField]
    MeshRenderer m_renderer;
    [SerializeField]
    [Tooltip("Initial Material. May be left empty. In that case it will bes set to the material present before the first toggle")]
    Material m_originalMaterial = null;
    [SerializeField]
    Material m_newMaterial;
    [Tooltip("Index of the material to toggle")]
    [SerializeField]
    int m_materialIndex = 0;

    bool m_isToggled = false;
    /// <summary>
    /// Readonly. Is the material currently toggled?
    /// </summary>
    public bool Toggled => m_isToggled;

    void Start() {
        if (m_originalMaterial == null) {
            UpdateOriginalMaterial();
        }
    }

    /// <summary>
    /// Toggles between the originalMaterial and the newMaterial
    /// </summary>
    public void Toggle() {
        Material[] mats = m_renderer.materials;

        if (m_isToggled) {
            if (m_originalMaterial == null) {
                UpdateOriginalMaterial();
            }
            mats[m_materialIndex] = m_originalMaterial;
            m_isToggled = false;
        } else {
            mats[m_materialIndex] = m_newMaterial;
            m_isToggled = true;
        }
        m_renderer.materials = mats;
    }

    /// <summary>
    /// Sets the current material as the original material
    /// </summary>
    public void UpdateOriginalMaterial() {
        m_originalMaterial = m_renderer.materials[m_materialIndex];
    }
}
