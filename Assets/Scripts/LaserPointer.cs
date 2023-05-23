using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarjoExample;
using Telexistence;

public class LaserPointer : MonoBehaviour
{
    public float maxDistance = 5f; // Maximum distance of the laser pointer
    private LineRenderer lineRenderer;

    public Material originalMaterial;
    public GameObject btn_photo;
    public Controller controller;
    private bool isActionExecuted = false;
    public int photo;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        RaycastHit hit;

        // The boolean returned by Physics.Raycast indicates whether the raycast hit anything
        bool didHit = Physics.Raycast(transform.position, transform.forward, out hit, maxDistance);

        if (controller.primary2DAxisTouch)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);

            // Check if the raycast hit something and if the hit object has the right tag
            if (didHit && hit.collider.gameObject.CompareTag("btn_photo"))
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hit.point);
                btn_photo.GetComponent<Renderer>().material.color = Color.yellow;

                if (controller.primary2DAxisClick && !isActionExecuted)
                {
                    photo = 1;
                    isActionExecuted = true;
                }

                if (!controller.primary2DAxisClick)
                {
                    photo = -1;
                    isActionExecuted = false;
                }
            }
            else if (btn_photo != null)
            {
                Renderer renderer = btn_photo.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = originalMaterial;
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }
}