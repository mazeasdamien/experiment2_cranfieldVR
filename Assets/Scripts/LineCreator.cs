using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Telexistence;

[RequireComponent(typeof(LineRenderer))]
public class LineCreator : MonoBehaviour
{
    public GameObject originObject; // the GameObject from which the line originates
    public float lineDistance = 10f; // the length of the line
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (originObject != null)
        {
            CreateLine();
        }
    }

    private void CreateLine()
    {
        // Two points to define a line, starting point is the game object's position, end point extends in the y direction
        Vector3[] points = new Vector3[2];

        // Starting point
        points[0] = originObject.transform.position;

        // End point
        points[1] = originObject.transform.position + originObject.transform.TransformDirection(0, lineDistance, 0);

        // Set the points on the Line Renderer
        lineRenderer.SetPositions(points);
    }
}
