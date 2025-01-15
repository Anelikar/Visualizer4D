using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A singleton that provides properties to change various settings and synchronizes them across gameobjects
/// </summary>
public class XRSettingsManager : MonoBehaviour
{
	static XRSettingsManager m_instance;
	public static XRSettingsManager Instance => m_instance;

	LocomotionManager m_locomotionManager;

	public enum MovementMode
	{
		smooth = 0,
		teleportation = 1
	}
	[Tooltip("Current mode of movement")]
	[SerializeField]
	MovementMode m_movementMode = MovementMode.smooth;
	public enum Hand
	{
		left = 0,
		right = 1
	}
	[Tooltip("Hand used to move in smooth mode of movement")]
	[SerializeField]
	Hand m_movementHand = Hand.left;
	[Tooltip("Movement speed in m/s in smooth mode of movement")]
	[SerializeField]
	float m_moveSpeed;
	/// <summary>
	/// Current mode of movement
	/// </summary>
	public MovementMode Movement {
		get => m_movementMode;
		set {
			m_movementMode = value;
			UpdateLocomotion();
		}
	}
	/// <summary>
	/// Hand used to move in smooth mode of movement
	/// </summary>
	public Hand MovementHand {
		get => m_movementHand;
		set {
			m_movementHand = value;
			UpdateLocomotion();
		}
	}
	/// <summary>
	/// Movement speed in m/s in smooth mode of movement
	/// </summary>
	public float MovementSpeed {
		get => m_moveSpeed;
		set {
			if (m_locomotionManager != null) {
				if (m_locomotionManager.MoveProvider != null) {
					m_moveSpeed = value;
					m_locomotionManager.MoveProvider.moveSpeed = m_moveSpeed;
				}
			}
		}
	}

	public enum TurnMode
	{
		smooth = 0,
		snap = 1
	}
	[Tooltip("Current mode of turning")]
	[SerializeField]
	TurnMode m_turnMode = TurnMode.smooth;
	[Tooltip("Turn rate in deg/click in snap mode of turning")]
	[SerializeField]
	float m_turnSpeedSnap;
	[Tooltip("Turn rate in deg/sec in smooth mode of turning")]
	[SerializeField]
	float m_turnSpeedCont;
	/// <summary>
	/// Current mode of turning
	/// </summary>
	public TurnMode Turn {
		get => m_turnMode;
		set {
			m_turnMode = value;
			UpdateLocomotion();
		}
	}
	/// <summary>
	/// Turn rate in deg/click in snap mode of turning
	/// </summary>
	public float TurnSpeedSnap {
		get => m_turnSpeedSnap;
		set {
			if (m_locomotionManager != null) {
				if (m_locomotionManager.SnapTurnProvider != null) {
					m_turnSpeedSnap = value;
					m_locomotionManager.SnapTurnProvider.turnAmount = m_turnSpeedSnap;
				}
			}
		}
	}
	/// <summary>
	/// Turn rate in deg/sec in smooth mode of turning
	/// </summary>
	public float TurnSpeedCont {
		get => m_turnSpeedCont;
		set {
			if (m_locomotionManager != null) {
				if (m_locomotionManager.ContiniousTurnProvider != null) {
					m_turnSpeedCont = value;
					m_locomotionManager.ContiniousTurnProvider.turnSpeed = m_turnSpeedCont;
				}
			}
		}
	}

	public enum XRMode
	{
		VR = 0,
		AR = 1
	}
	[Tooltip("UNUSED Current xr enviroment mode")]
	[SerializeField]
	XRMode m_XREnviroment = XRMode.VR;
	/// <summary>
	/// UNUSED Current xr enviroment mode
	/// </summary>
	public XRMode XREnviroment {
		get => m_XREnviroment;
		set {
			m_XREnviroment = value;
			UpdateXR();
		}
	}

	/// <summary>
	/// Screen UI dropdowns to select movement mode
	/// </summary>
	public List<TMPro.TMP_Dropdown> MoveDropdowns;
	/// <summary>
	/// Screen UI dropdowns to select turn mode
	/// </summary>
	public List<TMPro.TMP_Dropdown> TurnDropdowns;
	/// <summary>
	/// UNUSED Screen UI dropdowns to select xr enviroment mode
	/// </summary>
	public List<TMPro.TMP_Dropdown> XRDropdowns;

	/// <summary>
	/// Teleport interactables that will be toggled when changing movement mode
	/// </summary>
	public List<BaseTeleportationInteractable> TeleportInteractables;

	bool m_isInitialised = false;

	void Awake() {
		if (m_instance != null && m_instance != this) {
			// Copying component references form the existing gameobject into a singleton
			m_instance.m_locomotionManager = m_locomotionManager;
			m_instance.MoveDropdowns = MoveDropdowns;
			m_instance.TurnDropdowns = TurnDropdowns;
			m_instance.XRDropdowns = XRDropdowns;
			m_instance.TeleportInteractables = TeleportInteractables;
			Destroy(this);
		} else {
			m_instance = this;
			DontDestroyOnLoad(gameObject);
			InitMembers();
			Init();
		}
	}

