using Telexistence;
using UnityEngine;

public class MatchTransform : MonoBehaviour
{
    public GameObject objectToMatch;
    public GameObject referenceObject;

    void Update()
    {
        if (objectToMatch != null && referenceObject != null)
        {

                objectToMatch.transform.position = referenceObject.transform.position;
                objectToMatch.transform.rotation = referenceObject.transform.rotation;
        }
    }
}
