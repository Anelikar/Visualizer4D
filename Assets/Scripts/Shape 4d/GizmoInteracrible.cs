using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// XR Interactable behaviour that allows the gizmo to be grabbed and sends rotation information to the shape
/// </summary>
public class GizmoInteracrible : XRBaseInteractable
{
    [Header("Gizmo settings")]
    [Tooltip("Prefab of an object used in hover visuals")]
    [SerializeField]
    GameObject m_AttachPointPrefab;

    [Tooltip("Pivot to the gizmo")]
    [SerializeField]
    Transform m_pivot;
    [Tooltip("Gameobject holding all gizmos. Its localSize is used in calculating final gizmo radius")]
    [SerializeField]
    Transform m_gizmos;
    [Tooltip("Local radius of this gizmo")]
    [SerializeField]
    float m_gizmoRaduis = 0.53f;

    public enum Planes
    {
        YZ,
        XZ,
        XY,
        XW,
        YW,
        ZW
    }
    [Tooltip("The gizmo will rotate the shape in this plane")]
    [SerializeField]
    Planes m_gizmoPlane = Planes.YZ;

    [Tooltip("LineRenderer used in select visuals")]
    [SerializeField]
    LineRenderer m_grabLineRenderer;

    Transform m_activeAttach;

    List<IXRHoverInteractor> m_hoverInteractors;
    Transform m_hoveringTransform;

    IXRSelectInteractor m_selectInteractor;

    [Tooltip("This is called when the gizmo is rotated. Delta of the rotation is passed as arguments")]
    [SerializeField]
    UnityEvent<Vector3, Vector3> m_onRotation;
    /// <summary>
    /// This is called when the gizmo is rotated. Delta of the rotation is passed as arguments
    /// </summary>
    public UnityEvent<Vector3, Vector3> OnRotation => m_onRotation;

    protected override void Awake() {
        base.Awake();
        m_hoverInteractors = new List<IXRHoverInteractor>();
    }

    protected override void OnEnable() {
        base.OnEnable();

        selectEntered.AddListener(SelectEnter);
        selectExited.AddListener(SelectExit);
        hoverEntered.AddListener(HoverEnter);
        hoverExited.AddListener(HoverExit);
    }

