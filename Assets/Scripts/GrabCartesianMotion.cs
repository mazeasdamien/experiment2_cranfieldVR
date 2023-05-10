using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using Leap.InteractionEngine.Examples;

namespace Telexistence
{
    public class GrabCartesianMotion : MonoBehaviour
    {
        [SerializeField] private float raycastLength = 10.0f;
        [SerializeField] private LayerMask floorLayer;
        private Vector3 raycastDirection;

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

            raycastDirection = transform.right * raycastLength;
        }

        private void OnDrawGizmos()
        {
            DrawRay();
        }

        private void DrawRay()
        {
            Gizmos.color = Color.red;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, raycastDirection, out hit, raycastLength, floorLayer))
            {
                Gizmos.DrawLine(transform.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.2f);
            }
            else
            {
                Gizmos.DrawRay(transform.position, raycastDirection);
            }
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

            raycastDirection = transform.right * raycastLength;
        }

        private IEnumerator DisableGraspingTemporarily()
        {
            interactionBehaviour.ignoreGrasping = true;
            canvas.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            canvas.SetActive(false);
            interactionBehaviour.ignoreGrasping = false;
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