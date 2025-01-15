using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// A behaviour that controls displaying info data in the connected UI blocks
/// </summary>
public class InfoUI : MonoBehaviour
{
    [Tooltip("Info UI data that will be displayed")]
    public InfoData Data;

    /// <summary>
    /// Current string buffer
    /// </summary>
    [System.NonSerialized]
    public List<InfoData.InfoText> Strings;
    /// <summary>
    /// Current sprite buffer
    /// </summary>
    [System.NonSerialized]
    public List<Sprite> Sprites;
    /// <summary>
    /// Current action buffer
    /// </summary>
    [System.NonSerialized]
    public List<UnityAction> Actions;
    int m_currentString = 0;
    int m_currentSprite = 0;
    int m_currentAction = 0;

    [Space]
    [Tooltip("TMP Text components that will display the text")]
    public List<TMP_Text> TextBlocks;
    [Tooltip("Image components that will display images")]
    public List<Image> ImageBlocks;
    int m_currentTextBlock = 0;
    int m_currentImageBlock = 0;

    [Tooltip("Default font sizes of TMP Image components. \nWill import font sizes of text blocks on startup if empty or a different length to the text blocks list")]
    public List<float> DefaultFontSizes;

    [Space]
    [Tooltip("Delay between displaying concurrent blocks")]
    [SerializeField]
    float m_displayDelay = 0.2f;
    /// <summary>
    /// Delay between displaying concurrent blocks
    /// </summary>
    public float DisplayDelay {
        get => m_displayDelay;
        set => m_displayDelay = value;
    }

    AnimatedText[] m_textAnimations;
    UnityEngine.Localization.Components.LocalizeStringEvent[] m_stringLocalizers;
    AnimatedImage[] m_imageAnimations;

    bool m_clearedData = false;

    [Tooltip("Automatic advancing to the next data set")]
    [SerializeField]
    bool m_auto = false;
    [Tooltip("Delay before auto advancing")]
    [SerializeField]
    float m_timeToNext = 5f;

    /// <summary>
    /// Automatic advancing to the next data set
    /// </summary>
    public bool Auto {
        get => m_auto;
        set {
            m_auto = value;
            if (m_auto) {
                m_timeLastRan = Time.time;
            }
        }
    }
    /// <summary>
    /// Delay before auto advancing
    /// </summary>
    public float TimeToNext {
        get => m_timeToNext;
        set => m_timeToNext = value;
    }

    float m_timeLastRan = 0;

    void Awake() {
        if (Strings == null) {
            Strings = new List<InfoData.InfoText>();
        }
        if (DefaultFontSizes == null) {
            DefaultFontSizes = new List<float>();
        }
        if (Sprites == null) {
            Sprites = new List<Sprite>();
        }
        if (Actions == null) {
            Actions = new List<UnityAction>();
        }
        if (TextBlocks == null) {
            TextBlocks = new List<TMP_Text>();
        }
        if (ImageBlocks == null) {
            ImageBlocks = new List<Image>();
        }
    }

    void Start() {
        // Getting all of the nessessary component references and saving initial font sizes
        GetTextAnimations();
        ValidateFontSizes();
        GetLocalizers();
        GetImageAnimations();
    }

    void Update() {
        // Automatic advancing of the info block sets
        if (m_auto) {
            if (Time.time >= m_timeLastRan + m_timeToNext) {
                DisplayNext();
                m_timeLastRan = Time.time;
            }
        }
    }

    /// <summary>
    /// Displays the next set of info blocks
    /// </summary>
    public void DisplayNext() {
        // Filling the buffer until we reach the data block that doesn't have PlayNext set to true
        while (BufferNext());

        DisplayBuffer();
    }

