using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelFix : MonoBehaviour
{

    void FixedUpdate()
    {
        // Fix model back to its position if lost by sync/physics
        if (transform.localPosition != Vector3.zero)
        transform.localPosition = Vector3.zero;
        if (transform.localRotation != Quaternion.Euler(Vector3.zero))
        transform.localRotation = Quaternion.Euler(Vector3.forward);
    }
}
