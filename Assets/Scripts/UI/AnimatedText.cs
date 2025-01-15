using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A behaviour that privides methods for fading the text in and out while moving it and smoothly changing its string contents
/// </summary>
public class AnimatedText : MonoBehaviour
{
    [Tooltip("A TMP_text to animate. Will be set to this object's text if left empty.")]
    [SerializeField]
    TMP_Text m_text;
    [Tooltip("Duration of the animation")]
    [SerializeField]
    float m_duration = 0.3f;
    [Tooltip("Movement of the text in animation")]
    [SerializeField]
    Vector3 m_movement = Vector3.zero;

    /// <summary>
    /// A TMP_text to animate. Will be set to this object's text if left empty.
    /// </summary>
    public TMP_Text Text {
        get => m_text;
        set => m_text = value;
    }
    /// <summary>
    /// Duration of the animation
    /// </summary>
    public float Duration {
        get => m_duration;
        set => m_duration = value;
    }
    /// <summary>
    /// Movement of the text in animation
    /// </summary>
    public Vector3 Movement {
        get => m_movement;
        set => m_movement = value;
    }

    public delegate void LocalizerUpdate();

    Coroutine m_currentCoroutine = null;
    float m_alpha;

    void Awake() {
        if (m_text == null) {
            m_text = GetComponent<TMP_Text>();
        }
    }

    /// <summary>
    /// Starts fade in for the text
    /// </summary>
    public void StartFadeIn() {
        SetAlpha(0);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeIn(m_duration));
    }

    /// <summary>
    /// Starts fade out for the text
    /// </summary>
    public void StartFadeOut() {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_duration));
    }

    /// <summary>
    /// Starts fade out for the text and changes it to the new string when it's finished
    /// </summary>
    /// <param name="newText">New string</param>
    public void StartFadeOut(string newText) {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_duration, newText));
    }

    /// <summary>
    /// Fades the text out, changes its string and fades it back in
    /// </summary>
    /// <param name="newText">New string</param>
    /// <param name="delay">Delay between the fade out and fade in</param>
    public void RunFadeSequence(string newText, float delay = 0) {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeSequence(m_duration, delay, newText));
    }

    /// <summary>
    /// Fades the text out, changes its string, invokes LocalizerUpdate and fades it back in
    /// </summary>
    /// <param name="newText">New string</param>
    /// <param name="textUpdateCallback">Delegate that will be invoked after text change. Usually used to update the localizer</param>
    /// <param name="delay">Delay between the fade out and fade in</param>
    public void RunFadeSequence(string newText, LocalizerUpdate textUpdateCallback, float delay = 0) {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeSequence(m_duration, delay, newText, textUpdateCallback));
    }

    /// <summary>
    /// A coroutine method for the fade in
    /// </summary>
    /// <param name="duration">Duration of the fade in</param>
    IEnumerator FadeIn(float duration) {
        float elapsedTime = 0.0f;
        while (m_alpha <= 1.0f) {
            SetAlpha(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// A coroutine method for the fade out
    /// </summary>
    /// <param name="duration">Duration of the fade out</param>
    IEnumerator FadeOut(float duration) {
        float elapsedTime = 0.0f;
        while (m_alpha >= 0) {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// A coroutine method for the fade out and changing the string after its finished
    /// </summary>
    /// <param name="duration">Duration of the fade out</param>
    /// <param name="text">New string</param>
    IEnumerator FadeOut(float duration, string text) {
        float elapsedTime = 0.0f;
        while (m_alpha >= 0) {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        m_text.text = text;
    }

    /// <summary>
    /// A coroutine method that fades the text out, changes its string and fades it back in while playing the movement animation
    /// </summary>
    /// <param name="duration">Duration of fading. Will be applied for both fading in and fading out</param>
    /// <param name="delay">Delay between the fade out and fade in</param>
    /// <param name="text">New string</param>
    /// <param name="textUpdateCallback">Delegate that will be invoked after text change. Usually used to update the localizer</param>
    IEnumerator FadeSequence(float duration, float delay, string text, LocalizerUpdate textUpdateCallback = null) {
        float elapsedTime = 0.0f;
        while (m_alpha >= 0) {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_text.text = text;
        textUpdateCallback?.Invoke();

        Vector3 targetPos = m_text.rectTransform.localPosition;
        m_text.rectTransform.localPosition = targetPos - m_movement;
        Vector3 velocity = Vector3.zero;
        yield return new WaitForSeconds(delay);
        elapsedTime = 0.0f;
        while (m_alpha <= 1.0f) {
            m_text.rectTransform.localPosition = Vector3.SmoothDamp(m_text.rectTransform.localPosition, targetPos, ref velocity, duration, float.PositiveInfinity, Time.deltaTime);
            SetAlpha(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Sets the alpha of the text to a new value
    /// </summary>
    /// <param name="value">New alpha</param>
    void SetAlpha(float value) {
        m_alpha = value;
        m_text.alpha = m_alpha;
    }
}
