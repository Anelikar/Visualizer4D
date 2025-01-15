using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behaviour that makes a UI element constantly turn in the direction of LookTarget
/// </summary>
public class UILookAt : MonoBehaviour
{
    [Tooltip("The target to look at")]
    public Transform LookTarget;
    new RectTransform transform;

    void Awake() {
        transform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (LookTarget != null) {
            transform.LookAt(LookTarget);
            // Restricting x and z rotations so that the UI is staying vertical
            // Flipping y 180 degrees because the forward vector on UIs is front to back
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
        } else {
            gameObject.SetActive(false);
        }
    }
}
