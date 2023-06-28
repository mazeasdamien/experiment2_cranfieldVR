using Telexistence;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Collections;

namespace VarjoExample
{
    public class RestrictMovement : MonoBehaviour
    {
        public Transform point1;
        public Transform point2;

        public FanucHandler fanucHandler;
        public meshKinect meshKinect;

        public Material original;
        public Material notPossible;
        public Material inContact;

        public GameObject controller;
        private Renderer controllerRenderer;
        public bool isInContact;

        private Vector3 previousPosition;
        private Coroutine freezeCoroutine;
        private Quaternion previousRotation;
        public bool isRotating;

        public Hand hand;
        public bool isMoving;

        private void Start()
        {
            // Get the Renderer component from the controller
            controllerRenderer = controller.GetComponent<Renderer>();
            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }

        private IEnumerator UnfreezeMeshAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            // Unfreeze your mesh here
            meshKinect.freezeMesh = false;
        }

        private void Update()
        {
            // Check if the GameObject is moving
            float movementThreshold = 0.001f; // Adjust this value as needed
            isMoving = (transform.position - previousPosition).magnitude > movementThreshold;

            // Update previous position
            previousPosition = transform.position;

            if (isMoving)
            {
                meshKinect.freezeMesh = true;

                // If the Coroutine is running, stop it
                if (freezeCoroutine != null)
                {
                    StopCoroutine(freezeCoroutine);
                }
            }
            else
            {
                // Start the Coroutine to unfreeze the mesh after a delay
                freezeCoroutine = StartCoroutine(UnfreezeMeshAfterDelay());
            }
            // Check if the GameObject is rotating
            float rotationThreshold = 0.001f; // Adjust this value as needed
            isRotating = Quaternion.Angle(previousRotation, transform.rotation) > rotationThreshold;

            // Update previous rotation
            previousRotation = transform.rotation;

            if (isMoving || isRotating)
            {
                meshKinect.freezeMesh = true;

                // If the Coroutine is running, stop it
                if (freezeCoroutine != null)
                {
                    StopCoroutine(freezeCoroutine);
                }
            }
            else
            {
                // Start the Coroutine to unfreeze the mesh after a delay
                freezeCoroutine = StartCoroutine(UnfreezeMeshAfterDelay());
            }

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
            else if (!isInContact)
            {
                // Change the controller's material to the original
                controllerRenderer.material = original;
            }

        }
    }
}
