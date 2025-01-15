using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class for all the sound fx behaviours
/// </summary>
public abstract class BaseSound : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Reference to a QuickSound behaviour.\n\nWill be set to this object's QuickSound if left empty.")]
    protected QuickSound m_quickSound;
    /// <summary>
    /// Reference to a QuickSound behaviour. Will be set to this object's QuickSound if left empty.
    /// </summary>
    public QuickSound Sound {
        get => m_quickSound;
        set => m_quickSound = value;
    }

    protected virtual void Awake() {
        if (m_quickSound == null) {
            m_quickSound = GetComponent<QuickSound>();
        }
    }
}
