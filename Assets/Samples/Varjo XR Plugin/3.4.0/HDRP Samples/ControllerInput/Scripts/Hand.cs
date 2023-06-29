using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Telexistence;
using UnityEngine.UI;

namespace VarjoExample
{
    public class Hand : MonoBehaviour
    {
        public Transform xrRig;

        Controller controller;

        public List<Interactable> contactedInteractables = new List<Interactable>();
        private bool triggerDown;
        private FixedJoint fixedJoint = null;
        public Interactable currentInteractable;
        private Rigidbody heldObjectBody;

        public videoKinect videoKinect;
        public FanucHandler FH;
        public GameObject c;

        void Awake()
        {
            controller = GetComponent<Controller>();
            fixedJoint = GetComponent<FixedJoint>();

        }

        // Update is called once per frame
        void Update()
        {
            if (controller.triggerButton)
            {
                if (!triggerDown)
                {
                    triggerDown = true;
                    Pick();
                }
            }
            else if (!controller.primary2DAxisClick && triggerDown)
            {
                triggerDown = false;
                Drop();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Pickable"))
            {
                    Interactable interactable = other.gameObject.GetComponent<Interactable>();
                    contactedInteractables.Add(interactable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Pickable"))
            {
                    Interactable interactable = other.gameObject.GetComponent<Interactable>();
                    contactedInteractables.Remove(interactable);
            }
        }

        public void Pick()
        {
            currentInteractable = GetNearestInteractable();

            if (!currentInteractable)
            {
                return;
            }
                // Drop interactable if already held
                if (currentInteractable.activeHand)
                {
                    currentInteractable.activeHand.Drop();
                }

                // Attach
                heldObjectBody = currentInteractable.GetComponent<Rigidbody>();
                fixedJoint.connectedBody = heldObjectBody;


                // Set active hand
                currentInteractable.activeHand = this;
        }

        public void Drop()
        {
         if (!currentInteractable)
                return;
                // Detach
                fixedJoint.connectedBody = null;

                // Restore original color on drop
                for (int i = 0; i < currentInteractable.meshRenderers.Length; i++)
                {
                    currentInteractable.meshRenderers[i].material.color = currentInteractable.originalColors[i];
                }

                // Clear
                currentInteractable.activeHand = null;
                currentInteractable = null;
        }

        private Interactable GetNearestInteractable()
        {
            Interactable nearest = null;
            float minDistance = float.MaxValue;
            float distance = 0.0f;

            foreach (Interactable interactable in contactedInteractables)
            {
                if (interactable)
                {
                    distance = (interactable.transform.position - transform.position).sqrMagnitude;

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = interactable;
                    }
                }
            }
            return nearest;
        }
    }
}
