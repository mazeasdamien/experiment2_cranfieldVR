using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using Leap.InteractionEngine.Examples;

namespace Telexistence
{
    public class GrabCartesianMotion : MonoBehaviour
    {
        [SerializeField] private AudioClip grabStartSound;
        [SerializeField] private AudioClip grabEndSound;
        private AudioSource audioSource;

        [SerializeField] private Renderer otherGameObjectRenderer;
        private Color initialColor;
        [SerializeField] private Color hoverColor;
        [SerializeField] private Color grabColor;

        private InteractionBehaviour interactionBehaviour;
        private Rigidbody rigidBody;
        public FanucHandler fanucHandler;

        private LockRotation lockRotation;

        private Quaternion rotationBeforeGrab;

        public GameObject transformObj;

        public TransformTool transformTool;

        private void Start()
        {
            interactionBehaviour = GetComponent<InteractionBehaviour>();
            rigidBody = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();

            lockRotation = GetComponent<LockRotation>();

            initialColor = otherGameObjectRenderer.material.color;

            interactionBehaviour.OnGraspBegin += OnGraspBegin;
            interactionBehaviour.OnGraspEnd += OnGraspEnd;
            interactionBehaviour.OnHoverBegin += OnHoverBegin;
            interactionBehaviour.OnHoverEnd += OnHoverEnd;
            interactionBehaviour.ignoreContact = true;

            UpdateColor();
        }


        private void Update()
        {
            if (interactionBehaviour.isGrasped || transformTool.IsHandleBeingManipulated)
            {
                UpdateColor();
            }
        }

        private void UpdateColor()
        {
            if (fanucHandler.messageReachability == false)
            {
                otherGameObjectRenderer.material.color = Color.red;
            }
            else if (interactionBehaviour.isGrasped)
            {
                otherGameObjectRenderer.material.color = grabColor;
            }
            else if (interactionBehaviour.isHovered)
            {
                otherGameObjectRenderer.material.color = hoverColor;
            }
            else
            {
                otherGameObjectRenderer.material.color = initialColor;
            }
        }

        private void OnGraspBegin()
        {
            transformObj.SetActive(false);
            rotationBeforeGrab = transform.rotation;
            lockRotation.initialRotation = rotationBeforeGrab;
            interactionBehaviour.ignoreContact = false;
            rigidBody.constraints = RigidbodyConstraints.None;
            lockRotation.lockRotation = true;
            UpdateColor();
            audioSource.PlayOneShot(grabStartSound);
        }

        private void OnGraspEnd()
        {
            transformObj.SetActive(true);
            interactionBehaviour.ignoreContact = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            lockRotation.lockRotation = false;
            transform.rotation = lockRotation.initialRotation;
            audioSource.PlayOneShot(grabEndSound);
            UpdateColor();
            transform.rotation = rotationBeforeGrab;
        }

        private void OnHoverBegin()
        {
            UpdateColor();
        }

        private void OnHoverEnd()
        {
            UpdateColor();
        }

        private void OnDestroy()
        {
            interactionBehaviour.OnGraspBegin -= OnGraspBegin;
            interactionBehaviour.OnGraspEnd -= OnGraspEnd;
            interactionBehaviour.OnHoverBegin -= OnHoverBegin;
            interactionBehaviour.OnHoverEnd -= OnHoverEnd;
        }
    }
}