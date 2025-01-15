using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

/// <summary>
/// A behaviour that provides methods to update differrent locomotion providers using LocomotionFlags enum
/// </summary>
public class LocomotionManager : MonoBehaviour
{
    [Tooltip("A reference to the MoveProviderExtension")]
    [SerializeField]
    MoveProviderExtension m_moveProvider;
    [Tooltip("A reference to the ContiniousTurnProviderExtension")]
    [SerializeField]
    ContiniousTurnProviderExtension m_continiousTurnProvider;
    [Tooltip("A reference to the SnapTurnProviderExtension")]
    [SerializeField]
    SnapTurnProviderExtension m_snapTurnProvider;
    [Tooltip("A reference to the TeleportationProvider")]
    [SerializeField]
    TeleportationProvider m_teleportationProvider;
    /// <summary>
    /// A reference to the MoveProviderExtension
    /// </summary>
    public MoveProviderExtension MoveProvider => m_moveProvider;
    /// <summary>
    /// A reference to the ContiniousTurnProviderExtension
    /// </summary>
    public ContiniousTurnProviderExtension ContiniousTurnProvider => m_continiousTurnProvider;
    /// <summary>
    /// A reference to the SnapTurnProviderExtension
    /// </summary>
    public SnapTurnProviderExtension SnapTurnProvider => m_snapTurnProvider;
    /// <summary>
    /// A reference to the TeleportationProvider
    /// </summary>
    public TeleportationProvider TeleportProvider => m_teleportationProvider;

    [System.Flags]
    public enum LocomotionFlags
    {
        empty = 0,

        contMoveLeft = 1 << 0,
        contMoveRight = 1 << 1,
        teleport = 1 << 2,

        contTurnLeft = 1 << 4,
        contTurnRight = 1 << 5,
        snapTurnLeft = 1 << 6,
        snapTurnRight = 1 << 7
    };
    LocomotionFlags m_locomotion;
    /// <summary>
    /// Current locomotion flags. Will update Locomotion providers when changed.
    /// </summary>
    public LocomotionFlags Locomotion {
        get => m_locomotion;
        set => UpdateLocomotion(value);
    }

    /// <summary>
    /// Adds a flag to the current LocomotionFlags enum and updates locomotion providers
    /// </summary>
    /// <param name="flag">Flag to add</param>
    public void AddLocomotionFlag(LocomotionFlags flag) {
        UpdateLocomotion(m_locomotion | flag);
    }

    /// <summary>
    /// Removes a flag from the current LocomotionFlags enum and updates locomotion providers
    /// </summary>
    /// <param name="flag">Flag to remove</param>
    public void RemoveLocomotionFlag(LocomotionFlags flag) {
        UpdateLocomotion(m_locomotion & ~flag);
    }

    /// <summary>
    /// Updates different locomotion providers behaviour according to the given locomotion flags
    /// </summary>
    /// <param name="loc">Locomotion flags that will be used in updating locomotion providers</param>
    void UpdateLocomotion(LocomotionFlags loc) {
        VerifyFlags(ref loc);

        // Updating movement
        if (loc.HasFlag(LocomotionFlags.teleport)) {
            m_teleportationProvider.enabled = true;
            m_moveProvider.UseLeftHand = false;
            m_moveProvider.UseRightHand = false;
            m_moveProvider.enabled = false;
        } else {
            m_moveProvider.enabled = true;
            if (loc.HasFlag(LocomotionFlags.contMoveLeft)) {
                m_moveProvider.UseLeftHand = true;
                m_moveProvider.UseRightHand = false;
            }
            if (loc.HasFlag(LocomotionFlags.contMoveRight)) {
                m_moveProvider.UseLeftHand = false;
                m_moveProvider.UseRightHand = true;
            }
        }

        // Updating turning
        if (loc.HasFlag(LocomotionFlags.snapTurnLeft) || loc.HasFlag(LocomotionFlags.snapTurnRight)) {
            m_snapTurnProvider.enabled = true;
            m_continiousTurnProvider.enabled = false;
            if (loc.HasFlag(LocomotionFlags.snapTurnLeft)) {
                m_snapTurnProvider.UseLeftHand = true;
            } else {
                m_snapTurnProvider.UseLeftHand = false;
            }
            if (loc.HasFlag(LocomotionFlags.snapTurnRight)) {
                m_snapTurnProvider.UseRightHand = true;
            } else {
                m_snapTurnProvider.UseRightHand = false;
            }
        } else {
            m_snapTurnProvider.enabled = false;
            m_continiousTurnProvider.enabled = true;
            if (loc.HasFlag(LocomotionFlags.contTurnLeft)) {
                m_continiousTurnProvider.UseLeftHand = true;
            } else {
                m_continiousTurnProvider.UseLeftHand = false;
            }
            if (loc.HasFlag(LocomotionFlags.contTurnRight)) {
                m_continiousTurnProvider.UseRightHand = true;
            } else {
                m_continiousTurnProvider.UseRightHand = false;
            }
        }

        m_locomotion = loc;
    }

    /// <summary>
    /// Makes sure that the locomotion flags make sense and don't conflict with themselves and fixes them otherwise
    /// </summary>
    /// <param name="loc">Locomotion flags that will be checked</param>
    void VerifyFlags(ref LocomotionFlags loc) {
        // If there are no movement options, adding teleportation, otherwise making sure that teleportation and smooth movement aren't enabled together (priority to teleportation)
        // Making sure there is one hand left free for turning (priority to movement with the left hand, turning with the right one)
        if (!(loc.HasFlag(LocomotionFlags.contMoveLeft) || loc.HasFlag(LocomotionFlags.contMoveRight) || loc.HasFlag(LocomotionFlags.teleport))) {
            loc |= LocomotionFlags.teleport;
        } else {
            if (loc.HasFlag(LocomotionFlags.teleport)) {
                loc &= ~LocomotionFlags.contMoveLeft;
                loc &= ~LocomotionFlags.contMoveRight;
            } else if (loc.HasFlag(LocomotionFlags.contMoveLeft) && loc.HasFlag(LocomotionFlags.contMoveRight)) {
                loc &= ~LocomotionFlags.contMoveRight;
            }
        }

        // If there are no turning options, adding turning to movement-free hands, otherewise making sure that smooth and snap turning aren't enabled together on the same hand (priority to snap)
        if (!(loc.HasFlag(LocomotionFlags.contTurnLeft) || loc.HasFlag(LocomotionFlags.snapTurnLeft) || loc.HasFlag(LocomotionFlags.contTurnRight) || loc.HasFlag(LocomotionFlags.snapTurnRight))) {
            if (!loc.HasFlag(LocomotionFlags.contMoveLeft)) {
                loc |= LocomotionFlags.snapTurnLeft;
            }
            if (!loc.HasFlag(LocomotionFlags.contMoveRight)) {
                loc |= LocomotionFlags.snapTurnRight;
            }
        } else {
            if (loc.HasFlag(LocomotionFlags.snapTurnLeft)) {
                loc &= ~LocomotionFlags.contTurnLeft;
            }
            if (loc.HasFlag(LocomotionFlags.snapTurnRight)) {
                loc &= ~LocomotionFlags.contTurnRight;
            }
        }
    }
}
