using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

/// <summary>
/// A behaviour that provides methods for constructing and retrieving info UI data
/// </summary>
public class InfoData : MonoBehaviour
{
    /// <summary>
    /// Base class for info UI setup
    /// </summary>
    public abstract class InfoBlock 
    {
        /// <summary>
        /// Should the next block be read after this?
        /// </summary>
        public bool PlayNext = false;
    }

    /// <summary>
    /// Text block display setup for the info UI
    /// </summary>
    public class InfoText : InfoBlock
    {
        /// <summary>
        /// Text to display
        /// </summary>
        public string Text;
        /// <summary>
        /// Font size of the displayed text
        /// </summary>
        public float FontSize = -1;

        /// <summary>
        /// Locale table of the displayed text
        /// </summary>
        public readonly string LocaleTable = "";
        /// <summary>
        /// Locale key of the displayed text
        /// </summary>
        public readonly string LocaleKey = "";
        UnityEngine.Localization.LocalizedString m_localizedString;

        /// <param name="text">Text to display</param>
        /// <param name="playNext">Should the next block be read after this?</param>
        /// <param name="fontSize">Font size of the displayed text</param>
        public InfoText(string text, bool playNext = false, float fontSize = -1) {
            Text = text;
            FontSize = fontSize;
            PlayNext = playNext;
        }

        /// <param name="text">Text to display</param>
        /// <param name="localeTable">Locale table of the displayed text</param>
        /// <param name="localeKey">Locale key of the displayed text</param>
        /// <param name="playNext">Should the next block be read after this?</param>
        /// <param name="fontSize">Font size of the displayed text</param>
        public InfoText(string text, string localeTable, string localeKey, bool playNext = false, float fontSize = -1) {
            Text = text;
            FontSize = fontSize;
            PlayNext = playNext;

            LocaleTable = localeTable;
            LocaleKey = localeKey;
            InitLocalization();
        }

        /// <summary>
        /// Disconnects this text block from the localization system
        /// </summary>
        public void StopLocalization() {
            if (m_localizedString != null) {
                m_localizedString.StringChanged -= UpdateText;
                m_localizedString = null;
            }
        }

        /// <summary>
        /// Connects this text block to the localization system
        /// </summary>
        void InitLocalization() {
            if (LocaleKey != "") {
                m_localizedString = new UnityEngine.Localization.LocalizedString(LocaleTable, LocaleKey);
                m_localizedString.StringChanged += UpdateText;
            }
        }

        /// <summary>
        /// Changes text to display to a new value
        /// </summary>
        /// <param name="text">New text</param>
        void UpdateText(string text) {
            Text = text;
        }
    }

    /// <summary>
    /// Image display setup for the info UI
    /// </summary>
    public class InfoSprite : InfoBlock
    {
        /// <summary>
        /// Sprite to display
        /// </summary>
        public Sprite Sprite;

        /// <param name="sprite">Sprite to display</param>
        /// <param name="playNext">Should the next block be read after this?</param>
        public InfoSprite(Sprite sprite, bool playNext = false) {
            Sprite = sprite;
            PlayNext = playNext;
        }
    }

    /// <summary>
    /// Block for settting up the clearing of the displayed data
    /// </summary>
    public class InfoClear : InfoBlock
    {
        [System.Flags]
        public enum ClearFlags
        {
            none = 0,
            text = 1 << 0,
            images = 1 << 1,
            actions = 1 << 2,
            all = text | images | actions
        }
        /// <summary>
        /// Flags for clearing displayed data
        /// </summary>
        public ClearFlags Flags;

        /// <param name="flags">Flags for clearing displayed data</param>
        /// <param name="playNext">Should the next block be read after this?</param>
        public InfoClear(ClearFlags flags, bool playNext = false) {
            Flags = flags;
            PlayNext = playNext;
        }

        /// <summary>
        /// Sets the clear flags to "all"
        /// </summary>
        /// <param name="playNext">Should the next block be read after this?</param>
        public InfoClear(bool playNext = false) {
            Flags = ClearFlags.all;
            PlayNext = playNext;
        }
    }

