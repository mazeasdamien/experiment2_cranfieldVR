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
        [SerializeField] private GameObject canvas;
        [SerializeField] private Camera mainCamera;
        private Coroutine disableGraspingCoroutine;

        [SerializeField] private float maxMovementSpeed = 5.0f;
        [SerializeField] private float maxRotationSpeed = 300.0f;
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private void Start()
        {
            interactionBehaviour = GetComponent<InteractionBehaviour>();
            rigidBody = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();

            initialColor = otherGameObjectRenderer.material.color;

            interactionBehaviour.OnGraspBegin += OnGraspBegin;
            interactionBehaviour.OnGraspEnd += OnGraspEnd;
            interactionBehaviour.OnHoverBegin += OnHoverBegin;
            interactionBehaviour.OnHoverEnd += OnHoverEnd;
            interactionBehaviour.ignoreContact = true;

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        private void Update()
        {
            if (interactionBehaviour.isGrasped)
            {
                float movementSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
                float rotationSpeed = Quaternion.Angle(transform.rotation, lastRotation) / Time.deltaTime;

                if (movementSpeed > maxMovementSpeed || rotationSpeed > maxRotationSpeed)
                {
                    interactionBehaviour.ReleaseFromGrasp();
                    if (disableGraspingCoroutine != null)
                    {
                        StopCoroutine(disableGraspingCoroutine);
                    }
                    disableGraspingCoroutine = StartCoroutine(DisableGraspingTemporarily());
                }
            }

            lastPosition = transform.position;
            lastRotation = transform.rotation;

            UpdateColor();
        }

        private IEnumerator DisableGraspingTemporarily()
        {
            interactionBehaviour.ignoreGrasping = true;
            FaceCanvasToCamera();
            canvas.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            canvas.SetActive(false);
            interactionBehaviour.ignoreGrasping = false;
        }

        private void FaceCanvasToCamera()
        {
            if (mainCamera != null && canvas != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                cameraForward.y = 0; // Remove the vertical component to avoid undesired tilting of the canvas
                Vector3 targetDirection = canvas.transform.position - mainCamera.transform.position;
                targetDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                targetRotation *= Quaternion.Euler(0, 90, 0); // Apply the 90-degree offset
                canvas.transform.rotation = targetRotation;
            }
        }

        private void UpdateColor()
        {
            if (interactionBehaviour.isGrasped)
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
            interactionBehaviour.ignoreContact = false;
            rigidBody.constraints = RigidbodyConstraints.None;
            audioSource.PlayOneShot(grabStartSound);
            UpdateColor();
        }

        private void OnGraspEnd()
        {
            interactionBehaviour.ignoreContact = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            audioSource.PlayOneShot(grabEndSound);
            UpdateColor();
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