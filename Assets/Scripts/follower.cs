using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follower : MonoBehaviour
{
    public GameObject target; // The object this gameobject should follow
    public float yPosition; // The y-position this gameobject should maintain

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Follow target on X and Z axis
        Vector3 newPosition = new Vector3(target.transform.position.x, yPosition, target.transform.position.z);
        transform.position = newPosition;
    }
}