    protected override void OnDisable() {
        base.OnDisable();

        selectEntered.RemoveListener(SelectEnter);
        selectExited.RemoveListener(SelectExit);
        hoverEntered.RemoveListener(HoverEnter);
        hoverExited.RemoveListener(HoverExit);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed) {
            if (m_hoveringTransform != null) {
                // Updating hover visuals
                moveAttach(closestOnGizmo(m_hoveringTransform.position));
            }
            if (m_selectInteractor != null) {
                // Rotating the gizmo and the shape
                trackGrab();

                // Updating select visuals
                m_grabLineRenderer.SetPosition(0, m_activeAttach.position);
                m_grabLineRenderer.SetPosition(1, m_selectInteractor.transform.position);
            }
        }
    }

    void HoverEnter(HoverEnterEventArgs args) {
        // Adding interactors to the list to keep track of them if multiple are present
        m_hoverInteractors.Add(args.interactorObject);

        // Swithcing hover visuals to a new interactor if this is not currently selected
        if (m_selectInteractor == null) {
            m_hoveringTransform = args.interactorObject.transform;
            spawnAttach(closestOnGizmo(m_hoveringTransform.position));
        }
    }

    void HoverExit(HoverExitEventArgs args) {
        m_hoverInteractors.Remove(args.interactorObject);

        // Updating hover visuals if this is not currently selected
        if (m_selectInteractor == null) {
            if (m_hoverInteractors.Count == 0) {
                // Disabling hover visuals
                m_hoveringTransform = null;
                destroyAttach();
            } else {
                // Swithcing hover visuals to a previous interactor
                m_hoveringTransform = m_hoverInteractors[m_hoverInteractors.Count - 1].transform;
            }
        }
    }

    void SelectEnter(SelectEnterEventArgs args) {
        // Checking that the intractable isn't already selected
        // m_activeAttach is checked to avoid occasional desync between it and m_selectInteractor
        if (m_selectInteractor == null && m_activeAttach != null) {
            m_selectInteractor = args.interactorObject;

            // Disabling hover visuals and enabling select visuals
            m_hoveringTransform = null;
            m_grabLineRenderer.enabled = true;
            m_grabLineRenderer.SetPosition(0, m_activeAttach.position);
            m_grabLineRenderer.SetPosition(1, m_selectInteractor.transform.position);
        }
    }

    void SelectExit(SelectExitEventArgs args) {
        // Checking that the interactable was deselected by a relevant interactor
        if (m_selectInteractor == args.interactorObject) {
            m_selectInteractor = null;

            // Disabling select visuals
            destroyAttach();
            m_grabLineRenderer.enabled = false;

            // Enabling hover visuals if there is another interactor hovering this
            if (m_hoverInteractors.Count > 0) {
                m_hoveringTransform = m_hoverInteractors[m_hoverInteractors.Count - 1].transform;
                spawnAttach(closestOnGizmo(m_hoveringTransform.position));
            }
        }
    }

    /// <summary>
    /// Rotates the gizmo and the shape based on the difference between the angles from the pivot to the attach and the grab transforms
    /// </summary>
    void trackGrab() {
        // Finding vectors pointing to the attach and grab transforms
        Vector3 attachDir = m_activeAttach.position - m_pivot.position;
        Vector3 grabDir = m_selectInteractor.transform.position - m_pivot.position;

        // Finding angles to the attach and grab transforms
        // There is a pair of gizmos in every 3d axis
        // and the angles are the same for both gizmos in the pair
        float attachAngle = 0;
        float grabAngle = 0;
        switch (m_gizmoPlane) {
            case Planes.YZ:
            case Planes.XW:
                // Projecting and normalizing vectors to get values in a correct range for the acos function
                attachDir = new Vector3(0, attachDir.y, attachDir.z).normalized;
                grabDir = new Vector3(0, grabDir.y, grabDir.z).normalized;
                // Finding angles. 
                // This part will give us angles in the 0..180 range since we aren't using the second coordinate yet
                attachAngle = Mathf.Acos(attachDir.z) * Mathf.Rad2Deg;
                grabAngle = Mathf.Acos(grabDir.z) * Mathf.Rad2Deg;
                // Using the second coordinate to get a full 0..360 range
                if (grabDir.y < 0) {
                    grabAngle = 360 - grabAngle;
                }
                if (attachDir.y < 0) {
                    attachAngle = 360 - attachAngle;
                }
                break;
            case Planes.XZ:
            case Planes.YW:
                attachDir = new Vector3(attachDir.x, 0, attachDir.z).normalized;
                grabDir = new Vector3(grabDir.x, 0, grabDir.z).normalized;
                attachAngle = Mathf.Acos(attachDir.z) * Mathf.Rad2Deg;
                grabAngle = Mathf.Acos(grabDir.z) * Mathf.Rad2Deg;
                if (grabDir.x < 0) {
                    grabAngle = 360 - grabAngle;
                }
                if (attachDir.x < 0) {
                    attachAngle = 360 - attachAngle;
                }
                break;
            case Planes.XY:
            case Planes.ZW:
                attachDir = new Vector3(attachDir.x, attachDir.y, 0).normalized;
                grabDir = new Vector3(grabDir.x, grabDir.y, 0).normalized;
                attachAngle = Mathf.Acos(attachDir.y) * Mathf.Rad2Deg;
                grabAngle = Mathf.Acos(grabDir.y) * Mathf.Rad2Deg;
                if (grabDir.x < 0) {
                    grabAngle = 360 - grabAngle;
                }
                if (attachDir.x < 0) {
                    attachAngle = 360 - attachAngle;
                }
                break;
            default:
                break;
        }

        // Creating unit vectors to make euler angles rotations later
        // Coordinates for finding the angles were chosen based on readability
        // So the actual directions are corrected there by flipping some of them
        Vector3 eulerRotation = Vector3.zero;
        Vector3 wEulerRotation = Vector3.zero;
        switch (m_gizmoPlane) {
            case Planes.YZ:
                eulerRotation = new Vector3(-1, 0, 0);
                break;
            case Planes.XZ:
                eulerRotation = new Vector3(0, 1, 0);
                break;
            case Planes.XY:
                eulerRotation = new Vector3(0, 0, -1);
                break;
            case Planes.XW:
                wEulerRotation = new Vector3(-1, 0, 0);
                break;
            case Planes.YW:
                wEulerRotation = new Vector3(0, 1, 0);
                break;
            case Planes.ZW:
                wEulerRotation = new Vector3(0, 0, -1);
                break;
            default:
                break;
        }

        // Ingame gizmos are positioned 90 degrees apart by rotating their parents
        // so we only care about rotating actual gizmos here in z axis
        transform.Rotate(Vector3.forward * (grabAngle - attachAngle));

        // Although we are only rotating in 1 out of 6 planes
        // The full desired rotation is sent to keep the event generic
        m_onRotation.Invoke(eulerRotation * (attachAngle - grabAngle), wEulerRotation * (attachAngle - grabAngle));
    }

    /// <summary>
    /// Instantiates an object to be used in hover visuals
    /// </summary>
    /// <param name="attachPoint">Initial position of the instantiated object</param>
    void spawnAttach(Vector3 attachPoint) {
        if (m_activeAttach == null) {
            m_activeAttach = Instantiate(m_AttachPointPrefab, attachPoint, Quaternion.identity).transform;
            m_activeAttach.SetParent(transform);
        }
    }

    /// <summary>
    /// Moves activeAttach to the attachPoint
    /// </summary>
    /// <param name="attachPoint">New position of the activeAttach</param>
    void moveAttach(Vector3 attachPoint) {
        if (m_activeAttach != null) {
            m_activeAttach.position = attachPoint;
        }
    }

    /// <summary>
    /// Destroys an obect used in hover visuals
    /// </summary>
    void destroyAttach() {
        if (m_activeAttach != null) {
            Destroy(m_activeAttach.gameObject);
            m_activeAttach = null;
        }
    }

    /// <summary>
    /// Finds a point on a gizmo that is the closest to a given point
    /// </summary>
    /// <param name="point"></param>
    /// <returns>A point on a gizmo that is the closest to a given point</returns>
    Vector3 closestOnGizmo(Vector3 point) {
        Vector3 direction = (point - m_pivot.position).normalized;
        return new Vector3(
            direction.x * m_gizmos.localScale.x * m_gizmoRaduis,
            direction.y * m_gizmos.localScale.y * m_gizmoRaduis,
            direction.z * m_gizmos.localScale.z * m_gizmoRaduis) + m_pivot.position;
    }
}
