using Telexistence;
using UnityEngine;

public class RestrictMovement : MonoBehaviour
{
    public Transform point1;
    public Transform point2;

    public FanucHandler fanucHandler;

    public Vector3 lastValidPosition;
    public Quaternion lastValidRotation;

    public Material original;
    public Material notPossible;

    public GameObject controller;
    private Renderer controllerRenderer;

    private void Start()
    {
        // Initialize last valid position and rotation
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;

        // Get the Renderer component from the controller
        controllerRenderer = controller.GetComponent<Renderer>();
    }

    private void Update()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        // Check if FanucHandler's conditions are met
        if (fanucHandler.messageReachability)
        {
            // Change the controller's material to the original
            controllerRenderer.material = original;

            float x = Mathf.Clamp(position.x, Mathf.Min(point1.position.x, point2.position.x), Mathf.Max(point1.position.x, point2.position.x));
            float y = Mathf.Clamp(position.y, Mathf.Min(point1.position.y, point2.position.y), Mathf.Max(point1.position.y, point2.position.y));
            float z = Mathf.Clamp(position.z, Mathf.Min(point1.position.z, point2.position.z), Mathf.Max(point1.position.z, point2.position.z));

            transform.position = new Vector3(x, y, z);
            // assuming there is a similar method to clamp rotation
            transform.rotation = rotation;
        }
        else
        {
            // Change the controller's material to notPossible
            controllerRenderer.material = notPossible;

            // Reset to last valid position and rotation when conditions are not met
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
        }
    }
}
