using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.VFX;
using Telexistence;

public class modalities : MonoBehaviour
{
    public GameObject feed2D;
    public bool usePT;
    public bool useMarker;
    public TextMeshProUGUI DistanceText;
    public Button btn_NEXT_TASK;
    public List<Button> shape = new List<Button>();
    public List<Button> color = new List<Button>();
    public List<GameObject> toHide = new List<GameObject>();
    public GameObject panel_instruction;
    public GameObject panel_questionnaire;
    public GameObject photopanel;
    public TLXQuestionnaire tlxQuestionnaire;
    public GameObject trial;
    public GameObject m1;
    public GameObject m2;
    public GameObject m3;
    public GameObject m4;

    public TextMeshProUGUI info;
    public VisualEffect pt;
    public LaserPointer laserPointer;

    public int par_ID;
    public int modalities_order;

    public Dictionary<string, float> taskTimes = new Dictionary<string, float>();
    public Dictionary<string, float> modalityTimes = new Dictionary<string, float>();

    private float taskStartTime;
    private float modalityStartTime;

    private float checkpointStartTime;
    private float totalStartTime;


    [System.Serializable]
    public class ParticipantData
    {
        public string participantID;
        public string orderID;
    }

    [System.Serializable]
    public class Participant
    {
        public ParticipantData participant;
    }

    public enum ModalityType
    {
        TRIAL,
        DD,
        DDD,
        AV,
        DDDDD
    }

    public enum ModalityModel
    {
        nothing,
        trial,
        model1,
        model2,
        model3,
        model4
    }

    public enum TaskType
    {
        start,
        t1,
        t2,
        t3,
        questions
    }

    [SerializeField] private ModalityType currentModality;
    [SerializeField] private ModalityModel currentModel;
    [SerializeField] private TaskType currentTask;
    public List<GameObject> task1Objects;
    public List<GameObject> task2Objects;
    public List<GameObject> task3Objects;

    private int currentModalityIndex;
    private int currentTaskIndex;
    private ExperimentFlow experimentFlow;

    public ModalityType CurrentModality
    {
        get { return currentModality; }
        set { currentModality = value; }
    }

    public ModalityModel CurrentModel
    {
        get { return currentModel; }
        set { currentModel = value; }
    }

    public TaskType CurrentTask
    {
        get { return currentTask; }
        set { currentTask = value; }
    }

    [Serializable]
    public class Task
    {
        public string taskName;
        public string distance;
        public string panelType;
        public bool displayButtons;
        public string mainButtonText;
        public string model;
    }

    [Serializable]
    public class Modality
    {
        public string name;
        public Task[] tasks;
    }

    [Serializable]
    public class ExperimentFlow
    {
        public Modality[] modalities;
    }

    public void Checkpoint()
    {
        float checkpointTime = Time.time - checkpointStartTime;
        taskTimes[$"checkpoint_{checkpointStartTime}"] = checkpointTime;
        checkpointStartTime = Time.time;
    }

    private void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Participant.json");
        string jsonString = File.ReadAllText(path);
        Participant data = JsonUtility.FromJson<Participant>(jsonString);

        string path1 = Path.Combine(Application.streamingAssetsPath, "distance.json");
        string jsonString1 = File.ReadAllText(path1);
        experimentFlow = JsonUtility.FromJson<ExperimentFlow>(jsonString1);

        par_ID = int.Parse(data.participant.participantID);
        modalities_order = int.Parse(data.participant.orderID);

