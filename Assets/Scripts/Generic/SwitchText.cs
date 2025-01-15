using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;

/// <summary>
/// A behaviour that provides a method to toggle TMP text component between two strings or StringReferences
/// </summary>
public class SwitchText : MonoBehaviour
{
    [Tooltip("Text UI element to change")]
    [SerializeField]
    TMP_Text m_text;
    [SerializeField]
    [Tooltip("Initial string")]
    string m_string1;
    [Tooltip("New string")]
    [SerializeField]
    string m_string2;

    [Tooltip("Initial string reference")]
    [SerializeField]
    LocalizedString m_StringReference1 = null;
    [Tooltip("New string reference")]
    [SerializeField]
    LocalizedString m_StringReference2 = null;

    bool m_string1Active = true;

    /// <summary>
    /// Initial string reference
    /// </summary>
    public LocalizedString StringReference1 {
        get => m_StringReference1;
        set {
            // Unsubscribing from old string reference
            m_StringReference1.StringChanged -= UpdateString1;

            m_StringReference1 = value;

            // Subscribing to a new string reference
            if (isActiveAndEnabled && m_StringReference1 != null)
                m_StringReference1.StringChanged += UpdateString1;
        }
    }

    /// <summary>
    /// New string reference
    /// </summary>
    public LocalizedString StringReference2 {
        get => m_StringReference2;
        set {
            // Unsubscribing from old string reference
            m_StringReference2.StringChanged -= UpdateString2;

            m_StringReference2 = value;

            // Subscribing to a new string reference
            if (isActiveAndEnabled && m_StringReference2!= null)
                m_StringReference2.StringChanged += UpdateString2;
        }
    }

    void OnEnable() {
        if (m_StringReference1 != null) {
            m_StringReference1.StringChanged += UpdateString1;
        }
        if (m_StringReference2 != null) {
            m_StringReference2.StringChanged += UpdateString2;
        }
    }

    void OnDisable() {
        m_StringReference1.StringChanged -= UpdateString1;
        m_StringReference2.StringChanged -= UpdateString2;
    }

    void OnValidate() {
        // Displaying text from StringReference in editor
        m_StringReference1?.RefreshString();
        m_StringReference2?.RefreshString();
    }

    /// <summary>
    /// Set String1 to newString and updates UI text component if necessary
    /// </summary>
    /// <param name="newString">New value for String1</param>
    void UpdateString1(string newString) {
        m_string1 = newString;
        if (m_string1Active) {
            m_text.text = m_string1;
        }
    }

    /// <summary>
    /// Set String2 to newString and updates UI text component if necessary
    /// </summary>
    /// <param name="newString">New value for String2</param>
    void UpdateString2(string newString) {
        m_string2 = newString;
        if (!m_string1Active) {
            m_text.text = m_string2;
        }
    }

    /// <summary>
    /// Toggles between string1 and string2
    /// </summary>
    public void Switch() {
        if (m_string1Active) {
            m_text.text = m_string2;
            m_string1Active = false;
        } else {
            m_text.text = m_string1;
            m_string1Active = true;
        }
    }
}
