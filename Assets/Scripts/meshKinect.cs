using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using UnityEngine.VFX;
using TMPro;

namespace Telexistence
{
    public class meshKinect : MonoBehaviour
    {
        private int midDepth = -1;
        private int prevDepth = -1;

        public bool isRobotMoving = false;

        Device kinect;
        int depthWidth;
        int depthHeight;
        int num;
        Mesh mesh;
        Mesh emptyMesh;
        Vector3[] vertices;
        Color32[] colors;
        int[] indeces;
        Transformation transformation;

        public VisualEffect effect;
        public FanucHandler fanucHandler;
        public float maxDistance = 1.0f; // Define the maximum distance
        public LineCreator lineCreator;
        public GameObject textPrefab;
        private GameObject instantiatedText = null;
        public float textsize;

        public VisualEffect dmeshTempEffect;
        private bool hasAppliedLastMesh = false;
        public Mesh lastMesh;
        public modalities m;

        private BGRA[] colorArray;
        private Short3[] pointCloud;
        private ushort[] depthData;

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
            if (emptyMesh != null)
            {
                Destroy(emptyMesh);
            }
        }

        private void OnApplicationQuit()
        {
            kinect.StopCameras();
            kinect.Dispose();
        }


        void Start()
        {
            mesh = new Mesh();
            emptyMesh = new Mesh();
            InitKinect();
            InitMesh();
            Task t = KinectLoop(kinect);
        }

        void Update()
        {
            int midDepthInCm = Mathf.CeilToInt(midDepth / 10.0f);
            int prevDepthInCm = Mathf.CeilToInt(prevDepth / 10.0f);

            if (midDepthInCm == 0)
            {
                lineCreator.lineDistance = 0;
                if (instantiatedText != null)
                {
                    instantiatedText.SetActive(false);
                }
            }
            else
            {
                lineCreator.lineDistance = (midDepth / 10.0f) / 100;

                // Instantiate the text prefab if it doesn't exist
                if (instantiatedText == null)
                {
                    instantiatedText = Instantiate(textPrefab, transform);
                }
                else
                {
                    instantiatedText.SetActive(true);
                }

                // Position the instantiated text in the middle of the line
                instantiatedText.transform.position = lineCreator.originObject.transform.position + lineCreator.originObject.transform.TransformDirection(0, lineCreator.lineDistance / 2, 0);

                // Set the text to display the distance
                TMP_Text tmpText = instantiatedText.GetComponentInChildren<TMP_Text>();
                tmpText.text = midDepthInCm.ToString() + " cm";

                // Increase the font size
                tmpText.fontSize = textsize;  // Adjust this value as needed

                // Make the text face the camera
                instantiatedText.transform.LookAt(Camera.main.transform);

                // The text will be flipped 180 degrees on its vertical axis after LookAt. Adjust it back.
                instantiatedText.transform.Rotate(0, 180, 0);
            }
            prevDepth = midDepth;
            // Save the last mesh before the robot starts moving
            if (!isRobotMoving && fanucHandler.receiving)
            {
                if (lastMesh != null)
                {
                    Destroy(lastMesh);
                }
                lastMesh = Instantiate(mesh);
                hasAppliedLastMesh = false;
            }

            // Apply the last mesh to the dmeshTempEffect VFX when the robot starts moving
            if (isRobotMoving && !hasAppliedLastMesh)
            {
                dmeshTempEffect.SetMesh("RemoteData", lastMesh);
                dmeshTempEffect.transform.position = effect.transform.position;
                dmeshTempEffect.transform.rotation = effect.transform.rotation;
                hasAppliedLastMesh = true;
            }

            // Clear the mesh if the robot is moving
            if (isRobotMoving)
            {
                mesh.Clear();
                effect.SetMesh("RemoteData", emptyMesh);
            }
        }


