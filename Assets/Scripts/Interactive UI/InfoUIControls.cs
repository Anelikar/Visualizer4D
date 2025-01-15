using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The behaviour that provides logic to control the InfoUI using ButtonInteractables
/// </summary>
public class InfoUIControls : MonoBehaviour
{
    [Tooltip("Info UI controlled by this behaviour")]
    [SerializeField]
    InfoUI m_infoUI;
    [Tooltip("Controls that will be activated after the intro")]
    [SerializeField]
    List<GameObject> m_nextControls;

    [Tooltip("A delay before swapping controls on skip")]
    [SerializeField]
    float m_skipIntoDelay = 0.6f;

    [Header("Controls")]
    [Tooltip("A button to proceed to the next message")]
    [SerializeField]
    ButtonInteractable m_buttonNext;
    [Tooltip("A button to skip the intro")]
    [SerializeField]
    ButtonInteractable m_buttonSkip;
    [Tooltip("A button to toggle automatic message advancing")]
    [SerializeField]
    ButtonInteractable m_buttonAuto;
    ToggleMaterial m_buttonAutoToggleMat;

    void Start() {
        m_buttonAutoToggleMat = m_buttonAuto.GetComponent<ToggleMaterial>();
    }

    void OnEnable() {
        m_buttonNext.OnPress.AddListener(m_infoUI.DisplayNext);
        m_buttonSkip.OnPress.AddListener(SkipIntro);

        // Synchronyzing ToggleMaterial and Auto mode states 
        // This assumes there is only one button present at any given time to change this setting
        if (m_buttonAutoToggleMat == null) {
            m_buttonAutoToggleMat = m_buttonAuto.GetComponent<ToggleMaterial>();
        }
        if (m_buttonAutoToggleMat != null) {
            if (m_infoUI.Auto) {
                if (!m_buttonAutoToggleMat.Toggled) {
                    m_buttonAutoToggleMat.Toggle();
                }
            } else {
                if (m_buttonAutoToggleMat.Toggled) {
                    m_buttonAutoToggleMat.Toggle();
                }
            }
        }
    }

    void OnDisable() {
        m_buttonNext.OnPress.RemoveListener(m_infoUI.DisplayNext);
        m_buttonSkip.OnPress.RemoveListener(SkipIntro);
    }
    
    /// <summary>
    /// Skips all messages and invokes all actions in the current data block
    /// </summary>
    public void SkipIntro() {
        UnityEngine.Events.UnityAction[] actions =  m_infoUI.Data.GetRemaingActions();
        foreach (var item in actions) {
            if (item != null) {
                item.Invoke();
            }
        }
        m_infoUI.Clear();
        StartSwapUI();
    }

    /// <summary>
    /// Starts SwapUI coroutine with a delay of skipIntroDelay
    /// </summary>
    public void StartSwapUI() {
        StopAllCoroutines();
        StartCoroutine(SwapUI(m_skipIntoDelay));
    }

    /// <summary>
    /// After a delay, activates all gameobjects in the nextContols list and deactivates this gameobject
    /// </summary>
    /// <param name="delay">A delay before a swap</param>
    /// <returns></returns>
    IEnumerator SwapUI(float delay) {
        yield return new WaitForSeconds(delay);
        foreach (var item in m_nextControls) {
            item.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
