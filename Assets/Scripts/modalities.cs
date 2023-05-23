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
        Mix
    }

    [SerializeField] private ModalityType currentModality;

    public ModalityType CurrentModality
    {
        get { return currentModality; }
        private set { currentModality = value; }
    }

    private void Update()
    {
        SetModality(CurrentModality);
    }

    public void SetModality(ModalityType modality)
    {
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
            case ModalityType.Mix:
                useMarker = true;
                usePT = true;
                break;
        }

        // Update the current modality
        CurrentModality = modality;
    }
}
