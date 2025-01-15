using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A behaviour that privides methods for fading the image in and out and smoothly changing its sprite
/// </summary>
public class AnimatedImage : MonoBehaviour
{
    [Tooltip("An image to animate. Will be set to this object's Image if left empty")]
    [SerializeField]
    Image m_image;
    [Tooltip("Duration of the fade")]
    [SerializeField]
    float m_duration = 0.3f;

    /// <summary>
    /// An image to animate. Will be set to this object's Image if left empty
    /// </summary>
    public Image UIImage {
        get => m_image;
        set => m_image = value;
    }
    /// <summary>
    /// Duration of the fade
    /// </summary>
    public float Duration {
        get => m_duration;
        set => m_duration = value;
    }

    Coroutine m_currentCoroutine = null;
    float m_alpha;

    void Awake() {
        if (m_image == null) {
            m_image = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Starts fade in for the image
    /// </summary>
    public void StartFadeIn() {
        SetAlpha(0);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeIn(m_duration));
    }

    /// <summary>
    /// Starts fade out for the image
    /// </summary>
    public void StartFadeOut() {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_duration));
    }

    /// <summary>
    /// Starts fade out for the image and changes its sprite to the new one when it's finished
    /// </summary>
    /// <param name="newSprite">New sprite</param>
    public void StartFadeOut(Sprite newSprite) {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeOut(m_duration, newSprite));
    }

    /// <summary>
    /// Fades the image out, changes its sprite and fades it back in
    /// </summary>
    /// <param name="newSprite">New sprite</param>
    /// <param name="delay">Delay between the fade out and fade in</param>
    public void RunFadeSequence(Sprite newSprite, float delay = 0) {
        SetAlpha(1);
        StopAllCoroutines();
        m_currentCoroutine = StartCoroutine(FadeSequence(m_duration, newSprite, delay));
    }

    /// <summary>
    /// A coroutine method for the fade in
    /// </summary>
    /// <param name="duration">Duration of the fade in</param>
    IEnumerator FadeIn(float duration) {
        if (m_image.sprite == null) {
            SetAlpha(0);
            yield break;
        }

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
        if (m_image.sprite == null) {
            SetAlpha(0);
            yield break;
        }

        float elapsedTime = 0.0f;
        while (m_alpha >= 0) {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// A coroutine method for the fade out and changing the sprite after its finished 
    /// </summary>
    /// <param name="duration">Duration of the fade out</param>
    /// <param name="sprite">New sprite</param>
    IEnumerator FadeOut(float duration, Sprite sprite) {
        if (m_image.sprite == null) {
            SetAlpha(0);
            m_image.sprite = sprite;
            yield break;
        }

        float elapsedTime = 0.0f;
        while (m_alpha >= 0) {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        m_image.sprite = sprite;
    }

    /// <summary>
    /// A coroutine method that fades the image out, changes its sprite and fades it back in
    /// </summary>
    /// <param name="duration">Duration of fading. Will be applied for both fading in and fading out</param>
    /// <param name="sprite">New sprite</param>
    /// <param name="delay">Delay between the fade out and fade in</param>
    IEnumerator FadeSequence(float duration, Sprite sprite, float delay) {
        float elapsedTime = 0.0f;
        if (m_image.sprite == null) {
            SetAlpha(0);
            yield return new WaitForSeconds(duration);
        } else {
            while (m_alpha >= 0) {
                SetAlpha(1 - (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        m_image.sprite = sprite;

        yield return new WaitForSeconds(delay);
        if (m_image.sprite != null) {
            elapsedTime = 0.0f;
            while (m_alpha <= 1.0f) {
                SetAlpha(elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Sets the alpha of the image
    /// </summary>
    /// <param name="value">New alpha of the image</param>
    void SetAlpha(float value) {
        m_alpha = value;
        Color c = new Color(m_image.color.r, m_image.color.g, m_image.color.b, m_alpha);
        m_image.color = c;
    }
}
