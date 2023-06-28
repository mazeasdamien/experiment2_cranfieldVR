using Telexistence;
using UnityEngine;
using UnityEngine.XR;

namespace VarjoExample
{
    public class RestrictMovement : MonoBehaviour
    {
        public Transform point1;
        public Transform point2;

        public FanucHandler fanucHandler;

        public Material original;
        public Material notPossible;
        public Material inContact;

        public GameObject controller;
        private Renderer controllerRenderer;
        public bool isInContact;

        public Hand hand;

    private void Start()
        {
            // Get the Renderer component from the controller
            controllerRenderer = controller.GetComponent<Renderer>();
        }

        private void Update()
        {
            isInContact = false;
            foreach (var h in hand.contactedInteractables)
            {
                if (h.gameObject.name == "CURSOR_KINECT")
                {
                    isInContact = true;
                }
            }

            Vector3 position = transform.position;

            float x = Mathf.Clamp(position.x, Mathf.Min(point1.position.x, point2.position.x), Mathf.Max(point1.position.x, point2.position.x));
            float y = Mathf.Clamp(position.y, Mathf.Min(point1.position.y, point2.position.y), Mathf.Max(point1.position.y, point2.position.y));
            float z = Mathf.Clamp(position.z, Mathf.Min(point1.position.z, point2.position.z), Mathf.Max(point1.position.z, point2.position.z));
            transform.position = new Vector3(x, y, z);


            if (!fanucHandler.messageReachability)
            {
                // Change the controller's material to notPossible
                controllerRenderer.material = notPossible;
            }
            else if (isInContact)
            {
                // Change the controller's material to inContact
                controllerRenderer.material = inContact;
            }
            else if(!isInContact)
            {
                // Change the controller's material to the original
                controllerRenderer.material = original;
            }

        }
    }
}
