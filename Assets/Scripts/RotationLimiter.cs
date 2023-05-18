using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLimiter : MonoBehaviour
{
    public float xMin = -60f, xMax = 60f;
    public float yMin = -145f, yMax = -92f;
    public float zMin = -30f, zMax = 30f;

    // Store the current rotation
    private Quaternion rotation;

    private void Start()
    {
        rotation = transform.rotation;
    }

    private void Update()
    {
        // Convert rotation to Euler angles
        Vector3 euler = rotation.eulerAngles;

        // Adjust for 360 degree wrap around
        euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
        euler.y = (euler.y > 180) ? euler.y - 360 : euler.y;
        euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;

        // Clamp rotations
        euler.x = Mathf.Clamp(euler.x, xMin, xMax);
        euler.y = Mathf.Clamp(euler.y, yMin, yMax);
        euler.z = Mathf.Clamp(euler.z, zMin, zMax);

        // Convert back to quaternion and assign to rotation
        rotation = Quaternion.Euler(euler);
        transform.rotation = rotation;
    }
}
