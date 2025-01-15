using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that continiously places the XR origin collider under the main camera if it won't clip the terrain
/// </summary>
public class ColliderSync : MonoBehaviour
{
    [Tooltip("The reference to the XR Origin transform")]
    [SerializeField]
    Transform m_origin;
    /// <summary>
    /// The reference to the XR Origin transform. Origin collider will be reinitialized when this is changed.
    /// </summary>
    public Transform Origin {
        get => m_origin;
        set {
            m_origin = value;
            InitOriginCollider();
        }
    }
    CapsuleCollider m_originCollider;

    [Tooltip("The reference to the main camera")]
    [SerializeField]
    Transform m_camera;
    /// <summary>
    /// The reference to the main camera
    /// </summary>
    public Transform Camera {
        get => m_camera;
        set => m_camera = value;
    }

    [Tooltip("The tag of the floor terrain colliders")]
    [SerializeField]
    string m_floorTag = "floor";
    [Tooltip("Max distance of the raycast to the floor")]
    [SerializeField]
    float m_floorMaxDist = 10f;
    [Tooltip("Floor raycast will ignore these layers")]
    [SerializeField]
    LayerMask m_raycastLayerMask;
    /// <summary>
    /// The tag of the floor terrain colliders
    /// </summary>
    public string FloorTag {
        get => m_floorTag;
        set => m_floorTag = value;
    }
    /// <summary>
    /// Max distance of the raycast to the floor
    /// </summary>
    public float FloorMaxDst {
        get => m_floorMaxDist;
        set => m_floorMaxDist = value;
    }
    /// <summary>
    /// Floor raycast will ignore these layers
    /// </summary>
    public LayerMask RaycastLayerMask {
        get => m_raycastLayerMask;
        set => m_raycastLayerMask = value;
    }

    List<Collider> m_currentTerrainColliders;
    int? m_collidersLogId = null;
    int? m_distanceLogId = null;

    [Tooltip("Interval of collider synchronisation")]
    [SerializeField]
    float m_updateInterval = 0.5f;
    /// <summary>
    /// Interval of collider synchronisation
    /// </summary>
    public float UpdateInterval {
        get => m_updateInterval;
        set => m_updateInterval = value;
    }

    float m_nextUpdateTime;

    // Constants used in synchronization with the camera
    // A position of the camera at least 10cm above the floor is good enough
    const float SYNC_MIN_HEIGHT = 0.1f;
    // Also if synchronization didn't succeed in 10 seconds it probably won't ever succeed
    const float SYNC_MAX_TIME = 10f;
    const float SYNC_REPEAT_INTERVAL = 0.2f;
    const float SYNC_MAX_COUNT = SYNC_MAX_TIME / SYNC_REPEAT_INTERVAL;

    void Awake() {
        m_currentTerrainColliders = new List<Collider>();
    }

    void Start() {
        InitOriginCollider();
    }

    void OnDestroy() {
        DebugVR.Instance.RemovePermenentLog(m_collidersLogId);
        DebugVR.Instance.RemovePermenentLog(m_distanceLogId);
    }

    void OnTriggerEnter(Collider other) {
        // Adding colliders if they don't have the floor tag 
        // this behavoiur only moves the collider in xz plane so there is no risk of clipping floors
        if (!other.CompareTag(m_floorTag)) {
            m_currentTerrainColliders.Add(other);
        }

        m_collidersLogId = DebugVR.Instance.LogComponentGameobjects(m_currentTerrainColliders.ToArray(), m_collidersLogId);
    }

    void OnTriggerExit(Collider other) {
        m_currentTerrainColliders.Remove(other);
        // Forcing an update on the next frame
        m_nextUpdateTime = Time.time;

        m_collidersLogId = DebugVR.Instance.LogComponentGameobjects(m_currentTerrainColliders.ToArray(), m_collidersLogId);
    }

    void FixedUpdate() {
        // Moving this transform only on xz plane to avoid people ducking under colliders or jumping over them.
        // Legal y movements (e.g. ramps or stairs) will still be handled properly 
        // As they will move the origin's y position via the physics engine
        // And collider sync is supposed to be a child of the origin
        transform.position = new Vector3(m_camera.position.x, transform.position.y, m_camera.position.z);

        if (Time.time >= m_nextUpdateTime) {
            SyncCollider();
            m_nextUpdateTime += m_updateInterval;
        }
    }

