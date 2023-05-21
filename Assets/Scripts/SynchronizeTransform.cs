using UnityEngine;

public class SynchronizeTransform : MonoBehaviour
{
    public GameObject BaseGameObject; // Assign the base GameObject in the inspector
    public GameObject DeepChildGameObject; // Assign the deep child GameObject in the inspector

    // Update is called once per frame
    void Update()
    {
        // Synchronize position and rotation of the base GameObject to the deep child GameObject
        BaseGameObject.transform.position = DeepChildGameObject.transform.position;
        BaseGameObject.transform.rotation = DeepChildGameObject.transform.rotation;
    }
}
