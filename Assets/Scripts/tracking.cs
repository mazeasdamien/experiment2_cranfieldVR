using UnityEngine;
using Varjo.XR;
using TMPro;

public class tracking : MonoBehaviour
{
    public TMP_Text quality;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            VarjoEyeTracking.RequestGazeCalibration(VarjoEyeTracking.GazeCalibrationMode.Legacy);
        }

        quality.text = "left: " + VarjoEyeTracking.GetGazeCalibrationQuality().left + "  right: " + VarjoEyeTracking.GetGazeCalibrationQuality().right;

        VarjoEyeTracking.EyeMeasurements measurements =  VarjoEyeTracking.GetEyeMeasurements();
        VarjoEyeTracking.GazeData data =  VarjoEyeTracking.GetGaze();
        Debug.Log(measurements.leftEyeOpenness);
        Debug.Log(measurements.rightEyeOpenness);
    }
}