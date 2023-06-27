using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPTHandler : MonoBehaviour
{
    public enum Plane { ZX, XY, YZ }

    [Header("Circle Parameters")]
    public float radius = 2f;     // The radius of the circle.
    public float height;
    public float detailMultiplier = 10f; // Adjust this to change the detail of the circle
    public Plane plane = Plane.ZX;

    [Header("Controller and Prefab")]
    public GameObject controller; // The GameObject for start and end position
    public GameObject prefab;

    private LineRenderer lineRenderer;
    private int vertexCount;  // The number of vertices in the circle.
    private List<GameObject> instantiatedPrefabs = new List<GameObject>();  // List to hold all the instantiated prefabs.

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdatePrefabInstances();
    }

    void Update()
    {
        int newVertexCount = Mathf.Max(3, (int)(radius * detailMultiplier));
        if (newVertexCount != vertexCount)
        {
            vertexCount = newVertexCount;
            UpdatePrefabInstances();
        }

        // Check if instantiatedPrefabs is empty
        if (instantiatedPrefabs.Count > 0)
        {
            UpdateCircle();
        }
    }



    void UpdatePrefabInstances()
    {
        vertexCount = Mathf.Max(3, (int)(radius * detailMultiplier));

        // Destroy old prefabs if there are any.
        foreach (GameObject prefabInstance in instantiatedPrefabs)
        {
            Destroy(prefabInstance);
        }

        instantiatedPrefabs.Clear();  // Clear the list so new prefabs can be added.

        // Instantiate new prefabs
        for (int i = 0; i < vertexCount; i++)
        {
            GameObject newPrefab = Instantiate(prefab);
            instantiatedPrefabs.Add(newPrefab);
        }
    }

    void UpdateCircle()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = vertexCount + 2;  // The extra one is to close the circle and one for the start position.

        Vector3[] circlePoints = new Vector3[vertexCount + 2];
        for (int i = 0; i < vertexCount; i++)
        {
            float angle = ((float)i / vertexCount) * 360 * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            switch (plane)
            {
                case Plane.ZX:
                    circlePoints[i] = controller.transform.position + new Vector3(x, height, y);
                    break;
                case Plane.XY:
                    circlePoints[i] = controller.transform.position + new Vector3(x, y, height);
                    break;
                case Plane.YZ:
                    circlePoints[i] = controller.transform.position + new Vector3(height, y, x);
                    break;
            }

            // Update prefab position
            instantiatedPrefabs[i].transform.position = circlePoints[i];
        }

        // Set start position to controller's position and close the circle
        circlePoints[vertexCount] = circlePoints[0];
        circlePoints[vertexCount + 1] = controller.transform.position;

        lineRenderer.SetPositions(circlePoints);
    }
}
