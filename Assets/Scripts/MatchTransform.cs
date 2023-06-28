using Telexistence;
using UnityEngine;

public class MatchTransform : MonoBehaviour
{
    public GameObject objectToMatch;
    public GameObject referenceObject;

    public meshKinect mk;

    void Update()
    {
        if (objectToMatch != null && referenceObject != null)
        {
            if (mk.freezeMesh == false)
            {
                objectToMatch.transform.position = referenceObject.transform.position;
                objectToMatch.transform.rotation = referenceObject.transform.rotation;
            }
        }
    }
}
