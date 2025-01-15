using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// An XRSocketInteractor extension that provides events to react to a brush moving over it
/// </summary>
public class CanvasSocket : XRSocketInteractor
{
    [Header("Canvas properties")]
    [Tooltip("A minimum distance the interactor needs to move to invoke OnInteractorMove")]
    [SerializeField]
    float m_drawDeadzone = 0.01f;
    /// <summary>
    /// A minimum distance the interactor needs to move to invoke OnInteractorMove
    /// </summary>
    public float DrawDeadzone {
        get => m_drawDeadzone;
        set => m_drawDeadzone = value;
    }

    [Tooltip("This will be called when an interactor moves while hovering over a socket")]
    [SerializeField]
    UnityEvent<Vector3> m_onInteractorMove;
    /// <summary>
    /// This will be called when an interactor moves while hovering over a socket
    /// </summary>
    public UnityEvent<Vector3> OnInteractorMove => m_onInteractorMove;

    /// <summary>
    /// Currently hovering interactors
    /// </summary>
    List<IXRHoverInteractable> m_interactors;
    /// <summary>
    /// Currently active interactor
    /// </summary>
    Transform m_mainInteractor;
    Vector3 m_oldInteractorPos;

    protected override void Awake() {
        base.Awake();

        m_interactors = new List<IXRHoverInteractable>();
    }

    protected override void OnEnable() {
        base.OnEnable();

        hoverEntered.AddListener(HoverEnter);
        hoverExited.AddListener(HoverExit);
    }

    protected override void OnDisable() {
        base.OnDisable();

        hoverEntered.RemoveListener(HoverEnter);
        hoverExited.RemoveListener(HoverExit);
    }

    public override bool CanSelect(IXRSelectInteractable interactable) {
        // This interactor only needs to detect hovering
        return false;
    }

    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractor(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed && m_mainInteractor != null) {
            // Checking that the interactor moved beyond the draw deadzone
            if ((m_oldInteractorPos - m_mainInteractor.position).magnitude > m_drawDeadzone) {
                m_onInteractorMove.Invoke(m_mainInteractor.position);
                m_oldInteractorPos = m_mainInteractor.position;
            }
        }
    }

    void HoverEnter(HoverEnterEventArgs args) {
        // Adding interactor to the list of hovering interactors
        m_interactors.Add(args.interactableObject);

        if (m_mainInteractor == null) {
            // Making newly added interactor be the active one
            m_mainInteractor = args.interactableObject.transform;
            m_oldInteractorPos = m_mainInteractor.position;
        }
    }

    void HoverExit(HoverExitEventArgs args) {
        // Removing interactor form the list of hovering interactors
        m_interactors.Remove(args.interactableObject);

        // Checking if the exiting interactor was the active one
        if (m_mainInteractor == args.interactableObject.transform) {
            if (m_interactors.Count > 0) {
                // Making the next added interactor be the active one
                m_mainInteractor = m_interactors[m_interactors.Count - 1].transform;
                m_oldInteractorPos = m_mainInteractor.position;
            } else {
                m_mainInteractor = null;
            }
        }
    }
}
