using Telexistence;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

namespace VarjoExample
{
    public class RestrictMovement : MonoBehaviour
    {
        public Transform point1;
        public Transform point2;

        public VibrationController vibrationController;
        public FanucHandler fanucHandler;

        public Material original;
        public Material notPossible;
        public Material inContact;

        public GameObject controller;
        private Renderer controllerRenderer;
        public bool isInContact;

        private Vector3 previousPosition;
        private Quaternion previousRotation;
        public bool isRotating;

        public Hand hand;
        public bool isMoving;

        public GameObject prefabLastOk;
        private Vector3 lastReachablePosition;
        private GameObject instantiatedPrefab;
        private Quaternion lastReachableRotation;

        private void Start()
        {
            // Get the Renderer component from the controller
            controllerRenderer = controller.GetComponent<Renderer>();
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            lastReachablePosition = transform.position;
            lastReachableRotation = transform.rotation;
        }

        private void Update()
        {
            // Check if the GameObject is moving
            float movementThreshold = 0.001f; // Adjust this value as needed
            isMoving = (transform.position - previousPosition).magnitude > movementThreshold;

            // Update previous position
            previousPosition = transform.position;

            // Check if the GameObject is rotating
            float rotationThreshold = 0.001f; // Adjust this value as needed
            isRotating = Quaternion.Angle(previousRotation, transform.rotation) > rotationThreshold;

            // Update previous rotation
            previousRotation = transform.rotation;


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


            if (!fanucHandler.messageReachability || !fanucHandler.isYRotationInRange)
            {
                // Change the controller's material to notPossible
                vibrationController.TurnOnHaptic();
                controllerRenderer.material = notPossible;

                // Instantiate the prefab at the last known reachable position and rotation, if it hasn't been instantiated yet
                if (instantiatedPrefab == null)
                {
                    instantiatedPrefab = Instantiate(prefabLastOk, lastReachablePosition, lastReachableRotation);
                }
            }
            else if (isInContact)
            {
                // Change the controller's material to inContact
                controllerRenderer.material = inContact;

                // Update the last known reachable position and rotation
                lastReachablePosition = transform.position;
                lastReachableRotation = transform.rotation;

                // Destroy the instantiated prefab, if it exists
                if (instantiatedPrefab != null)
                {
                    Destroy(instantiatedPrefab);
                    instantiatedPrefab = null;
                }
            }
            else if (!isInContact)
            {
                // Change the controller's material to the original
                vibrationController.TurnOffHaptic();
                controllerRenderer.material = original;

                // Update the last known reachable position and rotation
                lastReachablePosition = transform.position;
                lastReachableRotation = transform.rotation;

                // Destroy the instantiated prefab, if it exists
                if (instantiatedPrefab != null)
                {
                    Destroy(instantiatedPrefab);
                    instantiatedPrefab = null;
                }
            }
        }
    }
}
