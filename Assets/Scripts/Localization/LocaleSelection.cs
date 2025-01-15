using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// A behavoiur that provides a way to change current locale
/// </summary>
public class LocaleSelection : MonoBehaviour
{
    int m_selectedLocale = 0;

    IEnumerator Start() {
        // Waiting for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        // Initializing selectedLocale variable
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i) {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (LocalizationSettings.SelectedLocale == locale)
                m_selectedLocale = i;
        }
    }

    /// <summary>
    /// Toggles current locale between locale 0 and locale 1
    /// </summary>
    public void SwapLocale() {
        if (m_selectedLocale == 0) {
            m_selectedLocale = 1;
        } else {
            m_selectedLocale = 0;
        }
        SelectLocale(m_selectedLocale);
    }

    /// <summary>
    /// Changes locale to the one with a specified index
    /// </summary>
    /// <param name="index">Index of a new locale</param>
    static void SelectLocale(int index) {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
