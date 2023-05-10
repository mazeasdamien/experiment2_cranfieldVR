using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;

public class videoKinect : MonoBehaviour
{
    public RawImage outputImage;
    private Texture2D texture2D;
    private Device kinect;

    private void Start()
    {
        texture2D = new Texture2D(1, 1);

        kinect = Device.Open(1);

        kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorMJPG,
            ColorResolution = ColorResolution.R1080p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });
    }

    private void Update()
    {
        using (Capture capture = kinect.GetCapture())
        {
            Microsoft.Azure.Kinect.Sensor.Image colorImage = capture.Color;

            texture2D.LoadImage(capture.Color.Memory.ToArray());
            texture2D.Apply();
            outputImage.texture = texture2D;
        }
    }

    private void OnDestroy()
    {
        kinect.StopCameras();
    }
}
