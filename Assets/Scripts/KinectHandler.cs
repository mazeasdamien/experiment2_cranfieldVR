using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using System.Runtime.InteropServices;

public class KinectHandler : MonoBehaviour
{
    public RawImage[] rawImages;
    public int width = 1280;
    public int height = 720;

    private Device[] _devices;
    private Transformation[] _transformations;
    private Texture2D[] _colorTextures;

    void Start()
    {
        InitializeAzureKinectSensors();
    }

    void Update()
    {
        UpdateColorFrames();
    }

    private void InitializeAzureKinectSensors()
    {
        int deviceCount = Device.GetInstalledCount();

        _devices = new Device[deviceCount];
        _transformations = new Transformation[deviceCount];
        _colorTextures = new Texture2D[deviceCount];

        for (int i = 0; i < deviceCount; i++)
        {
            _devices[i] = Device.Open(i);
            _devices[i].StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
            });

            _transformations[i] = _devices[i].GetCalibration().CreateTransformation();
            _colorTextures[i] = new Texture2D(width, height, TextureFormat.RGBA32, false);
            rawImages[i].texture = _colorTextures[i];
        }
    }

    private void UpdateColorFrames()
    {
        for (int i = 0; i < _devices.Length; i++)
        {
            using (Capture capture = _devices[i].GetCapture())
            {
                Microsoft.Azure.Kinect.Sensor.Image colorImage = capture.Color;
                if (colorImage != null)
                {
                    CopyColorDataToTexture(colorImage, _colorTextures[i]);
                    _colorTextures[i].Apply();
                }
            }
        }
    }

    private void CopyColorDataToTexture(Microsoft.Azure.Kinect.Sensor.Image colorImage, Texture2D targetTexture)
    {
        byte[] data = colorImage.Memory.ToArray();
        Color32[] colorData = new Color32[data.Length / 4];
        for (int i = 0; i < colorData.Length; i++)
        {
            int index = i * 4;
            colorData[i] = new Color32(data[index + 2], data[index + 1], data[index], data[index + 3]);
        }

        targetTexture.SetPixels32(colorData);
    }



    void OnDestroy()
    {
        for (int i = 0; i < _devices.Length; i++)
        {
            if (_devices[i] != null)
            {
                _devices[i].StopCameras();
                _devices[i].Dispose();
                _devices[i] = null;
            }
        }
    }
}
