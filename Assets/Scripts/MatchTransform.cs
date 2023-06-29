using Telexistence;
using UnityEngine;

public class MatchTransform : MonoBehaviour
{
    public GameObject objectToMatch;
    public GameObject referenceObject;
    public float positionLerpSpeed = 0.1f; // Speed of position interpolation
    public float rotationLerpSpeed = 0.1f; // Speed of rotation interpolation

    void Update()
    {
        if (objectToMatch != null && referenceObject != null)
        {
            // Interpolate position
            objectToMatch.transform.position = Vector3.Lerp(
                objectToMatch.transform.position,
                referenceObject.transform.position,
                positionLerpSpeed * Time.deltaTime
            );

            // Interpolate rotation
            objectToMatch.transform.rotation = Quaternion.Lerp(
                objectToMatch.transform.rotation,
                referenceObject.transform.rotation,
                rotationLerpSpeed * Time.deltaTime
            );
        }
    }
}
