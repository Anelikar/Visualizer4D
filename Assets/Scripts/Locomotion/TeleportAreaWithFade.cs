using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// An extension to the TeleportationArea that adds a fade sequence around the teleportation
/// </summary>
public class TeleportAreaWithFade : TeleportationArea
{
    [Header("Fade properties")]
    [Tooltip("Fade canvas that will be used in the animation. \n\nWill be attempted to be found in the scene if left empty.")]
    [SerializeField]
    FadeCanvas m_fadeCanvas;
    /// <summary>
    /// Fade canvas that will be used in the animation. Will be attempted to be found in the scene if left empty.
    /// </summary>
    public FadeCanvas FadeCanvas {
        get => m_fadeCanvas;
        set => m_fadeCanvas = value;
    }

    protected override void Awake() {
        base.Awake();

        if (m_fadeCanvas == null) {
            m_fadeCanvas = FindObjectOfType<FadeCanvas>();
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        base.OnSelectEntered(args);

        if (teleportTrigger == TeleportTrigger.OnSelectEntered) {
            StartCoroutine(FadeSequence(base.OnSelectEntered, args));
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args) {
        if (teleportTrigger == TeleportTrigger.OnSelectExited) {
            StartCoroutine(FadeSequence(base.OnSelectExited, args));
        }
    }

    protected override void OnActivated(ActivateEventArgs args) {
        if (teleportTrigger == TeleportTrigger.OnActivated) {
            StartCoroutine(FadeSequence(base.OnActivated, args));
        }
    }

    protected override void OnDeactivated(DeactivateEventArgs args) {
        if (teleportTrigger == TeleportTrigger.OnDeactivated) {
            StartCoroutine(FadeSequence(base.OnDeactivated, args));
        }
    }

    /// <summary>
    /// A coroutine method that fades the canvas in, invokes provided action then fades the canvas out
    /// </summary>
    /// <typeparam name="T">Type of arguments for the action that will be invoked</typeparam>
    /// <param name="action">Action that will be invoked in the middle of the sequence</param>
    /// <param name="args">Arguments for the action that will be invoked</param>
    IEnumerator FadeSequence<T>(UnityAction<T> action, T args) where T : BaseInteractionEventArgs {
        // Fading the canvas in
        m_fadeCanvas.QuickFadeIn();
        yield return m_fadeCanvas.CurrentCoroutine;

        // Invoking base interaction
        action.Invoke(args);

        // Fading the canvas out
        m_fadeCanvas.QuickFadeOut();
    }
}
