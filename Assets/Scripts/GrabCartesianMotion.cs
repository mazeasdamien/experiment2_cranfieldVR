using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

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

    private LockRotation lockRotation;

    private Quaternion rotationBeforeGrab;

    public GameObject transformObj;

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
    }

    private void OnGraspBegin()
    {
        transformObj.SetActive(false);
        rotationBeforeGrab = transform.rotation;
        lockRotation.initialRotation = rotationBeforeGrab;
        interactionBehaviour.ignoreContact = false;
        rigidBody.constraints = RigidbodyConstraints.None;
        lockRotation.lockRotation = true;


        audioSource.PlayOneShot(grabStartSound);
        otherGameObjectRenderer.material.color = grabColor;
    }

    private void OnGraspEnd()
    {
        transformObj.SetActive(true);
        interactionBehaviour.ignoreContact = true;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        lockRotation.lockRotation = false;
        transform.rotation = lockRotation.initialRotation;
        audioSource.PlayOneShot(grabEndSound);
        otherGameObjectRenderer.material.color = initialColor;
        transform.rotation = rotationBeforeGrab;
    }

    private void OnHoverBegin()
    {
        if (!interactionBehaviour.isGrasped)
        {
            otherGameObjectRenderer.material.color = hoverColor;
        }
    }

    private void OnHoverEnd()
    {
        if (!interactionBehaviour.isGrasped)
        {
            otherGameObjectRenderer.material.color = initialColor;
        }
    }

    private void OnDestroy()
    {
        interactionBehaviour.OnGraspBegin -= OnGraspBegin;
        interactionBehaviour.OnGraspEnd -= OnGraspEnd;
        interactionBehaviour.OnHoverBegin -= OnHoverBegin;
        interactionBehaviour.OnHoverEnd -= OnHoverEnd;
    }
}
