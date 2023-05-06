using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.VFX;

public class KinectHandler : MonoBehaviour
{
    public RawImage rawImage;
    public int width = 1280;
    public int height = 720;

    public Device _device;
    private Transformation _transformation;
    private Texture2D _colorTexture;
    private byte[] _colorImageData;
    private bool _dataReady;
    public int id;

    public VisualEffect effect;

    Mesh mesh;
    readonly List<Vector3> vertices = new();
    readonly List<Color32> colors = new();
    readonly List<int> indices = new();

    void Start()
    {
        InitializeAzureKinectSensor();
        Task.Run(() => UpdateColorFrame(CancellationToken.None));
    }

    void Update()
    {
        if (_dataReady)
        {
            UpdateTexture();
            GenerateMesh();
            effect.SetMesh("RemoteData", mesh);
            _dataReady = false;
        }
    }

    private void InitializeAzureKinectSensor()
    {
        _device = Device.Open(id);
        _device.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            CameraFPS = FPS.FPS15,
            SynchronizedImagesOnly = true,
        });

        _transformation = _device.GetCalibration().CreateTransformation();
        _colorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        rawImage.texture = _colorTexture;
    }

    private async void UpdateColorFrame(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (Capture capture = await Task.Run(() => _device.GetCapture(), cancellationToken))
            {
                Microsoft.Azure.Kinect.Sensor.Image colorImage = capture.Color;
                if (colorImage != null)
                {
                    _colorImageData = colorImage.Memory.ToArray();
                    _dataReady = true;
                }
            }
            await Task.Delay(100, cancellationToken);
        }
    }

    private void UpdateTexture()
    {
        // Swap R and B channels
        for (int i = 0; i < _colorImageData.Length; i += 4)
        {
            byte r = _colorImageData[i];
            _colorImageData[i] = _colorImageData[i + 2];
            _colorImageData[i + 2] = r;
        }

        _colorTexture.LoadRawTextureData(_colorImageData);
        _colorTexture.Apply();
    }

    private void GenerateMesh()
    {
        // Clear previous mesh data
        vertices.Clear();
        colors.Clear();
        indices.Clear();

        // Generate mesh data
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                Color32 color = new Color32(_colorImageData[index], _colorImageData[index + 1], _colorImageData[index + 2], 255);
                Vector3 vertex = new Vector3(x, y, 0);

                vertices.Add(vertex);
                colors.Add(color);
                indices.Add(vertices.Count - 1);
            }
        }

        // Create mesh
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points, 0);
        mesh.UploadMeshData(false);
    }

    void OnDestroy()
    {
        if (_device != null)
        {
            _device.StopCameras();
            _device.Dispose();
            _device = null;
        }
    }
}