        // Re-order modalities
        switch (modalities_order)
        {
            case 1:
                experimentFlow.modalities = new Modality[] {
                GetModalityByName("TRIAL"),
                GetModalityByName("DD"),
                GetModalityByName("DDD"),
                GetModalityByName("AV"),
                GetModalityByName("DDDDD"),
            };
                break;
            case 2:
                experimentFlow.modalities = new Modality[] {
                GetModalityByName("TRIAL"),
                GetModalityByName("DDD"),
                GetModalityByName("AV"),
                GetModalityByName("DDDDD"),
                GetModalityByName("DD"),
            };
                break;
            case 3:
                experimentFlow.modalities = new Modality[] {
                GetModalityByName("TRIAL"),
                GetModalityByName("AV"),
                GetModalityByName("DDDDD"),
                GetModalityByName("DD"),
                GetModalityByName("DDD"),
            };
                break;
            case 4:
                experimentFlow.modalities = new Modality[] {
                GetModalityByName("TRIAL"),
                GetModalityByName("DDDDD"),
                GetModalityByName("DD"),
                GetModalityByName("DDD"),
                GetModalityByName("AV"),
            };
                break;
        }

        currentModalityIndex = 0; // Start from the first modality
        currentTaskIndex = 0; // Start from the first task

        SetCurrentModalityAndTask();

