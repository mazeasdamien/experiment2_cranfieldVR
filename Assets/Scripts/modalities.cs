using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class modalities : MonoBehaviour
{
    public GameObject feed2D;
    public bool usePT;
    public bool useMarker;

    public GameObject trial;
    public GameObject m1;
    public GameObject m2;
    public GameObject m3;
    public GameObject m4;

    public enum ModalityType
    {
        Feed2D,
        PointCloud,
        Markers,
        Mix
    }

    public enum ModalityModel
    {
        TRIAL,
        M1,
        M2,
        M3,
        M4
    }

    public enum TaskType
    {
        t1,
        t2,
        t3
    }

    [SerializeField] private ModalityType currentModality;
    [SerializeField] private ModalityModel currentModel;
    [SerializeField] private TaskType currentTask;
    public List<GameObject> task1Objects;
    public List<GameObject> task2Objects;
    public List<GameObject> task3Objects;

    public ModalityType CurrentModality
    {
        get { return currentModality; }
        private set { currentModality = value; }
    }

    public ModalityModel CurrentModel
    {
        get { return currentModel; }
        private set { currentModel = value; }
    }

    public TaskType CurrentTask
    {
        get { return currentTask; }
        private set { currentTask = value; }
    }

    private void Update()
    {
        SetModality(CurrentModality);
        SetModel(CurrentModel);
        SetTask(CurrentTask);
    }

    public void SetTask(TaskType task)
    {
        // Deactivate all objects
        foreach (var obj in task1Objects)
            obj.SetActive(false);
        foreach (var obj in task2Objects)
            obj.SetActive(false);
        foreach (var obj in task3Objects)
            obj.SetActive(false);

        // Activate the objects for the specified task
        switch (task)
        {
            case TaskType.t1:
                foreach (var obj in task1Objects)
                    obj.SetActive(true);
                break;
            case TaskType.t2:
                foreach (var obj in task2Objects)
                    obj.SetActive(true);
                break;
            case TaskType.t3:
                foreach (var obj in task3Objects)
                    obj.SetActive(true);
                break;
        }

        // Update the current task
        CurrentTask = task;
    }

    public void SetModel(ModalityModel model)
    {

        // Activate the specified modality
        switch (model)
        {
            case ModalityModel.TRIAL:
                trial.SetActive(true);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.M1:
                trial.SetActive(false);
                m1.SetActive(true);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.M2:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(true);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.M3:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(true);
                m4.SetActive(false);
                break;
            case ModalityModel.M4:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(true);
                break;
        }

        // Update the current modality
        CurrentModel = model;
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
