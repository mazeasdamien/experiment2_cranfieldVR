using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using Image = Microsoft.Azure.Kinect.Sensor.Image;

public class KinectDevice
{
    public Device device;
    public Texture2D texture;
    public Color32[] colorArray;

    public KinectDevice(Device device, Texture2D texture, Color32[] colorArray)
    {
        this.device = device;
        this.texture = texture;
        this.colorArray = colorArray;
    }
}

public class KinectHandler : MonoBehaviour
{
    public RawImage[] rawImages;
    public string[] desiredSerialNumbers;

    private List<KinectDevice> kinectDevices;
    private Thread[] imageProcessingThreads;

    void Start()
    {
        kinectDevices = new List<KinectDevice>();
        imageProcessingThreads = new Thread[desiredSerialNumbers.Length];

        int deviceCount = Device.GetInstalledCount();

        foreach (string desiredSerialNumber in desiredSerialNumbers)
        {
            bool deviceFound = false;

            for (int i = 0; i < deviceCount; i++)
            {
                using (Device tempDevice = Device.Open(i))
                {
                    if (tempDevice.SerialNum == desiredSerialNumber)
                    {
                        deviceFound = true;

                        DeviceConfiguration config = new DeviceConfiguration
                        {
                            ColorFormat = ImageFormat.ColorBGRA32,
                            ColorResolution = ColorResolution.R720p,
                            DepthMode = DepthMode.NFOV_2x2Binned,
                            SynchronizedImagesOnly = true,
                        };

                        tempDevice.StartCameras(config);

                        (int textureWidth, int textureHeight) = GetColorResolutionDimensions(config.ColorResolution);
                        Texture2D kinectTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.BGRA32, false);
                        Color32[] colorArray = new Color32[textureWidth * textureHeight];

                        KinectDevice kinectDevice = new KinectDevice(tempDevice, kinectTexture, colorArray);
                        kinectDevices.Add(kinectDevice);

                        break;
                    }
                }
            }

            if (!deviceFound)
            {
                Debug.LogError($"Device with serial number {desiredSerialNumber} not found.");
            }
        }

        for (int i = 0; i < rawImages.Length; i++)
        {
            rawImages[i].texture = kinectDevices[i].texture;
        }
    }

    private (int, int) GetColorResolutionDimensions(ColorResolution resolution)
    {
        switch (resolution)
        {
            case ColorResolution.R720p:
                return (1280, 720);
            case ColorResolution.R1080p:
                return (1920, 1080);
            case ColorResolution.R1440p:
                return (2560, 1440);
            case ColorResolution.R1536p:
                return (2048, 1536);
            case ColorResolution.R2160p:
                return (3840, 2160);
            case ColorResolution.R3072p:
                return (4096, 3072);
            default:
                Debug.LogError("Unsupported color resolution.");
                return (0, 0);
        }
    }

    void Update()
    {/*
        for (int i = 0; i < kinectDevices.Count; i++)
        {
            if (kinectDevices[i].device != null && !kinectDevices[i].device.IsDisposed &&
                (imageProcessingThreads[i] == null || !imageProcessingThreads[i].IsAlive))
            {
                Capture capture = kinectDevices[i].device.GetCapture();
                if (capture != null)
                {
                    capture.Reference();
                    imageProcessingThreads[i] = new Thread(() => ProcessImage(capture, kinectDevices[i]));
                    imageProcessingThreads[i].Start();
                }
            }
        }*/
    }


    private void ProcessImage(Capture capture, KinectDevice kinectDevice)
    {
        using (capture)
        {
            Image colorImage = capture.Color;
            BGRA[] colorData = colorImage.GetPixels<BGRA>().ToArray();

            for (int i = 0; i < colorData.Length; i++)
            {
                kinectDevice.colorArray[i] = new Color32(colorData[i].R, colorData[i].G, colorData[i].B, colorData[i].A);
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                kinectDevice.texture.SetPixels32(kinectDevice.colorArray);
                kinectDevice.texture.Apply();
            });
        }
    }

    void OnDestroy()
    {
        foreach (var kinectDevice in kinectDevices)
        {
            kinectDevice.device.StopCameras();
            kinectDevice.device.Dispose();
        }
    }
}
