using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using UnityEngine.VFX;
using UnityEngine.UI;

public class meshKinect : MonoBehaviour
{
    Device kinect;
    int depthWidth;
    int depthHeight;
    int num;
    Mesh mesh;
    Vector3[] vertices;
    Color32[] colors;
    int[] indeces;
    Texture2D texture;
    Transformation transformation;

    public int nearClip = 300;
    public int farClip = 3000;
    public int ID;
    public VisualEffect effect;
    public RawImage rawImage;

    private void OnDestroy()
    {
        kinect.StopCameras();
    }

    void Start()
    {
        InitKinect();
        InitMesh();
        Task t = KinectLoop(kinect);
    }

    void InitKinect()
    {
        kinect = Device.Open(ID);
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
        texture = new Texture2D(depthWidth, depthHeight);
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
                Microsoft.Azure.Kinect.Sensor.Image modifiedColor = transformation.ColorImageToDepthCamera(capture);
                BGRA[] colorArray = modifiedColor.GetPixels<BGRA>().ToArray();

                Microsoft.Azure.Kinect.Sensor.Image cloudImage = transformation.DepthImageToPointCloud(capture.Depth);
                Short3[] PointCloud = cloudImage.GetPixels<Short3>().ToArray();

                int triangleIndex = 0;
                int pointIndex = 0;
                int topLeft, topRight, bottomLeft, bottomRight;
                int tl, tr, bl, br;
                for (int y = 0; y < depthHeight; y++)
                {
                    for (int x = 0; x < depthWidth; x++)
                    {
                        vertices[pointIndex].x = PointCloud[pointIndex].X * 0.001f;
                        vertices[pointIndex].y = -PointCloud[pointIndex].Y * 0.001f;
                        vertices[pointIndex].z = PointCloud[pointIndex].Z * 0.001f;

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
                            tl = PointCloud[topLeft].Z;
                            tr = PointCloud[topRight].Z;
                            bl = PointCloud[bottomLeft].Z;
                            br = PointCloud[bottomRight].Z;

                            if (tl > nearClip && tr > nearClip && bl > nearClip && tl < farClip && tr < farClip && bl < farClip)
                            {
                                indeces[triangleIndex++] = topLeft;
                                indeces[triangleIndex++] = topRight;
                                indeces[triangleIndex++] = bottomLeft;
                            }
                            else
                            {
                                indeces[triangleIndex++] = 0;
                                indeces[triangleIndex++] = 0;
                                indeces[triangleIndex++] = 0;
                            }

                            if (bl > nearClip && tr > nearClip && br > nearClip && bl < farClip && tr < farClip && br < farClip)
                            {
                                indeces[triangleIndex++] = bottomLeft;
                                indeces[triangleIndex++] = topRight;
                                indeces[triangleIndex++] = bottomRight;
                            }
                            else
                            {
                                indeces[triangleIndex++] = 0;
                                indeces[triangleIndex++] = 0;
                                indeces[triangleIndex++] = 0;
                            }
                        }
                        pointIndex++;
                    }
                }

                texture.SetPixels32(colors);
                texture.Apply();

                rawImage.texture = texture; // Display color image on RawImage

                mesh.vertices = vertices;
                mesh.colors32 = colors;

                mesh.triangles = indeces;
                mesh.RecalculateBounds();
                effect.SetMesh("RemoteData", mesh);
            }
        }
    }
}
