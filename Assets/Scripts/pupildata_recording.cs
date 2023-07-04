using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Varjo.XR.VarjoEyeTracking;

public class pupildata_recording : MonoBehaviour
{
    string filePath;
    double recordingStartTime = -1;
    public modalities mm;

    public bool isRecording = false;
    public bool isFilterSet;
    public bool isFrequencySet;

    public double elapsedTime;

    void Start()
    {
        isFilterSet = SetGazeOutputFilterType(GazeOutputFilterType.Standard);
        isFrequencySet = SetGazeOutputFrequency(GazeOutputFrequency.Frequency100Hz);
    }

    public void CreateCSV()
    {
        filePath = Path.Combine(Application.dataPath, $"eye_tracking_data_{mm.par_ID}_{mm.CurrentModality}.csv");

        // Write the CSV header
        using (StreamWriter writer = new StreamWriter(filePath, append: false))
        {
            writer.WriteLine("Time,LeftPupilDiameter,RightPupilDiameter");
        }

    }

    private void Update()
    {
        if (isRecording)
        {
            RecordData();
        }
        else 
        {
            StopRecording();
        }
    }

    void StopRecording()
    {
        ResetRecording();
    }

    void ResetRecording()
    {
        recordingStartTime = -1;
    }

    void RecordData()
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
                    elapsedTime = (gazeDataList[i].captureTime / 1e6) - recordingStartTime;  // Convert Varjo timestamp from nanoseconds to milliseconds
                    Debug.Log(elapsedTime);
                    using (StreamWriter writer = new StreamWriter(filePath, append: true))
                    {
                        // Write the sample in the CSV file
                        writer.WriteLine($"{elapsedTime},{leftPupilDiameter},{rightPupilDiameter}");
                    }
                }
            }
        }
    }
}
