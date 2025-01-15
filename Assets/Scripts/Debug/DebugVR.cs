using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton that provides quick accesss to a couple gameobjects that can be used in debugging and an interface to print console output and custom text to a TMP text conponent
/// </summary>
public class DebugVR : MonoBehaviour
{
    static DebugVR m_instance;
    /// <summary>
    /// Instance of this singleton
    /// </summary>
    public static DebugVR Instance => m_instance;

    [Tooltip("These will display lines from the console log")]
    [SerializeField]
    TMPro.TMP_Text[] m_textsConsole;
    string m_textConsole;
    [Tooltip("These will display custom text set to TextCustom")]
    [SerializeField]
    TMPro.TMP_Text[] m_textsCustom;
    string m_textCustom;

    [Tooltip("Maximum lines to display from the console log")]
    [SerializeField]
    int m_maxLines = 8;
    public int MaxLines {
        get => m_maxLines;
        set => m_maxLines = value;
    }

    Queue<string> m_consoleQueue = new Queue<string>();

    Transform m_sphere1;
    /// <summary>
    /// An object that can be used for visualising points while debuging
    /// </summary>
    public Transform Sphere1 => m_sphere1;
    Transform m_sphere2;
    /// <summary>
    /// An object that can be used for visualising points while debuging
    /// </summary>
    public Transform Sphere2 => m_sphere2;
    Transform m_sphere3;
    /// <summary>
    /// An object that can be used for visualising points while debuging
    /// </summary>
    public Transform Sphere3 => m_sphere3;

    LineRenderer m_line;
    /// <summary>
    /// Can be used for visualising lines or paths while debuging. Line points should be set in LinePositions in this object
    /// </summary>
    public LineRenderer Line => m_line;

    [Tooltip("Will be instantiated to debug spheres")]
    [SerializeField]
    GameObject m_debugSpherePrefab;
    [Tooltip("Will be instantiated to the debug line")]
    [SerializeField]
    GameObject m_debugLinePrefab;

    void Awake() {
        if (m_instance != null && m_instance != this) {
            Destroy(this);
        } else {
            m_instance = this;
        }
        DontDestroyOnLoad(gameObject);

        Init();
    }

    /// <summary>
    /// Instantiating prefabs for debug objects and initializing them
    /// </summary>
    void Init() {
        m_sphere1 = Instantiate(m_debugSpherePrefab, transform).transform;
        m_sphere2 = Instantiate(m_debugSpherePrefab, transform).transform;
        m_sphere3 = Instantiate(m_debugSpherePrefab, transform).transform;

        m_sphere1.GetComponent<MeshRenderer>().material.color = Color.red;
        m_sphere2.GetComponent<MeshRenderer>().material.color = Color.green;
        m_sphere3.GetComponent<MeshRenderer>().material.color = Color.blue;

        m_sphere1.gameObject.SetActive(false);
        m_sphere2.gameObject.SetActive(false);
        m_sphere3.gameObject.SetActive(false);

        m_line = Instantiate(m_debugLinePrefab, transform).GetComponent<LineRenderer>();

        if (m_line.positionCount == 0) {
            m_line.gameObject.SetActive(false);
        }

        m_permanentLog = new Dictionary<int, string>();
    }

    /// <summary>
    /// Read Only. Contains the last MaxLines lines from the console log
    /// </summary>
    public string TextConsole {
        get {
            return m_textConsole;
        }
        private set {
            m_textConsole = value;
            foreach (var item in m_textsConsole) {
                item.text = m_textConsole;
            }
        }
    }

    Dictionary<int, string> m_permanentLog;
    string m_tempLog;

    /// <summary>
    /// Used to get or set custom temporary text for the ingame debug displays
    /// </summary>
    public string TextCustom {
        get {
            return m_textCustom;
        }
        set {
            m_tempLog = value;
            m_textCustom = PringCustomLog();
        }
    }

