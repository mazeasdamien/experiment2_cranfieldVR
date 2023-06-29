using Telexistence;
using UnityEngine;

public class MatchTransform : MonoBehaviour
{
    public modalities m;
    public GameObject outpose;
    public GameObject objectToMatch;
    public GameObject referenceObject;
    public float positionLerpSpeed = 0.1f; // Speed of position interpolation
    public float rotationLerpSpeed = 0.1f; // Speed of rotation interpolation

    void Update()
    {
        if (m.usePT)
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
        else
        {
            // Interpolate position
            objectToMatch.transform.position = Vector3.Lerp(
                objectToMatch.transform.position,
                outpose.transform.position,
                positionLerpSpeed * Time.deltaTime
            );

            // Interpolate rotation
            objectToMatch.transform.rotation = Quaternion.Lerp(
                objectToMatch.transform.rotation,
                outpose.transform.rotation,
                rotationLerpSpeed * Time.deltaTime
           );
        }
    }
}
