using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// This behaviour changes origin mode of the XR Origin after its initialization if run from the editor
/// </summary>
public class XROriginEditorInitialization : MonoBehaviour
{
    [Tooltip("Reference to the current XR origin. \n\nWill be set to this object if left empty.")]
    [SerializeField]
    XROrigin m_origin;

    [Tooltip("Reference to the CameraOffset transform")]
    [SerializeField]
    Transform m_offset;

    void Awake() {
        if (m_origin == null) {
            m_origin = GetComponent<XROrigin>();
        }
    }

    void Start() {
        // Running the code only when connected to the editor
#if UNITY_EDITOR
        // Tracking mode is set to "Floor" in the inspector as it's the mode that allows local floor tracking.
        // Here it's set to "Device" as local floor tracking doesn't work well when starting the project from the editor
        m_origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        m_offset.transform.localPosition = new Vector3(m_offset.transform.localPosition.x, m_origin.CameraYOffset, m_offset.transform.localPosition.z);
        Debug.Log("EDITOR: XR tracking mode is set to device");
#endif

        // Removing this component as it will only run once
        Destroy(this);
    }
}
