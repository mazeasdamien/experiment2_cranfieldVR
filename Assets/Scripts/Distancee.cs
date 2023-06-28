using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;

public class Distancee : MonoBehaviour
{
    public modalities Modalities;
    public TextMeshProUGUI DistanceText;

    private List<Modality> modalities;

    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "distance.json");
        string jsonString = File.ReadAllText(filePath);

        modalities = JsonConvert.DeserializeObject<Root>(jsonString).modalities;
    }

    private void Update()
    {
        string currentModality = Modalities.CurrentModel.ToString();
        string currentTask = Modalities.CurrentTask.ToString();

        foreach (var modality in modalities)
        {
            if (modality.name == currentModality)
            {
                foreach (var task in modality.tasks)
                {
                    if (task.taskName == currentTask)
                    {
                        DistanceText.text = task.distance;
                        return;
                    }
                }
            }
        }

        DistanceText.text = "No distance found";
    }
}

public class Task
{
    public string taskName;
    public string distance;
}

public class Modality
{
    public string name;
    public List<Task> tasks;
}

public class Root
{
    public List<Modality> modalities;
}