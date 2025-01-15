using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A behaviour that provides methods to toggle between direct and ray interactors
/// </summary>
public class ToggleRay : MonoBehaviour
{
    [Tooltip("The direct interactor that is being toggled")]
    [SerializeField]
    XRDirectInteractor m_directInteractor;
    [Tooltip("The ray interactor that is being toggled. \n\nWill be set to this object if left empty.")]
    [SerializeField]
    XRRayInteractor m_rayInteractor;
    /// <summary>
    /// The direct interactor that is being toggled
    /// </summary>
    public XRDirectInteractor DirectInteractor {
        get => m_directInteractor;
        set => m_directInteractor = value;
    }
    /// <summary>
    /// The ray interactor that is being toggled. Will be set to this object if left empty.
    /// </summary>
    public XRRayInteractor RayInteractor {
        get => m_rayInteractor;
        set => m_rayInteractor = value;
    }

    [Tooltip("Should interactors switch even if an object is selected?")]
    [SerializeField]
    bool m_forceToggle = false;
    /// <summary>
    /// Should interactors switch even if an object is selected?
    /// </summary>
    public bool ForceToggle {
        get => m_forceToggle;
        set => m_forceToggle = value;
    }

    bool m_isSwitched = false;

    void Awake() {
        if (m_rayInteractor == null) {
            m_rayInteractor = GetComponent<XRRayInteractor>();
        }

        // Initializing interactor states
        EnableRay(false);
    }

    /// <summary>
    /// Enables the ray interactor if appropriate
    /// </summary>
    public void ActivateRay() {
        if (!IsTouchingInteractable() || m_forceToggle)
            EnableRay(true);
    }

    /// <summary>
    /// Disables the ray interactor if nesessary
    /// </summary>
    public void DeactivateRay() {
        if (m_isSwitched)
            EnableRay(false);
    }

    /// <summary>
    /// Checks if the direct interactor is currently touching an interactable
    /// </summary>
    /// <returns>True, if the intaractor is touching any interactables, otherwise false</returns>
    bool IsTouchingInteractable() {
        List<IXRInteractable> targets = new List<IXRInteractable>();
        m_directInteractor.GetValidTargets(targets);
        return (targets.Count > 0);
    }

    /// <summary>
    /// Sets enabled of the ray intaractor to the provided value and the direct interactor to the opposite
    /// </summary>
    /// <param name="value">Should the ray interactor be enabled?</param>
    void EnableRay(bool value) {
        m_isSwitched = value;
        m_rayInteractor.enabled = value;
        m_directInteractor.enabled = !value;
    }
}
