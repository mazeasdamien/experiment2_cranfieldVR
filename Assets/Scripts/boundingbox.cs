using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class boundingbox : MonoBehaviour
{
    public Transform p1;
    public Transform p2;
    public LineRenderer lineRenderer;
    public Button toggleButton;  // Reference to the Button component
    public GameObject center;

    private bool isVisible = true;  // Variable to keep track of the LineRenderer visibility

    void Start()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 24;

        // Add a listener to the Button component
        toggleButton.onClick.AddListener(ToggleBoundingBoxVisibility);
    }

    void Update()
    {
        if (p1 == null || p2 == null) return;

        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(p1.position.x, p1.position.y, p1.position.z);
        corners[1] = new Vector3(p1.position.x, p1.position.y, p2.position.z);
        corners[2] = new Vector3(p1.position.x, p2.position.y, p1.position.z);
        corners[3] = new Vector3(p1.position.x, p2.position.y, p2.position.z);
        corners[4] = new Vector3(p2.position.x, p1.position.y, p1.position.z);
        corners[5] = new Vector3(p2.position.x, p1.position.y, p2.position.z);
        corners[6] = new Vector3(p2.position.x, p2.position.y, p1.position.z);
        corners[7] = new Vector3(p2.position.x, p2.position.y, p2.position.z);

        // Calculate the center of the bounding box
        Vector3 centerPosition = (p1.position + p2.position) / 2.0f;
        center.transform.position = centerPosition;

        if (isVisible)
        {

            lineRenderer.SetPositions(new Vector3[]
        {
            corners[0], corners[1], corners[5], corners[4], // Bottom Face
            corners[1], corners[3], corners[7], corners[5], // Front Face
            corners[3], corners[2], corners[6], corners[7], // Top Face
            corners[2], corners[0], corners[4], corners[6], // Back Face
            corners[2], corners[3], corners[1], corners[0], // Left Face
            corners[6], corners[7], corners[5], corners[4]  // Right Face
        });
        }
    }

    void ToggleBoundingBoxVisibility()
    {
        // Toggle the visibility state
        isVisible = !isVisible;

        // Apply the state to the LineRenderer
        lineRenderer.enabled = isVisible;
    }
}
