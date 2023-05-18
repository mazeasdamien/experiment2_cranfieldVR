using UnityEngine;

public class RestrictMovement : MonoBehaviour
{
    public Transform point1;
    public Transform point2;

    public Vector3 rotationLimitsMin = new Vector3(-60, -145, -30);
    public Vector3 rotationLimitsMax = new Vector3(60, -92, 30);

    private void Update()
    {
        Vector3 position = transform.position;

        float x = Mathf.Clamp(position.x, Mathf.Min(point1.position.x, point2.position.x), Mathf.Max(point1.position.x, point2.position.x));
        float y = Mathf.Clamp(position.y, Mathf.Min(point1.position.y, point2.position.y), Mathf.Max(point1.position.y, point2.position.y));
        float z = Mathf.Clamp(position.z, Mathf.Min(point1.position.z, point2.position.z), Mathf.Max(point1.position.z, point2.position.z));

        transform.position = new Vector3(x, y, z);
    }

    public void Rotate(Vector3 rotation)
    {
        Vector3 newRotation = transform.eulerAngles + rotation;

        if (newRotation.x > 180) newRotation.x -= 360;
        if (newRotation.y > 180) newRotation.y -= 360;
        if (newRotation.z > 180) newRotation.z -= 360;

        if (newRotation.x < rotationLimitsMin.x || newRotation.x > rotationLimitsMax.x) return;
        if (newRotation.y < rotationLimitsMin.y || newRotation.y > rotationLimitsMax.y) return;
        if (newRotation.z < rotationLimitsMin.z || newRotation.z > rotationLimitsMax.z) return;

        transform.Rotate(rotation);
    }
}
