using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lastSafety : MonoBehaviour
{
    public bool hitRoof;
    public GameObject objectToChangeColor; // The object whose color you want to change

    void Start()
    {

    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.right);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(transform.position, transform.right * hit.distance, Color.red);

            if (hit.collider.gameObject.tag == "Roof")
            {
                hitRoof = true;
                objectToChangeColor.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                hitRoof = false;
                objectToChangeColor.GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            hitRoof = false;
            Debug.DrawRay(transform.position, transform.right * 1000, Color.green);
            objectToChangeColor.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}