    /// <summary>
    /// Adds the next blocks in info data to the display buffer
    /// </summary>
    /// <returns>Is PlayNext of the buffered block set to true?</returns>
    bool BufferNext() {
        InfoData.InfoBlock current = Data.Next();
        if (current == null)
            return false;

        if (current is InfoData.InfoText) {
            Strings.Add((InfoData.InfoText)current);
        } else if (current is InfoData.InfoSprite) {
            Sprites.Add(((InfoData.InfoSprite)current).Sprite);
        } else if (current is InfoData.InfoAction) {
            Actions.Add(((InfoData.InfoAction)current).Action);
        } else if (current is InfoData.InfoClear) {
            InfoData.InfoClear clear = (InfoData.InfoClear)current;
            // Clear blocks are not displaying anything so they are executed immediately
            if (clear.Flags.HasFlag(InfoData.InfoClear.ClearFlags.text)) {
                ClearText();
            }
            if (clear.Flags.HasFlag(InfoData.InfoClear.ClearFlags.images)) {
                ClearImages();
            }
            if (clear.Flags.HasFlag(InfoData.InfoClear.ClearFlags.actions)) {
                ClearActions();
            }
        }
        return current.PlayNext;
    }

    /// <summary>
    /// Displays buffered texts and images and runs buffered actions
    /// </summary>
    void DisplayBuffer() {
        int index = 0;
        while (DisplayNextString(ref index));
        index = 0;
        while (DisplayNextImage(ref index));
        while (RunNextAction());

        ClearUsedBuffers();
    }

    /// <summary>
    /// Gets references to AnimatedText components in the connected TMP_Text gameobjects
    /// </summary>
    public void GetTextAnimations() {
        m_textAnimations = new AnimatedText[TextBlocks.Count];
        for (int i = 0; i < TextBlocks.Count; i++) {
            m_textAnimations[i] = TextBlocks[i].GetComponent<AnimatedText>();
        }
    }

    /// <summary>
    /// Saves initial font sizes of the connected text blocks
    /// </summary>
    public void ValidateFontSizes() {
        if (DefaultFontSizes.Count != TextBlocks.Count) {
            foreach (var item in TextBlocks) {
                DefaultFontSizes.Add(item.fontSize);
            }
        }
    }

    /// <summary>
    /// Gets references to LocalizeStringEvent components in the connected TMP_Text gameobjects
    /// </summary>
    public void GetLocalizers() {
        m_stringLocalizers = new UnityEngine.Localization.Components.LocalizeStringEvent[TextBlocks.Count];
        for (int i = 0; i < TextBlocks.Count; i++) {
            m_stringLocalizers[i] = TextBlocks[i].GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
        }
    }

    /// <summary>
    /// Gets references to AnimatedImage components in the connected TMP_Text gameobjects
    /// </summary>
    public void GetImageAnimations() {
        m_imageAnimations = new AnimatedImage[ImageBlocks.Count];
        for (int i = 0; i < ImageBlocks.Count; i++) {
            m_imageAnimations[i] = ImageBlocks[i].GetComponent<AnimatedImage>();
        }
    }

    /// <summary>
    /// Starts the text fade sequence for the provided string in the provided text block
    /// </summary>
    /// <param name="blockIndex">Index of the text block where the sequence will be ran</param>
    /// <param name="stringIndex">Index of the string that will be displayed</param>
    /// <param name="delay">Delay between fade out and fade in</param>
    void StartTextFadeSequence(int blockIndex, int stringIndex, float delay) {
        if (m_textAnimations[blockIndex] != null) {
            string text = Strings[stringIndex].Text;
            string key = Strings[stringIndex].LocaleKey;
            float fontSize = Strings[stringIndex].FontSize;
            m_textAnimations[blockIndex].RunFadeSequence(text, () => UpdateTextBlock(blockIndex, key, fontSize), delay);
        }
    }