    /// <summary>
    /// Blocks for setting up an action to run while displaying data
    /// </summary>
    public class InfoAction : InfoBlock
    {
        /// <summary>
        /// Action to run
        /// </summary>
        public UnityAction Action;

        /// <param name="action">Action to run</param>
        /// <param name="playNext">Should the next block be read after this?</param>
        public InfoAction(UnityAction action, bool playNext = false) {
            Action = action;
            PlayNext = playNext;
        }
    }

    /// <summary>
    /// List of the data blocks for the info UI
    /// </summary>
    public List<InfoBlock> InfoBlocks;
    int m_currentBlock = 0;

    [Tooltip("Localization table for the displayed localized text")]
    [SerializeField]
    string m_locTable = "Info String Table";
    /// <summary>
    /// Localization table for the displayed localized text
    /// </summary>
    public string LocTable {
        get => m_locTable;
        set => m_locTable = value;
    }

    void Awake() {
        if (InfoBlocks == null) {
            InfoBlocks = new List<InfoBlock>();
        }
    }

    /// <summary>
    /// Returns the next block from the data list
    /// </summary>
    /// <returns>The next block from the data list</returns>
    public InfoBlock Next() {
        if (m_currentBlock < InfoBlocks.Count) {
            if (InfoBlocks[m_currentBlock] is InfoText) {
                ((InfoText)InfoBlocks[m_currentBlock]).StopLocalization();
            }
            return InfoBlocks[m_currentBlock++];
        }
        return null;
    }

    // Building data blocks using scripts is not very elegant, but writing a custom editor for them seems like an overkill.
    // So see IntroUIDataBuilder for a template for using these methods.

    /// <summary>
    /// Adds the block for displaying text with default font size to the data list
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddText(string text, bool addNext = false) {
        InfoBlocks.Add(new InfoText(text, addNext));
    }

    /// <summary>
    /// Adds the block for displaying localized text with default font size to the data list
    /// </summary>
    /// <param name="text">Locale key of the displayed text</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddTextLoc(string text, bool addNext = false) {
        InfoBlocks.Add(new InfoText(LocalizationSettings.StringDatabase.GetLocalizedString(m_locTable, text), m_locTable, text, addNext));
    }

    /// <summary>
    /// Adds the block for displaying localized text to the data list
    /// </summary>
    /// <param name="text">Locale key of the displayed text</param>
    /// <param name="fontSize">Font size of the diaplyed text</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddTextLoc(string text, float fontSize, bool addNext = false) {
        InfoBlocks.Add(new InfoText(LocalizationSettings.StringDatabase.GetLocalizedString(m_locTable, text), m_locTable, text, addNext, fontSize));
    }

    /// <summary>
    /// Adds the block for displaying a sprite to the data list
    /// </summary>
    /// <param name="sprite">Sprite to display</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddSprite(Sprite sprite, bool addNext = false) {
        InfoBlocks.Add(new InfoSprite(sprite, addNext));
    }

    /// <summary>
    /// Adds the block for running an action to the data list
    /// </summary>
    /// <param name="action">Action to run</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddAction(UnityAction action, bool addNext = false) {
        InfoBlocks.Add(new InfoAction(action, addNext));
    }

    /// <summary>
    /// Adds the block for clearing data with clear flags set to "all" to the data list
    /// </summary>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddClear(bool addNext = false) {
        InfoBlocks.Add(new InfoClear(addNext));
    }

    /// <summary>
    /// Adds the block for clearing data to the data list
    /// </summary>
    /// <param name="flags">Displayed data to clear</param>
    /// <param name="addNext">Should the next block be read after this?</param>
    public void AddClear(InfoClear.ClearFlags flags, bool addNext = false) {
        InfoBlocks.Add(new InfoClear(flags, addNext));
    }

    /// <summary>
    /// Finds all of the univoked action in the data list and returns them as an array
    /// </summary>
    /// <returns>An array of uninvoked actions</returns>
    public UnityAction[] GetRemaingActions() {
        List<UnityAction> actions = new List<UnityAction>();

        InfoBlock block = Next();
        while (block != null) {
            if (block is InfoAction) {
                actions.Add(((InfoAction)block).Action);
            }
            block = Next();
        }
        return actions.ToArray();
    }
}
