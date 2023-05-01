using UnityEngine;

public class LockRotation : MonoBehaviour
{
    public bool lockRotation;

    public Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (lockRotation)
        {
            transform.rotation = initialRotation;
        }
    }
}