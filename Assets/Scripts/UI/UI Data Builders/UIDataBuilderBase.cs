using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for UI data constructors
/// </summary>
public abstract class UIDataBuilderBase : MonoBehaviour
{
    /// <summary>
    /// Info UI data block
    /// </summary>
    [SerializeField]
    protected InfoData m_data;

    /// <summary>
    /// Constructs the full UI data block
    /// </summary>
    public abstract void BuildData();
}
