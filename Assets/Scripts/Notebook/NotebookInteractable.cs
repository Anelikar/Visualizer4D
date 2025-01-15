using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// An XRGrabInteractable extension that connects sets animator bools on SelectEntered and SelectExited
/// </summary>
public class NotebookInteractable : XRGrabInteractable
{
    [Header("Notebook properties")]
    [Tooltip("Notebook animator that will have its bools set")]
    [SerializeField]
    Animator m_animator;

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        base.OnSelectEntered(args);

        m_animator.SetBool("opened", true);
    }

    protected override void OnSelectExited(SelectExitEventArgs args) {
        base.OnSelectExited(args);

        m_animator.SetBool("opened", false);
    }
}
