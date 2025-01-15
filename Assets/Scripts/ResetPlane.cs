using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that returns tracked gameobjects to their original positions when the collide with the plane
/// </summary>
public class ResetPlane : MonoBehaviour
{
    /// <summary>
    /// Holds position and rotation of the transform
    /// </summary>
    class TransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformData(Vector3 position, Quaternion rotation) {
            Position = position;
            Rotation = rotation;
        }
    }

    [Tooltip("Gameobjects that will be tracked and reset when colliding with the plane. Reset list must be updated if this is changed at runtime")]
    [SerializeField]
    List<GameObject> ResetObjects;

    List<TransformData> m_defaultTransforms;
    List<Rigidbody> m_resetRigidbodies;

    void Awake() {
        if (ResetObjects == null) {
            ResetObjects = new List<GameObject>();
        }
        UpdateResetList();
    }

    /// <summary>
    /// Saves current positions of all tracked objects as default
    /// </summary>
    public void UpdateResetList() {
        m_defaultTransforms = new List<TransformData>();
        m_resetRigidbodies = new List<Rigidbody>();
        for (int i = 0; i < ResetObjects.Count; i++) {
            Rigidbody r = ResetObjects[i].GetComponent<Rigidbody>();
            if (r != null) {
                m_resetRigidbodies.Add(r);
                m_defaultTransforms.Add(new TransformData(ResetObjects[i].transform.position, ResetObjects[i].transform.rotation));
            }
        }
    }

    /// <summary>
    /// Saves current position and rotation of the provided gameobject
    /// </summary>
    /// <param name="obj">Gameobject that will be updated</param>
    public void UpdateDefault(GameObject obj) {
        // Finding the index of the object
        int index = -1;
        for (int i = 0; i < ResetObjects.Count; i++) {
            if (ResetObjects[i] == obj) {
                index = i;
                break;
            }
        }
        if (index == -1) {
            return;
        }

        // Updating defaults
        m_defaultTransforms[index].Position = obj.transform.position;
        m_defaultTransforms[index].Rotation = obj.transform.rotation;
    }

    /// <summary>
    /// Sets a gameobject to be tracked for reset
    /// </summary>
    /// <param name="obj">New tracked object</param>
    public void AddTrackedObject(GameObject obj) {
        ResetObjects.Add(obj);
        Rigidbody r = obj.GetComponent<Rigidbody>();
        if (r != null) {
            m_resetRigidbodies.Add(r);
            m_defaultTransforms.Add(new TransformData(obj.transform.position, obj.transform.rotation));
        }
    }

    public void RemoveTrackedObject(GameObject obj) {
        // Finding the index of the object
        int index = -1;
        for (int i = 0; i < ResetObjects.Count; i++) {
            if (ResetObjects[i] == obj) {
                index = i;
                break;
            }
        }
        if (index == -1) {
            return;
        }

        // Removing found object from tracking
        ResetObjects.RemoveAt(index);
        m_resetRigidbodies.RemoveAt(index);
        m_defaultTransforms.RemoveAt(index);
    }

    void OnCollisionEnter(Collision collision) {
        // Finding which tracked gameobject has enterered collision
        int index = -1;
        for (int i = 0; i < ResetObjects.Count; i++) {
            if (ResetObjects[i] == collision.gameObject) {
                index = i;
                break;
            }
        }
        if (index == -1) {
            return;
        }

        try {
            // Setting tracked gameobect's position and rotation to their default
            Rigidbody r = m_resetRigidbodies[index];
            r.position = m_defaultTransforms[index].Position;
            r.rotation = m_defaultTransforms[index].Rotation;

            // Resetting tracked gameobect's momentum
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        } catch (System.Exception e) {
            // This will probably only run if index is out of bounds, which means some object was deleted from a reset list from the inspector at runtime.
            Debug.LogError("Reset list is probably desynced. Try calling UpdateResetList() after changing it.\n" + e.Message);
        }
    }
}