    /// <summary>
    /// Teleports collider under the camera if the new position would be valid
    /// </summary>
    void SyncCollider() {
        // Checking for following conditions: 
        // 1. The trigger under the camera is not colliding with any terrain
        // 2. There is floor under the camera
        if (m_currentTerrainColliders.Count > 0) {
            return;
        }
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, m_floorMaxDist, m_raycastLayerMask, QueryTriggerInteraction.Ignore)) {
            m_distanceLogId = DebugVR.Instance.UpdatePermanentLog("Camera is not above the floor or the floor is too far", m_distanceLogId);
            return;
        } else if (!hit.collider.CompareTag(m_floorTag)) {
            m_distanceLogId = DebugVR.Instance.UpdatePermanentLog($"Camera is not above the floor. Raycast hit {hit.collider.gameObject.name}", m_distanceLogId);
            return;
        }
        m_distanceLogId = DebugVR.Instance.UpdatePermanentLog($"Floor distance: {hit.distance}", m_distanceLogId);

        // Setting origin collider center to camera's xz local position.
        // Y is controlled only by the physics engine to avoid fighting with gravity.
        m_originCollider.center = new Vector3(
            m_camera.localPosition.x,
            m_originCollider.center.y,
            m_camera.localPosition.z);
    }

    /// <summary>
    /// Gets a reference to the origin's collider and copies its height and center to this object's trigger collider.
    /// </summary>
    void InitOriginCollider() {
        // Setting the y position of this transform to camera's y if it's too low
        // This could happen in floor tracking mode
        // Because this gameobject is likely parented to Camera Offset
        // And the Camera Offset in floor tracking mode is at 0,0,0; not at initial camera height
        RaycastHit hit;
        bool floorFound = true;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, m_floorMaxDist, m_raycastLayerMask, QueryTriggerInteraction.Ignore)) {
            floorFound = false;
        } else if (!hit.collider.CompareTag(m_floorTag)) {
            floorFound = false;
        }
        if (!floorFound || hit.distance < SYNC_MIN_HEIGHT) {
            // Synchronization is done repeapedly in a couroutine 
            // As the camera is often still at the floor when this runs the first time
            Debug.Log("Attempting initial collider synchronization");
            StartCoroutine(TrySyncToCamera());
        } else {
            if (floorFound) {
                Debug.Log($"Raycast hit {hit.collider.gameObject.name}");
            }
            Debug.Log($"Floor found: {floorFound}, Distance: {hit.distance}\nInitial position is acceptable, skipping synchronization");
        }

        m_originCollider = m_origin.GetComponent<CapsuleCollider>();
        CapsuleCollider thisCollider = GetComponent<CapsuleCollider>();

        // Y is reversed because the origin's center is at the feet while the camera's is at the head
        thisCollider.center = new Vector3(
            m_originCollider.center.x, 
            -m_originCollider.center.y,
            m_originCollider.center.z);
        thisCollider.height = m_originCollider.height;
    }

    /// <summary>
    /// A coroutine method that tries to position this transform at the camera level
    /// </summary>
    IEnumerator TrySyncToCamera() {
        transform.position = new Vector3(transform.position.x, m_camera.position.y, transform.position.z);
        yield return new WaitForSeconds(SYNC_REPEAT_INTERVAL);
        int count = 1;

        while (transform.position.y - m_origin.position.y < SYNC_MIN_HEIGHT && count < SYNC_MAX_COUNT) {
            transform.position = new Vector3(transform.position.x, m_camera.position.y, transform.position.z);
            count++;
            yield return new WaitForSeconds(SYNC_REPEAT_INTERVAL);
        }

        if (count == SYNC_MAX_COUNT) {
            Debug.Log($"Synchronization failed in {count} tries");
        } else {
            Debug.Log($"Synchronization succeeded in {count} tries");
        }
    }

    /// <summary>
    /// Tries to to position collider sync transform at the camera level
    /// </summary>
    public void SyncToCamera() {
        StartCoroutine(TrySyncToCamera());
    }
}
