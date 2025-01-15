using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// An extension to the ActionBasedContinuousMoveProvider that allows to toggle reading input of the left and the right controllers separately
/// </summary>
public class MoveProviderExtension : ActionBasedContinuousMoveProvider
{
    [Tooltip("Should the provider read the values of the left controller?")]
    [SerializeField]
    bool m_useLeftHand = true;
    [Tooltip("Should the provider read the values of the right controller?")]
    [SerializeField]
    bool m_useRightHand = true;
    /// <summary>
    /// Should the provider read the values of the left controller?
    /// </summary>
    public bool UseLeftHand {
        get => m_useLeftHand;
        set => m_useLeftHand = value;
    }
    /// <summary>
    /// Should the provider read the values of the right controller?
    /// </summary>
    public bool UseRightHand {
        get => m_useRightHand;
        set => m_useRightHand = value;
    }

    protected override Vector2 ReadInput() {
        // Saving input values of controllers if they are enabled, otherwise saving Vector2.zero
        var leftHandValue = m_useLeftHand ? (leftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero) : Vector2.zero;
        var rightHandValue = m_useRightHand ? (rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero) : Vector2.zero;

        // Returning combined input
        return leftHandValue + rightHandValue;
    }
}