	/// <summary>
	/// Initialises dropdown and teleport interactables lists
	/// </summary>
	/// <param name="reset">Should this clear the lists if they already exist?</param>
	void InitMembers(bool reset = false) {
		if (MoveDropdowns == null || reset) {
			MoveDropdowns = new List<TMPro.TMP_Dropdown>();
		}
		if (TurnDropdowns == null || reset) {
			TurnDropdowns = new List<TMPro.TMP_Dropdown>();
		}
		if (XRDropdowns == null || reset) {
			XRDropdowns = new List<TMPro.TMP_Dropdown>();
		}
		if (TeleportInteractables == null || reset) {
			TeleportInteractables = new List<BaseTeleportationInteractable>();
		}
	}

	/// <summary>
	/// Checks for the correct initialization of the locomotion manager and syncronizes settings and speed values with it
	/// </summary>
	void Init() {
		if (m_locomotionManager == null) {
			// Trying to find the locomotion manager
			m_locomotionManager = GameObject.Find("XR Origin (XR Rig)").GetComponent<LocomotionManager>();
			if (m_locomotionManager == null) {
				return;
			}
		}
		// Checking if locomotion manager components are initialised
		if (m_locomotionManager.MoveProvider == null ||
			m_locomotionManager.ContiniousTurnProvider == null ||
			m_locomotionManager.SnapTurnProvider == null ||
			m_locomotionManager.TeleportProvider == null) {
			return;
		}

		// Sncronysing speed values with the locomotion manager
		m_moveSpeed = m_locomotionManager.MoveProvider.moveSpeed;
		m_turnSpeedCont = m_locomotionManager.ContiniousTurnProvider.turnSpeed;
		m_turnSpeedSnap = m_locomotionManager.SnapTurnProvider.turnAmount;
		m_isInitialised = true;

		UpdateLocomotion();
	}

	/// <summary>
	/// Toggles teleports and synchronizes locomotion manager and dropdown UIs sith this class's settings
	/// </summary>
	public void UpdateLocomotion() {
		if (!m_isInitialised) {
			// Trying to reinitialize
			Init();
			if (!m_isInitialised) {
				return;
			}
		}

		LocomotionManager.LocomotionFlags flags = BuildLocomotionFlags();

		// Toggling teleportation planes based on the movement mode
		if (flags.HasFlag(LocomotionManager.LocomotionFlags.teleport)) {
			SetTeleportsActive(true);
		} else {
			SetTeleportsActive(false);
		}

		// Synchronizing locomotion flags with this class's settings
		m_locomotionManager.Locomotion = flags;

		// Synchronizing UI dropdowns with this class's settings
		UpdateUIs();
	}

	/// <summary>
	/// Not implemented
	/// </summary>
	public void UpdateXR() {
		throw new System.NotImplementedException("XR mode selection is not implemented");
	}

	/// <summary>
	/// Builds LocomotionFlags enum from this class's settings enums
	/// </summary>
	/// <returns>LocomotionFlags enum built from this class's settings enums</returns>
	LocomotionManager.LocomotionFlags BuildLocomotionFlags() {
		LocomotionManager.LocomotionFlags flags = LocomotionManager.LocomotionFlags.empty;

		if (m_movementMode == MovementMode.smooth) {
			if (m_movementHand == Hand.left) {
				flags |= LocomotionManager.LocomotionFlags.contMoveLeft;
			} else {
				flags |= LocomotionManager.LocomotionFlags.contMoveRight;
			}

			if (m_turnMode == TurnMode.smooth) {
				if (m_movementHand == Hand.left) {
					flags |= LocomotionManager.LocomotionFlags.contTurnRight;
				} else {
					flags |= LocomotionManager.LocomotionFlags.contTurnLeft;
				}
			} else {
				if (m_movementHand == Hand.left) {
					flags |= LocomotionManager.LocomotionFlags.snapTurnRight;
				} else {
					flags |= LocomotionManager.LocomotionFlags.snapTurnLeft;
				}
			}
		} else {
			flags |= LocomotionManager.LocomotionFlags.teleport;
			if (m_turnMode == TurnMode.smooth) {
				flags |= LocomotionManager.LocomotionFlags.contTurnRight;
				flags |= LocomotionManager.LocomotionFlags.contTurnLeft;
			} else {
				flags |= LocomotionManager.LocomotionFlags.snapTurnRight;
				flags |= LocomotionManager.LocomotionFlags.snapTurnLeft;
			}
		}

		return flags;
	}

	/// <summary>
	/// Synchronizes UI dropdowns with this class's current settings
	/// </summary>
	void UpdateUIs() {
		foreach (var item in MoveDropdowns) {
			item.value = (int)m_movementMode;
		}
		foreach (var item in TurnDropdowns) {
			item.value = (int)m_turnMode;
		}
		foreach (var item in XRDropdowns) {
			item.value = (int)m_XREnviroment;
		}
	}

	/// <summary>
	/// Toggles all teleport plane components referenced in TeleportInteractables list
	/// </summary>
	/// <param name="active">Set teleports active or inactive?</param>
	void SetTeleportsActive(bool active) {
		foreach (var item in TeleportInteractables) {
			item.enabled = active;
		}
	}
}
