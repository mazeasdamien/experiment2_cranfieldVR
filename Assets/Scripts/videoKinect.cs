using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp.Aruco;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Telexistence
{
    public class videoKinect : MonoBehaviour
    {
        public RawImage outputImage;
        private Device kinect;
        private VideoCapture cap_opencv;
        private Dictionary arucoDictionary;
        private DetectorParameters detectorParameters;
        private Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        private Mat distCoeffs = new Mat(1, 8, MatType.CV_32FC1);
        public float markerLength = 0.06f;
        public float captureFrameRate = 24f;
        private BGRA[] colorData;
        private Calibration calibration;
        public Mat bgrMat;
        private Texture2D texture;
        public GameObject kinectGameObject;
        public FanucHandler fanucHandler;
        private GameObject instantiatedPrefabL = null;
        public modalities modalities;
        public LaserPointer laser;
        public Vector3 markerpos;
        public GameObject cubePrefab;
        private Dictionary<int, GameObject> markerDict = new Dictionary<int, GameObject>();

        private void Start()
        {
            // Initialize video capture
            cap_opencv = new VideoCapture();
            cap_opencv.Open(1, VideoCaptureAPIs.ANY);

            // Initialize ArUco marker detection
            arucoDictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
            detectorParameters = new DetectorParameters();
            detectorParameters.CornerRefinementMethod = CornerRefineMethod.Subpix;
            detectorParameters.CornerRefinementWinSize = 9;
            int colorWidth = calibration.ColorCameraCalibration.ResolutionWidth;
            int colorHeight = calibration.ColorCameraCalibration.ResolutionHeight;
            colorData = new BGRA[colorWidth * colorHeight];
            bgrMat = new Mat(colorHeight, colorWidth, MatType.CV_8UC3);

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
            InitCameraMatrixAndDistCoeffs(calibration, cameraMatrix, distCoeffs);
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
                        texture = new Texture2D(200,200, TextureFormat.RGB24, false);
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
                        UpdateTexture(bgrMat, texture);
                        // Apply the texture to the outputImage RawImage
                        outputImage.texture = texture;
                        // Cleanup
                        pinnedArray.Free();



                        CvAruco.DetectMarkers(bgrMat, arucoDictionary, out var corners, out var ids, detectorParameters, out var rejectedPoints);
                        CvAruco.DrawDetectedMarkers(bgrMat, corners, ids, Scalar.Green);

                        if (ids.Length > 0)
                        {
                            using (Mat rvecsMat = new Mat())
                            using (Mat tvecsMat = new Mat())
                            {
                                CvAruco.EstimatePoseSingleMarkers(corners, markerLength, cameraMatrix, distCoeffs, rvecsMat, tvecsMat);

                                for (int i = 0; i < ids.Length; i++)
                                {
                                    int id = ids[i];
                                    Vec3d rvec = rvecsMat.Get<Vec3d>(i);
                                    Vec3d tvec = tvecsMat.Get<Vec3d>(i);

                                    DrawAxis(bgrMat, rvec, tvec, markerLength / 2, cameraMatrix, distCoeffs);  // draw axis on the detected marker

                                    Vector3 markerPositionInKinectSpace = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); // If Y axis needs to be flipped
                                    Matrix4x4 kinectToWorld = kinectGameObject.transform.localToWorldMatrix;
                                    Vector3 markerPositionInWorldSpace = kinectToWorld.MultiplyPoint3x4(markerPositionInKinectSpace);

                                    // Store the position of marker in markerpos
                                    markerpos = markerPositionInWorldSpace;

                                    if (cubePrefab != null)
                                    {
                                        if (!markerDict.ContainsKey(id))
                                        {
                                            GameObject cube = Instantiate(cubePrefab, markerpos, Quaternion.identity);
                                            cube.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f); // Set the scale to 4cm x 4cm x 4cm
                                            markerDict[id] = cube;
                                        }
                                        else
                                        {
                                            markerDict[id].transform.position = markerpos;
                                        }
                                    }
                                }
                            }
                        }

                            // Convert the OpenCV Mat with the drawn axis to Texture2D and assign it to the RawImage
                            UpdateTexture(bgrMat, texture);
                        outputImage.texture = texture;
                    }
                }
            }
        }

        public static void DrawAxis(Mat image, Vec3d rvec, Vec3d tvec, float length, Mat cameraMatrix, Mat distCoeffs)
        {
            float[,] axisPoints = new float[,] {
    {0, 0, 0},
    {length, 0, 0},
    {0, length, 0},
    {0, 0, length}
    };
            Mat objectPoints = new Mat(4, 3, MatType.CV_32FC1, axisPoints);
            Mat imagePoints = new Mat();
            Cv2.ProjectPoints(objectPoints, rvec, tvec, cameraMatrix, distCoeffs, imagePoints);

            Cv2.Line(image, new Point((int)imagePoints.Get<Point2f>(0).X, (int)imagePoints.Get<Point2f>(0).Y), new Point((int)imagePoints.Get<Point2f>(1).X, (int)imagePoints.Get<Point2f>(1).Y), Scalar.Red, 2);
            Cv2.Line(image, new Point((int)imagePoints.Get<Point2f>(0).X, (int)imagePoints.Get<Point2f>(0).Y), new Point((int)imagePoints.Get<Point2f>(2).X, (int)imagePoints.Get<Point2f>(2).Y), Scalar.Green, 2);
            Cv2.Line(image, new Point((int)imagePoints.Get<Point2f>(0).X, (int)imagePoints.Get<Point2f>(0).Y), new Point((int)imagePoints.Get<Point2f>(3).X, (int)imagePoints.Get<Point2f>(3).Y), Scalar.Blue, 2);
        }

        public static void InitCameraMatrixAndDistCoeffs(Calibration calibration, Mat cameraMatrix, Mat distCoeffs)
        {
            Intrinsics Intrinsics = calibration.ColorCameraCalibration.Intrinsics;
            // Create the camera matrix
            cameraMatrix.Set(0, 0, Intrinsics.Parameters[2]);
            cameraMatrix.Set(0, 1, 0);
            cameraMatrix.Set(0, 2, Intrinsics.Parameters[0]);
            cameraMatrix.Set(1, 0, 0);
            cameraMatrix.Set(1, 1, Intrinsics.Parameters[3]);
            cameraMatrix.Set(1, 2, Intrinsics.Parameters[1]);
            cameraMatrix.Set(2, 0, 0);
            cameraMatrix.Set(2, 1, 0);
            cameraMatrix.Set(2, 2, 1f);

            // Create the distortion coefficients
            distCoeffs.Set(0, 0, Intrinsics.Parameters[4]);
            distCoeffs.Set(0, 1, Intrinsics.Parameters[5]);
            distCoeffs.Set(0, 2, Intrinsics.Parameters[13]);
            distCoeffs.Set(0, 3, Intrinsics.Parameters[12]);
            distCoeffs.Set(0, 4, Intrinsics.Parameters[6]);
            distCoeffs.Set(0, 5, Intrinsics.Parameters[7]);
            distCoeffs.Set(0, 6, Intrinsics.Parameters[8]);
            distCoeffs.Set(0, 7, Intrinsics.Parameters[9]);
        }

        public void UpdateTexture(Mat mat, Texture2D texture)
        {
            int width = mat.Width;
            int height = mat.Height;
            int channels = mat.Channels();

            // Calculate the start points of the 100x100 area
            int startX = (width - 200) / 2;
            int startY = (height - 200) / 2;

            // Convert the Mat's data to a byte array
            byte[] data = new byte[width * height * channels];
            Marshal.Copy(mat.Data, data, 0, data.Length);

            byte[] croppedData = new byte[200 * 200 * channels];

            if (channels == 4)
            {
                for (int y = 0; y < 200; y++)
                {
                    for (int x = 0; x < 200; x++)
                    {
                        int i = ((startY + y) * width + (startX + x)) * channels;
                        int j = (y * 200 + x) * channels;

                        // Swap the red and blue channels and copy the pixel data to the cropped array
                        croppedData[j] = data[i + 2];
                        croppedData[j + 1] = data[i + 1];
                        croppedData[j + 2] = data[i];
                        croppedData[j + 3] = data[i + 3];
                    }
                }
            }
            else if (channels == 3)
            {
                for (int y = 0; y < 200; y++)
                {
                    for (int x = 0; x < 200; x++)
                    {
                        int i = ((startY + y) * width + (startX + x)) * channels;
                        int j = (y * 200 + x) * channels;

                        // Swap the red and blue channels and copy the pixel data to the cropped array
                        croppedData[j] = data[i + 2];
                        croppedData[j + 1] = data[i + 1];
                        croppedData[j + 2] = data[i];
                    }
                }
            }
            else
            {
                throw new ArgumentException("Input Mat must have 3 or 4 channels.");
            }

            // Load the byte array into the texture
            texture.LoadRawTextureData(croppedData);
            texture.Apply();
        }



        private void OnDestroy()
        {
            if (kinect != null)
            {
                kinect.StopCameras();
                kinect = null;
            }
            if (cap_opencv != null)
            {
                cap_opencv.Dispose();
                cap_opencv = null;
            }
            if (cameraMatrix != null)
            {
                cameraMatrix.Dispose();
                cameraMatrix = null;
            }
            if (distCoeffs != null)
            {
                distCoeffs.Dispose();
                distCoeffs = null;
            }
            if (bgrMat != null)
            {
                bgrMat.Dispose();
                bgrMat = null;
            }
        }

    }
}