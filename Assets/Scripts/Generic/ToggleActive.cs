using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behavior that provides a method to toggle gameobect's active state without knowing its current state
/// </summary>
public class ToggleActive : MonoBehaviour
{
    public void Toggle() {
        if (gameObject.activeInHierarchy) {
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
        }
    }
}
