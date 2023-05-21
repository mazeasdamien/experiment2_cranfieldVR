using System.Collections.Generic;
using UnityEngine;

namespace VarjoExample
{
    [RequireComponent(typeof(Rigidbody))]
    public class Interactable : MonoBehaviour
    {
        [HideInInspector]
        public Hand activeHand;
        public List<Color> originalColors = new List<Color>();
        public MeshRenderer[] meshRenderers;
        void Start()
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer.tag != "UI")
                {
                    originalColors.Add(meshRenderer.material.color);
                }
            }
        }
    }
}