    /// <summary>
    /// Used to get or set line positions for the debug line renderer
    /// </summary>
    public Vector3[] LinePositions {
        get {
            Vector3[] positions = new Vector3[0];
            m_line.GetPositions(positions);
            return positions;
        }
        set {
            m_line.SetPositions(value);
            if (m_line.positionCount == 0) {
                m_line.gameObject.SetActive(false);
            } else {
                m_line.gameObject.SetActive(true);
            }
        }
    }

    void OnEnable() {
        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    /// <summary>
    /// Adds logString to the log queue and pushes it to ingame debug displays
    /// </summary>
    /// <param name="logString">Log message</param>
    /// <param name="stackTrace">Unused</param>
    /// <param name="type">Unused</param>
    void HandleLog(string logString, string stackTrace, LogType type) {
        // Restricting the length of the queue
        if (m_consoleQueue.Count >= m_maxLines)
            m_consoleQueue.Dequeue();

        m_consoleQueue.Enqueue(logString);

        // Making a string to print and printing it on ingame displays 
        var builder = new System.Text.StringBuilder();
        foreach (var item in m_consoleQueue) {
            builder.Append(item).Append("\n");
        }

        TextConsole = builder.ToString();
    }

    public static DebugVR Get() {
        return GameObject.Find("Debug").GetComponent<DebugVR>();
    }

    /// <summary>
    /// Prints the names of the gameobjects containing provided components to the permanent log
    /// </summary>
    /// <typeparam name="T">Any Component-derived class</typeparam>
    /// <param name="list">Components to list</param>
    /// <param name="id">Id of the log entry. Will create a new entry if null</param>
    /// <returns>Id of the log entry</returns>
    public int LogComponentGameobjects<T>(T[] list, int? id = null) where T : Component {
        var builder = new System.Text.StringBuilder();
        foreach (var item in list) {
            builder.Append(item.gameObject.name).Append("\n");
        }

        return UpdatePermanentLog(builder.ToString(), id);
    }

    /// <summary>
    /// Builds custom log and prints it on the ingame debug displays
    /// </summary>
    /// <returns>Built log</returns>
    string PringCustomLog() {
        var builder = new System.Text.StringBuilder();

        // Adding permanent log to a builder
        foreach (var item in m_permanentLog) {
            builder.Append(item.Value).Append("\n");
        }

        // Adding a separator
        if (m_permanentLog.Count > 0 && m_tempLog != "") {
            builder.Append("======\n");
        }

        // Adding temp log to a builder
        builder.Append(m_tempLog);

        // Printing the log on ingame displays 
        string log = builder.ToString();
        foreach (var item in m_textsCustom) {
            item.text = log;
        }

        return log;
    }

    /// <summary>
    /// Adds a string to be permanently logged on TextCustom
    /// </summary>
    /// <param name="text">New string to log</param>
    /// <returns>Id of the logged string</returns>
    int AddPermenentLog(string text) {
        // Getting a decently random int value for id
        int id = (int)((double)Random.value * int.MaxValue);
        // Making sure the id is unique
        while (m_permanentLog.ContainsKey(id)) {
            id = (int)((double)Random.value * int.MaxValue);
        }

        m_permanentLog.Add(id, text);
        return id;
    }

    /// <summary>
    /// Removes a permanently logged string
    /// </summary>
    /// <param name="id">Id of the string to remove</param>
    public void RemovePermenentLog(int? id) {
        if (id != null) {
            m_permanentLog.Remove(id.Value);
        }
    }

    /// <summary>
    /// Changes the text of a permanent log entry to a new value or creates a new entry
    /// </summary>
    /// <param name="text">New entry value</param>
    /// <param name="id">Id of the permanent entry. Will create a new entry if null</param>
    /// <returns>Id of the entry</returns>
    public int UpdatePermanentLog(string text, int? id = null) {
        if (id == null) {
            id = AddPermenentLog(text);
        } else {
            try {
                m_permanentLog[id.Value] = text;
            } catch (KeyNotFoundException) {
                id = AddPermenentLog(text);
            }
        }
        m_textCustom = PringCustomLog();

        return id.Value;
    }

    /// <summary>
    /// Clears permanent custom log
    /// </summary>
    public void ClearPermanantLog() {
        m_permanentLog.Clear();
    }
}