        void InitKinect()
        {
            kinect = Device.Open(0);
            kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30,
            });
            transformation = kinect.GetCalibration().CreateTransformation();
        }

        void InitMesh()
        {
            depthWidth = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
            depthHeight = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
            num = depthWidth * depthHeight;

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            vertices = new Vector3[num];
            colors = new Color32[num];
            Vector2[] uv = new Vector2[num];
            Vector3[] normals = new Vector3[num];
            indeces = new int[6 * (depthWidth - 1) * (depthHeight - 1)];

            int index = 0;
            for (int y = 0; y < depthHeight; y++)
            {
                for (int x = 0; x < depthWidth; x++)
                {
                    uv[index] = new Vector2(((float)(x + 0.5f) / (float)(depthWidth)), ((float)(y + 0.5f) / ((float)(depthHeight))));
                    normals[index] = new Vector3(0, -1, 0);
                    index++;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.normals = normals;
        }

        private async Task KinectLoop(Device device)
        {
            while (true)
            {
                using (Capture capture = await Task.Run(() => device.GetCapture()).ConfigureAwait(true))
                {
                    using (Microsoft.Azure.Kinect.Sensor.Image modifiedColor = transformation.ColorImageToDepthCamera(capture))
                    {
                        colorArray = modifiedColor.GetPixels<BGRA>().ToArray();

                        using (Microsoft.Azure.Kinect.Sensor.Image cloudImage = transformation.DepthImageToPointCloud(capture.Depth))
                        {
                            pointCloud = cloudImage.GetPixels<Short3>().ToArray();

                            using (Microsoft.Azure.Kinect.Sensor.Image depthImage = capture.Depth)
                            {
                                if (depthImage != null)
                                {
                                    int centerIndex = (depthImage.WidthPixels / 2) + (depthImage.HeightPixels / 2) * depthImage.WidthPixels;
                                    depthData = depthImage.GetPixels<ushort>().ToArray();
                                    midDepth = depthData[centerIndex];
                                }
                            }
                        }
                    }

                    int triangleIndex = 0;
                    int pointIndex = 0;
                    int topLeft, topRight, bottomLeft, bottomRight;
                    int tl, tr, bl, br;
                    for (int y = 0; y < depthHeight; y++)
                    {
                        for (int x = 0; x < depthWidth; x++)
                        {
                            float xVal = pointCloud[pointIndex].X * 0.001f;
                            float yVal = -pointCloud[pointIndex].Y * 0.001f;
                            float zVal = pointCloud[pointIndex].Z * 0.001f;

                            if (Mathf.Sqrt(xVal * xVal + yVal * yVal + zVal * zVal) <= maxDistance)
                            {
                                vertices[pointIndex].x = xVal;
                                vertices[pointIndex].y = yVal;
                                vertices[pointIndex].z = zVal;

                                colors[pointIndex].a = 255;
                                colors[pointIndex].b = colorArray[pointIndex].B;
                                colors[pointIndex].g = colorArray[pointIndex].G;
                                colors[pointIndex].r = colorArray[pointIndex].R;

                                if (x != (depthWidth - 1) && y != (depthHeight - 1))
                                {
                                    topLeft = pointIndex;
                                    topRight = topLeft + 1;
                                    bottomLeft = topLeft + depthWidth;
                                    bottomRight = bottomLeft + 1;
                                    tl = pointCloud[topLeft].Z;
                                    tr = pointCloud[topRight].Z;
                                    bl = pointCloud[bottomLeft].Z;
                                    br = pointCloud[bottomRight].Z;

                                    indeces[triangleIndex++] = topLeft;
                                    indeces[triangleIndex++] = topRight;
                                    indeces[triangleIndex++] = bottomLeft;

                                    indeces[triangleIndex++] = bottomLeft;
                                    indeces[triangleIndex++] = topRight;
                                    indeces[triangleIndex++] = bottomRight;
                                }
                            }

                            pointIndex++;
                        }
                    }

                    mesh.Clear();
                    mesh.vertices = vertices;
                    mesh.colors32 = colors;

                    mesh.triangles = indeces;
                    mesh.RecalculateBounds();

                    if (m.usePT == true)
                    {
                        effect.SetMesh("RemoteData", mesh);
                    }
                    else
                    {
                        effect.SetMesh("RemoteData", emptyMesh);
                    }
                }
            }
        }
    }
}