        if (CurrentModality != ModalityType.TRIAL)
        {
            checkpointStartTime = Time.time;
            totalStartTime = Time.time;
        }
    }

    private void SaveDataToCSV(float taskTime, float modalityTime, float totalTime)
    {
        string folderPath = Path.Combine(Application.dataPath, $"Participants_data");
        Directory.CreateDirectory(folderPath);

        string fileName = $"participant_data.csv";
        string filePath = Path.Combine(folderPath, fileName);

        using (StreamWriter sw = new StreamWriter(filePath, true))  // note 'true' to append
        {
            // Write participant ID and modalities order
            sw.Write($"{par_ID},{modalities_order},");

            // For each modality, write shape/color pair
            foreach (Modality modality in experimentFlow.modalities)
            {
                if (modality.name != "TRIAL")
                {
                    string shapeColorPair = laserPointer.GetShapeColorPair();
                    sw.Write($"{shapeColorPair},");
                }
            }

            // Write responses from questionnaire
            List<string> answers = tlxQuestionnaire.GetAnswers();
            for (int i = 0; i < answers.Count; i++)
            {
                sw.Write($"{answers[i]}");
                if (i < answers.Count - 1)  // add comma except for last item
                {
                    sw.Write(",");
                }
            }

            // Write task time, modality time and total time
            sw.Write($"{taskTime},{modalityTime},{totalTime}");

            // Write a new line
            sw.WriteLine();
        }
    }

    private Modality GetModalityByName(string name)
    {
        foreach (Modality modality in experimentFlow.modalities)
        {
            if (modality.name.Equals(name))
            {
                return modality;
            }
        }

        throw new Exception("No modality found with the name " + name);
    }

    private void SetCurrentModalityAndTask()
    {
        // Get the current modality and task
        Modality currentModality = experimentFlow.modalities[currentModalityIndex];
        Task currentTask = currentModality.tasks[currentTaskIndex];

        // Record start times
        taskStartTime = Time.time;
        if (currentTaskIndex == 0)  // If it's the first task in a modality, record modality start time
        {
            modalityStartTime = Time.time;
        }

        // Set the modality and task
        Enum.TryParse(currentModality.name, out ModalityType modalityType);
        CurrentModality = modalityType;
        Enum.TryParse(currentTask.taskName, out TaskType taskType);
        CurrentTask = taskType;
        // Set the model
        Enum.TryParse(currentTask.model, out ModalityModel model);
        CurrentModel = model;

        info.text = currentModality.name + " " + currentTask.taskName;

        // Set param
        DistanceText.text = currentTask.distance;
        btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = currentTask.mainButtonText;

        // Set buttons display state
        foreach (var button in shape)
        {
            button.gameObject.SetActive(currentTask.displayButtons);
        }
        foreach (var button in color)
        {
            button.gameObject.SetActive(currentTask.displayButtons);
        }

        // Set panel display state
        switch (currentTask.panelType)
        {
            case "instruction":
                foreach (var g in toHide)
                {
                    g.SetActive(true);
                }
                panel_instruction.SetActive(true);
                panel_questionnaire.SetActive(false);
                break;
            case "questionnaire":
                foreach (var g in toHide)
                {
                    g.SetActive(false);
                }
                panel_instruction.SetActive(false);
                panel_questionnaire.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void NextTask()
    {
        // Record the time spent on the current task
        float taskEndTime = Time.time;
        float taskDuration = taskEndTime - taskStartTime;
        Task currentTask = experimentFlow.modalities[currentModalityIndex].tasks[currentTaskIndex];
        taskTimes[currentTask.taskName] = taskDuration;

        // If it's the t3 task, also record the modality time and reset modality start time
        if (currentTask.taskName == "t3")
        {
            float modalityEndTime = Time.time;
            float modalityDuration = modalityEndTime - modalityStartTime;
            Modality currentModality = experimentFlow.modalities[currentModalityIndex];
            modalityTimes[currentModality.name] = modalityDuration;

            modalityStartTime = Time.time; // Reset the modality start time
        }

        currentTaskIndex++;
        if (currentTaskIndex >= experimentFlow.modalities[currentModalityIndex].tasks.Length)
        {
            Modality currentModality = experimentFlow.modalities[currentModalityIndex];

            // Reset questionnaire when a modality ends, except for "TRIAL"
            if (currentModality.name != "TRIAL")
            {
                tlxQuestionnaire.ResetQuestionnaire();
            }
            currentTaskIndex = 0;

            // Now, after all the checks, we can increment currentModalityIndex safely.
            currentModalityIndex++;
        }

        if (currentModalityIndex >= experimentFlow.modalities.Length)
        {
            StartCoroutine(EndExperiment());
        }
        else
        {
            SetCurrentModalityAndTask();
        }

    }

    IEnumerator EndExperiment()
    {
        // Check if currentModalityIndex is within the bounds of the array
        if (currentModalityIndex < experimentFlow.modalities.Length)
        {
            // Record the time spent on the current modality
            float modalityEndTime = Time.time;
            float modalityDuration = modalityEndTime - modalityStartTime;
            Modality currentModality = experimentFlow.modalities[currentModalityIndex];
            modalityTimes[currentModality.name] = modalityDuration;
        }

        // Calculate total experiment time
        float totalExperimentTime = Time.time - totalStartTime;

        // Save to CSV
        SaveDataToCSV(0, 0, totalExperimentTime);

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Quit application
#if UNITY_EDITOR
        // If running in Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
// If running in build
Application.Quit();
#endif
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
        photopanel.SetActive(true);

        // Activate the objects for the specified task
        switch (task)
        {
            case TaskType.start:
                photopanel.SetActive(false);
                break;
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
            case TaskType.questions:
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
            case ModalityModel.trial:
                trial.SetActive(true);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.model1:
                trial.SetActive(false);
                m1.SetActive(true);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.model2:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(true);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
            case ModalityModel.model3:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(true);
                m4.SetActive(false);
                break;
            case ModalityModel.model4:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(true);
                break;
            case ModalityModel.nothing:
                trial.SetActive(false);
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                break;
        }

        // Update the current modality
        CurrentModel = model;
    }

    public void SetModality(ModalityType modality)
    {
        // Activate the specified modality
        switch (modality)
        {
            case ModalityType.TRIAL:
                feed2D.SetActive(true);
                useMarker = false;
                usePT = true;
                break;
            case ModalityType.DD:
                feed2D.SetActive(true);
                useMarker = false;
                usePT = false;
                break;
            case ModalityType.DDD:
                feed2D.SetActive(false);
                useMarker = false;
                usePT = true;
                break;
            case ModalityType.AV:
                feed2D.SetActive(false);
                useMarker = true;
                usePT = false;
                break;
            case ModalityType.DDDDD:
                feed2D.SetActive(true);
                useMarker = false;
                usePT = true;
                break;
        }

        // Update the current modality
        CurrentModality = modality;
    }
}
