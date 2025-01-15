using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that provides methods to fade a canvas over time
/// </summary>
public class FadeCanvas : MonoBehaviour
{
    [Tooltip("The speed at which the canvas fades")]
    [SerializeField]
    float m_duration = 1.0f;
    [Tooltip("The speed at which the canvas quickfades")]
    [SerializeField]
    float m_quickDuration = 0.25f;

    /// <summary>
    /// The speed at which the canvas fades
    /// </summary>
    public float Duration {
        get => m_duration;
        set => m_duration = value;
    }
    /// <summary>
    /// The speed at which the canvas quickfades
    /// </summary>
    public float QuickDuration {
        get => m_quickDuration;
        set => m_quickDuration = value;
    }

    [Tooltip("Reference to a CanvasGroup. \n\nWill be set to this object if left empty")]
    [SerializeField]
    CanvasGroup m_canvasGroup = null;
    /// <summary>
    /// Reference to a CanvasGroup. Will be set to this object if left empty
    /// </summary>
    public CanvasGroup CanvasGroup {
        get => m_canvasGroup;
        set => m_canvasGroup = value;
    }

    Coroutine m_currentCoroutine = null;
    /// <summary>
    /// Currentlly running coroutine
    /// </summary>
    public Coroutine CurrentCoroutine => m_currentCoroutine;

    float m_alpha = 0.0f;

    void Awake() {
        if (m_canvasGroup == null) {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Starts a fade in animation for the connected canvas group
    /// </summary>
    public void StartFadeIn() {
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeIn(m_duration));
    }

    /// <summary>
    /// Starts a fade out animation for the connected canvas group
    /// </summary>
    public void StartFadeOut() {
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_duration));
    }

    /// <summary>
    /// Starts a quick fade in animation for the connected canvas group
    /// </summary>
    public void QuickFadeIn() {
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeIn(m_quickDuration));
    }

    /// <summary>
    /// Starts a quick fade out animation for the connected canvas group
    /// </summary>
    public void QuickFadeOut() {
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_quickDuration));
    }

    /// <summary>
    /// A coroutine method for fading in the connected canvas group
    /// </summary>
    /// <param name="duration">Duration of the fade in</param>
    IEnumerator FadeIn(float duration) {
        float elapsedTime = 0.0f;

        while (m_alpha <= 1.0f)
        {
            SetAlpha(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// A coroutine method for fading out the connected canvas group
    /// </summary>
    /// <param name="duration">Duration of the fade out</param>
    IEnumerator FadeOut(float duration) {
        float elapsedTime = 0.0f;

        while (m_alpha >= 0.0f)
        {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Sets the alpha of the connected canvas group to a new value
    /// </summary>
    /// <param name="value">New alpha</param>
    void SetAlpha(float value) {
        m_alpha = value;
        m_canvasGroup.alpha = m_alpha;
    }
}