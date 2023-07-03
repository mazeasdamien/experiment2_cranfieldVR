using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Varjo.XR.VarjoEyeTracking;

public class pupildata_recording : MonoBehaviour
{
    string filePath;
    bool isRecording;
    double recordingStartTime = -1;

    public bool isFilterSet;
    public bool isFrequencySet;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "eye_tracking_data.csv");

        // Write the CSV header
        using (StreamWriter writer = new StreamWriter(filePath, append: false))
        {
            writer.WriteLine("Time,LeftPupilDiameter,RightPupilDiameter");
        }

        isFilterSet = SetGazeOutputFilterType(GazeOutputFilterType.Standard);
        isFrequencySet = SetGazeOutputFrequency(GazeOutputFrequency.Frequency100Hz);
    }

    void Update()
    {
        // Toggle recording when the "Page Down" key is pressed
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            isRecording = !isRecording;
        }

        // Only record data if isRecording is true
        if (isRecording)
        {
            // Use the method to get the most recent gaze data and eye measurements
            List<GazeData> gazeDataList = new List<GazeData>();
            List<EyeMeasurements> eyeMeasurementsList = new List<EyeMeasurements>();
            GetGazeList(out gazeDataList, out eyeMeasurementsList);

            for (int i = 0; i < gazeDataList.Count; i++)
            {
                // If recording just started, capture the start time
                if (recordingStartTime < 0)
                {
                    recordingStartTime = gazeDataList[i].captureTime / 1e6;  // Convert Varjo timestamp from nanoseconds to milliseconds
                }

                // Retrieve eye status
                int leftStatus = (int)gazeDataList[i].leftStatus;
                int rightStatus = (int)gazeDataList[i].rightStatus;

                // Save only the valid data samples
                if ((leftStatus == 2 || leftStatus == 3) && (rightStatus == 2 || rightStatus == 3))
                {
                    // Retrieve pupil diameters from EyeMeasurements
                    float leftPupilDiameter = eyeMeasurementsList[i].leftPupilDiameterInMM;
                    float rightPupilDiameter = eyeMeasurementsList[i].rightPupilDiameterInMM;

                    if ((leftPupilDiameter != 0) && (rightPupilDiameter != 0))
                    {
                        double elapsedTime = (gazeDataList[i].captureTime / 1e6) - recordingStartTime;  // Convert Varjo timestamp from nanoseconds to milliseconds
                        int seconds = (int)(elapsedTime / 1000);
                        int milliseconds = (int)(elapsedTime % 1000);
                        double timestamp = seconds + (milliseconds / 1000.0);  // Combine seconds and milliseconds
                        using (StreamWriter writer = new StreamWriter(filePath, append: true))
                        {
                            // Write the sample in the CSV file
                            writer.WriteLine($"{timestamp:F3},{leftPupilDiameter},{rightPupilDiameter}");
                        }
                    }
                }
            }
        }
        else
        {
            // Reset the recording start time when recording is stopped
            recordingStartTime = -1;
        }
    }
}
