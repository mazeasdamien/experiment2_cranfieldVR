using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movexrrig : MonoBehaviour
{
    public GameObject target1;
    public GameObject target2;
    private bool isAtTarget1;

    void Start()
    {
        isAtTarget1 = true;
        gameObject.transform.position = target1.transform.position;  // Start at position of target1
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (isAtTarget1)
            {
                gameObject.transform.position = target2.transform.position;  // Move to position of target2
                isAtTarget1 = false;
            }
            else
            {
                gameObject.transform.position = target1.transform.position;  // Move back to position of target1
                isAtTarget1 = true;
            }
        }
    }
}