using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// A behaviour that invokes an event every time the scene is loaded
/// </summary>
public class OnSceneLoad : MonoBehaviour
{
    [Tooltip("Will be invoked every time the scene is loaded")]
    public UnityEvent OnLoad = new UnityEvent();

    UnityAction<Scene, LoadSceneMode> m_sceneLoadAction;

    private void Awake() {
        m_sceneLoadAction = (x, y) => { OnLoad.Invoke(); };

        SceneManager.sceneLoaded += m_sceneLoadAction;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= m_sceneLoadAction;
    }
}
