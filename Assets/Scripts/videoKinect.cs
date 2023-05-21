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
        private Transformation transformation;
        private DetectorParameters detectorParameters;
        private Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        private Mat distCoeffs = new Mat(1, 8, MatType.CV_32FC1);
        public float markerLength = 0.08f;
        public float captureFrameRate = 24f;
        private BGRA[] colorData;
        private Calibration calibration;
        private Mat bgrMat;

        // Reference to the Kinect sensor GameObject in your Unity scene
        public GameObject kinectGameObject;
        public GameObject markerPrefab;
        // A dictionary to keep track of the marker GameObjects. The key is the marker id.
        private Dictionary<int, GameObject> markerObjects = new Dictionary<int, GameObject>();
        public FanucHandler fanucHandler;

        // This is the Mat that will store the snapshot
        public Mat snapshotMat { get; private set; }

        // This is the Texture2D that will store the snapshot in a format that can be applied to a RawImage
        public Texture2D snapshotTexture { get; private set; }

        private void Start()
        {
            // Initialize video capture and face detection
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
            transformation = calibration.CreateTransformation();
            InitCameraMatrixAndDistCoeffs(calibration, cameraMatrix, distCoeffs);
        }

        private void Update()
        {
            if (fanucHandler.receiving == true)
            {
                foreach (KeyValuePair<int, GameObject> entry in markerObjects)
                {
                    entry.Value.SetActive(false);
                }
            }
            else
            {
                foreach (KeyValuePair<int, GameObject> entry in markerObjects)
                {
                    entry.Value.SetActive(true);
                }
            }

            using (Capture capture = kinect.GetCapture())
            {
                Microsoft.Azure.Kinect.Sensor.Image colorImage = capture.Color;

                if (colorImage != null && colorImage.WidthPixels > 0 && colorImage.HeightPixels > 0)
                {
                    // Update colorData with the latest pixel data
                    colorData = colorImage.GetPixels<BGRA>().ToArray();

                    // Update colorMat with the latest colorData
                    GCHandle pinnedArray = GCHandle.Alloc(colorData, GCHandleType.Pinned);
                    IntPtr colorDataPtr = pinnedArray.AddrOfPinnedObject();
                    using (Mat colourMat = new Mat(colorImage.HeightPixels, colorImage.WidthPixels, MatType.CV_8UC4, colorDataPtr))
                    {
                        // Convert BGRA to BGR format
                        Cv2.CvtColor(colourMat, bgrMat, ColorConversionCodes.BGRA2BGR);

                        CvAruco.DetectMarkers(bgrMat, arucoDictionary, out var corners, out var ids, detectorParameters, out var rejectedPoints);
                        CvAruco.DrawDetectedMarkers(bgrMat, corners, ids, Scalar.Green);

                        if (ids.Length > 0)
                        {
                            using (Mat rvecsMat = new Mat())
                            using (Mat tvecsMat = new Mat())
                            {
                                CvAruco.EstimatePoseSingleMarkers(corners, markerLength, cameraMatrix, distCoeffs, rvecsMat, tvecsMat);

                                // Keep track of the ids that were detected in this frame
                                HashSet<int> detectedIds = new HashSet<int>();

                                for (int i = 0; i < ids.Length; i++)
                                {
                                    int id = ids[i];
                                    detectedIds.Add(id);

                                    Vec3d rvec = rvecsMat.Get<Vec3d>(i);
                                    Vec3d tvec = tvecsMat.Get<Vec3d>(i);


                                    // Convert rotation vector to rotation matrix
                                    Mat rotationMatrix = new Mat();
                                    Cv2.Rodrigues(rvec, rotationMatrix);

                                    // Get marker pose in Kinect space
                                    Vector3 markerPositionInKinectSpace = new Vector3(-(float)tvec[0], (float)tvec[1], (float)tvec[2]); // If Y axis needs to be flipped

                                    // Get the transform from Kinect space to Unity world space
                                    Matrix4x4 kinectToWorld = kinectGameObject.transform.localToWorldMatrix;

                                    // Transform the marker pose from Kinect space to Unity world space
                                    Vector3 markerPositionInWorldSpace = kinectToWorld.MultiplyPoint3x4(markerPositionInKinectSpace);

                                    DrawAxis(bgrMat, rvec, tvec, markerLength, cameraMatrix, distCoeffs);
                                    GameObject markerObject;
                                    if (!markerObjects.TryGetValue(id, out markerObject))
                                    {
                                        // If the marker GameObject doesn't exist yet, create it
                                        markerObject = Instantiate(markerPrefab, markerPositionInWorldSpace, Quaternion.identity);
                                        markerObjects[id] = markerObject;
                                    }
                                    else
                                    {
                                        // If the marker GameObject already exists, update its pose
                                        markerObject.transform.position = markerPositionInWorldSpace;
                                        markerObject.transform.rotation = Quaternion.identity;

                                    }
                                }
                                // Remove any marker GameObjects that were not detected in this frame
                                List<int> idsToRemove = new List<int>();
                                foreach (int id in markerObjects.Keys)
                                {
                                    if (!detectedIds.Contains(id))
                                    {
                                        idsToRemove.Add(id);
                                    }
                                }
                                foreach (int id in idsToRemove)
                                {
                                    GameObject markerObject = markerObjects[id];
                                    Destroy(markerObject);
                                    markerObjects.Remove(id);
                                }
                            }

                        }
                        else
                        {
                            // If no markers were detected, remove all marker GameObjects
                            foreach (GameObject markerObject in markerObjects.Values)
                            {
                                Destroy(markerObject);
                            }
                            markerObjects.Clear();
                        }

                        if (outputImage.texture != null)
                        {
                            Destroy(outputImage.texture);
                        }
                        outputImage.texture = MatToTexture2D(bgrMat);

                    }
                    pinnedArray.Free();
                }
            }
        }

        public void TakeSnapshot()
        {
            // Create a new Mat to store the snapshot
            snapshotMat = new Mat();
            // Copy the current bgrMat into snapshotMat
            bgrMat.CopyTo(snapshotMat);

            // Create a new Texture2D to store the snapshot
            snapshotTexture = MatToTexture2D(snapshotMat);
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

        public static Texture2D MatToTexture2D(Mat mat)
        {
            int width = mat.Width;
            int height = mat.Height;
            int channels = mat.Channels();
            Texture2D texture;

            // Convert the Mat's data to a byte array
            byte[] data = new byte[width * height * mat.ElemSize()];
            Marshal.Copy(mat.Data, data, 0, data.Length);

            if (channels == 4)
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                // Swap the red and blue channels
                for (int i = 0; i < data.Length; i += 4)
                {
                    byte temp = data[i];
                    data[i] = data[i + 2];
                    data[i + 2] = temp;
                }
            }
            else if (channels == 3)
            {
                texture = new Texture2D(width, height, TextureFormat.RGB24, false);

                // Swap the red and blue channels
                for (int i = 0; i < data.Length; i += 3)
                {
                    byte temp = data[i];
                    data[i] = data[i + 2];
                    data[i + 2] = temp;
                }
            }
            else
            {
                throw new ArgumentException("Input Mat must have 3 or 4 channels.");
            }

            // Load the byte array into the texture
            texture.LoadRawTextureData(data);
            texture.Apply();

            return texture;
        }

        private void OnDestroy()
        {
            kinect.StopCameras();
            cap_opencv.Dispose();
            cameraMatrix.Dispose();
            distCoeffs.Dispose();
            bgrMat.Dispose();
        }
    }
}