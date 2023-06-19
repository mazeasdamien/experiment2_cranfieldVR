using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenCvSharp.Aruco;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Buffers.Text;
using System.Runtime.InteropServices.ComTypes;
using System;

namespace Telexistence
{
    public class videoKinectRectangle : MonoBehaviour
    {
        public RawImage outputImage;
        private Device kinect;
        private VideoCapture cap_opencv;
        public float captureFrameRate = 24f;
        private BGRA[] colorData;
        private Calibration calibration;
        private Mat bgrMat;
        private Texture2D texture;
        private GameObject kinectGameObject;
        public int MIN_AREA = 500; // Adjust this value to your needs


        private void Start()
        {
            // Initialize video capture
            cap_opencv = new VideoCapture();
            cap_opencv.Open(1, VideoCaptureAPIs.ANY);

            kinect = Device.Open(1);

            kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30,
            });
            calibration = kinect.GetCalibration();

            // Initialize bgrMat
            bgrMat = new Mat();
        }


        private void Update()
{
    using (Capture capture = kinect.GetCapture())
    {
        Microsoft.Azure.Kinect.Sensor.Image colorImage = capture.Color;

        if (colorImage != null && colorImage.WidthPixels > 0 && colorImage.HeightPixels > 0)
        {
            // Create the texture if it's not already created
            if (texture == null)
            {
                texture = new Texture2D(colorImage.WidthPixels, colorImage.HeightPixels, TextureFormat.RGB24, false);
            }

            // Update colorData with the latest pixel data
            colorData = colorImage.GetPixels<BGRA>().ToArray();

            // Update colorMat with the latest colorData
            GCHandle pinnedArray = GCHandle.Alloc(colorData, GCHandleType.Pinned);
            IntPtr colorDataPtr = pinnedArray.AddrOfPinnedObject();
                    using (Mat colourMat = new Mat(colorImage.HeightPixels, colorImage.WidthPixels, MatType.CV_8UC4, colorDataPtr))
                    {
                        // Convert BGRA to BGR format
                        Cv2.CvtColor(colourMat, bgrMat, ColorConversionCodes.BGRA2BGR);

                        // Apply the texture to the outputImage RawImage
                        UpdateTexture(bgrMat, texture);
                        outputImage.texture = texture;

                        // Cleanup
                        pinnedArray.Free();

                        // Apply threshold to find the contours
                        Mat grayImage = new Mat();
                        Mat cannyEdges = new Mat();

                        Cv2.CvtColor(bgrMat, grayImage, ColorConversionCodes.BGR2GRAY);
                        Cv2.Blur(grayImage, grayImage, new Size(3, 3));
                        Cv2.Canny(grayImage, cannyEdges, 50, 150);

                        Point[][] contours;
                        HierarchyIndex[] hierarchyIndices;
                        Cv2.FindContours(cannyEdges, out contours, out hierarchyIndices, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                        Point[][] contoursPoly = new Point[contours.Length][];

                        for (int i = 0; i < contours.Length; i++)
                        {
                            Point[] poly = Cv2.ApproxPolyDP(contours[i], 3, true);
                            contoursPoly[i] = poly;
                        }

                        for (int i = 0; i < contours.Length; i++)
                        {
                            if (contoursPoly[i].Length == 4)
                            {
                                OpenCvSharp.Rect rect = Cv2.BoundingRect(contoursPoly[i]);

                                // Ignore rectangles smaller than a certain area.
                                if (rect.Width * rect.Height > MIN_AREA)
                                {
                                    Cv2.Rectangle(bgrMat, rect, Scalar.Green, 2);
                                }
                            }
                        }
                        Cv2.CvtColor(bgrMat, bgrMat, ColorConversionCodes.BGR2RGB);

                        // Convert to Texture2D and apply it to the outputImage
                        UpdateTexture(bgrMat, texture);
                        outputImage.texture = texture;
                    }
        }
    }
}


        private void UpdateTexture(Mat mat, Texture2D texture)
        {
            IntPtr data = mat.Data;
            byte[] rawPixelData = new byte[mat.Width * mat.Height * mat.ElemSize()];
            Marshal.Copy(data, rawPixelData, 0, rawPixelData.Length);
            texture.LoadRawTextureData(rawPixelData);
            texture.Apply();
        }

        private void OnDestroy()
        {
            if (kinect != null)
            {
                kinect.StopCameras();
            }
        }
    }
}