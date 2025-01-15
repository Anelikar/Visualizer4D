using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// A behaviour that invokes OnPress and OnRelease events when the provided input actions are started and cancelled
/// </summary>
public class OnButtonPress : MonoBehaviour
{
    [Tooltip("Input actions that will trigger the event")]
    public InputAction action;

    [Tooltip("Will be invoked when the button is presed")]
    public UnityEvent OnPress = new UnityEvent();

    [Tooltip("Will be invoked when the button is released")]
    public UnityEvent OnRelease = new UnityEvent();

    void Awake() {
        action.started += InvokePress;
        action.canceled += InvokeRelease;
    }

    void OnDestroy() {
        action.started -= InvokePress;
        action.canceled -= InvokeRelease;
    }

    void OnEnable() {
        action.Enable();
    }

    void OnDisable() {
        action.Disable();
    }

    /// <summary>
    /// Invokes OnPress event
    /// </summary>
    /// <param name="_">Discarded</param>
    void InvokePress(InputAction.CallbackContext _) {
        OnPress.Invoke();
    }

    /// <summary>
    /// Invokes OnRelease event
    /// </summary>
    /// <param name="_">Discarded</param>
    void InvokeRelease(InputAction.CallbackContext _) {
        OnRelease.Invoke();
    }
}
