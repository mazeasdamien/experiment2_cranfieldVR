using UnityEngine;

public class RestrictMovement : MonoBehaviour
{
    public Transform point1;
    public Transform point2;


    private void Update()
    {
        Vector3 position = transform.position;

        float x = Mathf.Clamp(position.x, Mathf.Min(point1.position.x, point2.position.x), Mathf.Max(point1.position.x, point2.position.x));
        float y = Mathf.Clamp(position.y, Mathf.Min(point1.position.y, point2.position.y), Mathf.Max(point1.position.y, point2.position.y));
        float z = Mathf.Clamp(position.z, Mathf.Min(point1.position.z, point2.position.z), Mathf.Max(point1.position.z, point2.position.z));

        transform.position = new Vector3(x, y, z);
    }
}
