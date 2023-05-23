using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GazeTimeDisplay : MonoBehaviour
{
    // Reference to the TextMeshProUGUI component
    public TextMeshProUGUI gazeTimeText;

    // Dictionary to store gaze time for each tagged object
    private Dictionary<string, float> gazeTimes = new Dictionary<string, float>() {
    {"Robot", 0f},
    {"Tablet", 0f},
    {"InstructionPanel", 0f},
    {"3DScene", 0f}
    };

    void Update()
    {
        // Presume that the GazeTracking function updates gazeTimes Dictionary
        GazeTracking();

        // Update the text display
        string displayText = "";
        foreach (var item in gazeTimes)
        {
            displayText += item.Key + ": " + item.Value.ToString("F2") + " seconds\n";
        }
        gazeTimeText.text = displayText;
    }

    void GazeTracking()
    {
        // Your gaze tracking logic here.
        // For the purpose of the example, we're simulating gaze on different objects

        // Add some random time to gazeTimes just for demonstration purposes
        gazeTimes["Robot"] += Random.Range(0.01f, 0.03f);
        gazeTimes["Tablet"] += Random.Range(0.01f, 0.03f);
        gazeTimes["InstructionPanel"] += Random.Range(0.01f, 0.03f);
        gazeTimes["3DScene"] += Random.Range(0.01f, 0.03f);
    }
}
