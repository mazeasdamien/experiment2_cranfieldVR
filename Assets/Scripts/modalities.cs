using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class modalities : MonoBehaviour
{
    public GameObject feed2D;
    public bool usePT;
    public bool useMarker;

    public enum ModalityType
    {
        Feed2D,
        PointCloud,
        Markers,
        PointCloudFeed2D
    }

    [SerializeField] private ModalityType currentModality;

    public ModalityType CurrentModality
    {
        get { return currentModality; }
        private set { currentModality = value; }
    }

    public void SetModality(ModalityType modality)
    {
        // Deactivate all modalities initially
        feed2D.SetActive(false);
        usePT = false;
        useMarker = false;

        // Activate the specified modality
        switch (modality)
        {
            case ModalityType.Feed2D:
                feed2D.SetActive(true);
                break;
            case ModalityType.PointCloud:
                usePT = true;
                break;
            case ModalityType.Markers:
                useMarker = true;
                break;
            case ModalityType.PointCloudFeed2D:
                feed2D.SetActive(true);
                usePT = true;
                break;
            default:
                Debug.LogWarning("Unknown modality: " + modality.ToString());
                break;
        }

        // Update the current modality
        CurrentModality = modality;
    }
}
