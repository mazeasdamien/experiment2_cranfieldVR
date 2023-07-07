using System.Collections;
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
    public AudioClip startRecordingSound;
    public AudioClip endRecordingSound;
    private AudioSource audioSource;
    private bool isF9Recording = false;
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = endRecordingSound;
        isFilterSet = SetGazeOutputFilterType(GazeOutputFilterType.Standard);
        isFrequencySet = SetGazeOutputFrequency(GazeOutputFrequency.Frequency100Hz);
    }

    public void CreateCSV()
    {
        if (mm.CurrentModality != modalities.ModalityType.TRIAL)
        {
            filePath = Path.Combine(Application.dataPath, "Participants_data", $"participant_{mm.par_ID}", $"eye_tracking_data_{mm.par_ID}_{mm.CurrentModality}.csv");

            // Write the CSV header
            using (StreamWriter writer = new StreamWriter(filePath, append: false))
            {
                writer.WriteLine("Time,LeftPupilDiameter,RightPupilDiameter");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F9) && !isF9Recording)
        {
            StartCoroutine(RecordForDuration(60.0f)); // 60 seconds
        }
        if (isRecording)
        {
            RecordData();
        }
        else
        {
            StopRecording();
        }
    }

    IEnumerator RecordForDuration(float duration)
    {
        isF9Recording = true;
        isRecording = true;
        filePath = Path.Combine(Application.dataPath, "Participants_data", $"participant_{mm.par_ID}", $"BASELESS_{mm.par_ID}.csv");
        CreateCSV();

        audioSource.clip = startRecordingSound;
        audioSource.Play();

        yield return new WaitForSeconds(duration);

        isRecording = false;
        isF9Recording = false;

        audioSource.clip = endRecordingSound;
        audioSource.Play();
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

                if (leftPupilDiameter >= 1.2f && rightPupilDiameter >= 1.2f)
                {
                    elapsedTime = (gazeDataList[i].captureTime / 1e6) - recordingStartTime;

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