    /// <summary>
    /// Starts the text fade out sequence in the provided text block
    /// </summary>
    /// <param name="blockIndex">Index of the text block where the fade out will be ran</param>
    /// <returns>Is there an AnimatedText component with the provided index?</returns>
    bool StartTextFadeOut(int blockIndex) {
        if (m_textAnimations[blockIndex] != null) {
            m_textAnimations[blockIndex].StartFadeOut("");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts the text fade sequence for the provided sprite in the provided image block
    /// </summary>
    /// <param name="blockIndex">Index of the image block where the sequence will be ran</param>
    /// <param name="sprite">Sprite that will be displayed</param>
    /// <param name="delay">Delay between fade out and fade in</param>
    void StartImageFadeSequence(int blockIndex, Sprite sprite, float delay) {
        if (m_imageAnimations[blockIndex] != null) {
            m_imageAnimations[blockIndex].RunFadeSequence(sprite, delay);
        }
    }

    /// <summary>
    /// Starts the image fade out sequence in the provided image block
    /// </summary>
    /// <param name="blockIndex">Index of the image block where the fade out will be ran</param>
    /// <returns>Is there an AnimatedImage component with the provided index?</returns>
    bool StartImageFadeOut(int blockIndex) {
        if (m_imageAnimations[blockIndex] != null) {
            m_imageAnimations[blockIndex].StartFadeOut(null);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Displays the next buffered text in the next text block
    /// </summary>
    /// <param name="index">Reference to the index of the text in the current block set. Used to space the delays of the concurrently displayed texts</param>
    /// <returns>Are there any buffered texts left?</returns>
    bool DisplayNextString(ref int index) {
        if (m_currentString >= Strings.Count)
            return false;

        DisplayNextStringUnchecked(index * m_displayDelay);
        index++;
        return true;
    }

    /// <summary>
    /// Updades position and size of the next text block and displays the next buffered text there
    /// </summary>
    /// <param name="pos">New position</param>
    /// <param name="sizeDelta">New relative size</param>
    /// <returns>Are there any buffered texts left?</returns>
    bool DisplayNextString(Vector2 pos, Vector2 sizeDelta) {
        if (m_currentString >= Strings.Count)
            return false;

        SetBlockPosition(TextBlocks[m_currentTextBlock].rectTransform, pos, sizeDelta);
        DisplayNextStringUnchecked(0);
        return true;
    }

    /// <summary>
    /// Displays the next buffered text in the next text block. Will throw an error if there is no texts left in the buffer
    /// </summary>
    /// <param name="delay">Delay of the text fade sequence</param>
    void DisplayNextStringUnchecked(float delay) {
        PrepCurrentTextBlock();
        StartTextFadeSequence(m_currentTextBlock, m_currentString, delay);
        m_currentString++;
        m_currentTextBlock++;
    }

    /// <summary>
    /// Displays the next buffered image in the next image block
    /// </summary>
    /// <param name="index">Reference to the index of the image in the current block set. Used to space the delays of the concurrently displayed images</param>
    /// <returns>Are there any buffered images left?</returns>
    bool DisplayNextImage(ref int index) {
        if (m_currentSprite >= Sprites.Count)
            return false;

        DisplayNextImageUnchecked(index * m_displayDelay);
        index++;
        return true;
    }

    /// <summary>
    /// Updades position and size of the next image block and displays the next buffered image there
    /// </summary>
    /// <param name="pos">New position</param>
    /// <param name="sizeDelta">New relative size</param>
    /// <returns>Are there any buffered images left?</returns>
    bool DisplayNextImage(Vector2 pos, Vector2 sizeDelta) {
        if (m_currentSprite >= Sprites.Count)
            return false;
        
        SetBlockPosition(ImageBlocks[m_currentImageBlock].rectTransform, pos, sizeDelta);
        DisplayNextImageUnchecked(0);
        return true;
    }

    /// <summary>
    /// Displays the next buffered image in the next image block. Will throw an error if there is no images left in the buffer
    /// </summary>
    /// <param name="delay">Delay of the image fade sequence</param>
    void DisplayNextImageUnchecked(float delay) {
        PrepCurrentImageBlock();
        StartImageFadeSequence(m_currentImageBlock, Sprites[m_currentSprite], delay);
        m_currentSprite++;
        m_currentImageBlock++;
    }

    /// <summary>
    /// Invokes the next buffered action
    /// </summary>
    /// <returns>Are there any buffered actions left?</returns>
    bool RunNextAction() {
        if (m_currentAction >= Actions.Count)
            return false;

        if (Actions[m_currentAction] != null) {
            Actions[m_currentAction].Invoke();
        }

        // A check for the situation when the invoked action was to clear this object's action data.
        if (m_clearedData) {
            m_clearedData = false;
            return false;
        }

        m_currentAction++;
        return true;
    }

    /// <summary>
    /// Fades out all displayed blocks and sets current block indices to 0
    /// </summary>
    public void Clear() {
        ClearText();
        ClearImages();
        ClearActions();
    }

    /// <summary>
    /// Fades out all of the text blocks and sets current text block index to 0
    /// </summary>
    public void ClearText() {
        for (int i = 0; i < TextBlocks.Count; i++) {
            m_stringLocalizers[i].StringReference = null;
            if (!StartTextFadeOut(i))
                TextBlocks[i].text = "";
        }
        m_currentTextBlock = 0;
    }

    /// <summary>
    /// Fades out all of the image blocks and sets current image block index to 0
    /// </summary>
    public void ClearImages() {
        for (int i = 0; i < ImageBlocks.Count; i++) {
            if (!StartImageFadeOut(i)) {
                ImageBlocks[i].sprite = null;
            }
        }
        m_currentImageBlock = 0;
    }

    /// <summary>
    /// Placeholder, currently does nothing
    /// </summary>
    public void ClearActions() {
        
    }

    /// <summary>
    /// Removes all data from the buffer
    /// </summary>
    public void ClearData() {
        Strings.Clear();
        Sprites.Clear();
        Actions.Clear();
        m_currentString = 0;
        m_currentSprite = 0;
        m_currentAction = 0;

        m_clearedData = true;
    }

    /// <summary>
    /// Removes already used data from the buffer
    /// </summary>
    void ClearUsedBuffers() {
        while (m_currentString > 0) {
            Strings.RemoveAt(0);
            m_currentString--;
        }
        while (m_currentSprite > 0) {
            Sprites.RemoveAt(0);
            m_currentSprite--;
        }
        while (m_currentAction > 0) {
            Actions.RemoveAt(0);
            m_currentAction--;
        }
    }

    /// <summary>
    /// Sets the block position and relative size for the provided transform
    /// </summary>
    /// <param name="transform">Transform to update</param>
    /// <param name="pos">New position</param>
    /// <param name="sizeDelta">New relative size</param>
    void SetBlockPosition(RectTransform transform, Vector2 pos, Vector2 sizeDelta) {
        transform.localPosition = pos;
        transform.sizeDelta = sizeDelta;
    }

    /// <summary>
    /// Clears text blocks if all of them are used
    /// </summary>
    void PrepCurrentTextBlock() {
        if (m_currentTextBlock > TextBlocks.Count - 1) {
            ClearText();
        }
    }

    /// <summary>
    /// Sets the localization key and font size of the text block with the privided index to the new values 
    /// </summary>
    /// <param name="block">Index of the text block to update</param>
    /// <param name="locKey">New localization key</param>
    /// <param name="fontSize">New font size</param>
    void UpdateTextBlock(int block, string locKey, float fontSize) {
        if (locKey == "") {
            m_stringLocalizers[block].StringReference = null;
        } else {
            m_stringLocalizers[block].StringReference = new UnityEngine.Localization.LocalizedString(Data.LocTable, locKey);
        }

        if (fontSize == -1) {
            TextBlocks[block].fontSize = DefaultFontSizes[block];
        } else {
            TextBlocks[block].fontSize = fontSize;
        }
    }

    /// <summary>
    /// Clears image blocks if all of them are used
    /// </summary>
    void PrepCurrentImageBlock() {
        if (m_currentImageBlock > ImageBlocks.Count - 1) {
            ClearImages();
        }
    }
}
