using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Threading;

namespace Telexistence
{
    public class FanucHandler : MonoBehaviour
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private string _serverIP = "127.0.0.1";
        private int _port = 5000;

        public Transform kinect_cursor;
        public Transform worldPosition;
        public List<Transform> robot = new List<Transform>();
        private Vector3 tempPos = new();
        private Vector3 tempRot = new();
        string previousMessage = null;
        private CancellationTokenSource _cancellationTokenSource;
        public bool messageReachability;

        void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            ConnectToServer();
            ReadDataFromServerAsync(_cancellationTokenSource.Token);
            StartCoroutine(SendDataCoroutine());
        }

        IEnumerator SendDataCoroutine()
        {
            while (true)
            {
                if (kinect_cursor.localPosition != tempPos || kinect_cursor.localEulerAngles != tempRot)
                {
                    tempPos = gameObject.transform.localPosition;
                    tempRot = gameObject.transform.localEulerAngles;

                    var rx = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).x;
                    var ry = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).y;
                    var rz = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).z;

                    if (float.IsNaN(rx) == false && float.IsNaN(ry) == false && float.IsNaN(rz) == false)
                    {

                        string message = $"{-kinect_cursor.localPosition.x * 1000},{kinect_cursor.localPosition.y * 1000},{kinect_cursor.localPosition.z * 1000},{rx},{ry},{rz}";
                        if (previousMessage == null || previousMessage != message)
                        {
                            SendMessageToServer(message);
                            previousMessage = message;
                        }
                    }
                }
                yield return new WaitForSeconds(1f / 5f);
            }
        }

        private void SendMessageToServer(string message)
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to send message to server: " + e.Message);
                }
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient(_serverIP, _port);
                _stream = _client.GetStream();
                Debug.Log("Connected to server successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to server: " + e.Message);
            }
        }

        private async void ReadDataFromServerAsync(CancellationToken cancellationToken)
        {
            if (_stream == null) return;

            byte[] buffer = new byte[1024];
            StringBuilder accumulatedData = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_client.Connected)
                {
                    try
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                        if (bytesRead == 0)
                        {
                            Debug.LogWarning("Server closed the connection.");
                            Dispose();
                            return;
                        }

                        accumulatedData.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                        while (accumulatedData.ToString().Contains("\n"))
                        {
                            int newlineIndex = accumulatedData.ToString().IndexOf("\n");
                            string data = accumulatedData.ToString().Substring(0, newlineIndex);
                            accumulatedData.Remove(0, newlineIndex + 1);

                            //Debug.Log("Received data: " + data);

                            string[] values = data.Split(',');

                            // Handle message with position and joint angles
                            if (values.Length == 12)
                            {
                                // Parse joint angles and xyzwpr position
                                float[] jointAngles = new float[6];
                                for (int i = 0; i < 6; i++)
                                {
                                    jointAngles[i] = float.Parse(values[i]);
                                }

                                float x = float.Parse(values[6]);
                                float y = float.Parse(values[7]);
                                float z = float.Parse(values[8]);
                                float w = float.Parse(values[9]);
                                float p = float.Parse(values[10]);
                                float r = float.Parse(values[11]);

                                UpdateRobotTransforms(jointAngles, new Vector3(x, y, z), new Vector3(w, p, r));
                            }
                            // Handle message with reachability information
                            else if (values.Length == 1)
                            {
                                bool.TryParse(values[0], out messageReachability);
                                Debug.Log("Message Reachability: " + messageReachability);
                            }
                            else
                            {
                                Debug.LogError("Received incorrect number of values: " + values.Length + ". Data: " + data);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.LogWarning("Operation canceled.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to read data from server: " + e.Message);
                    }
                }
            }
        }


        private void UpdateRobotTransforms(float[] jointAngles, Vector3 position, Vector3 rotation)
        {
            if (robot.Count != jointAngles.Length)
            {
                Debug.LogError("Robot joint count doesn't match the joint angles received.");
                return;
            }

            robot[0].localEulerAngles = new Vector3(0, 0, -jointAngles[0]);
            robot[1].localEulerAngles = new Vector3(0, -jointAngles[1], 0);
            robot[2].localEulerAngles = new Vector3(0, jointAngles[2] + jointAngles[1], 0);
            robot[3].localEulerAngles = new Vector3(-jointAngles[3], 0, 0);
            robot[4].localEulerAngles = new Vector3(0, jointAngles[4], 0);
            robot[5].localEulerAngles = new Vector3(-jointAngles[5], 0, 0);

            worldPosition.localPosition = new Vector3(-position.x / 1000, position.y / 1000, position.z / 1000);
            Vector3 eulerAngles = CreateQuaternionFromFanucWPR(rotation.x, rotation.y, rotation.z).eulerAngles;
            worldPosition.localEulerAngles = new Vector3(eulerAngles.x, -eulerAngles.y, -eulerAngles.z);
        }

        private Vector3 CreateFanucWPRFromQuaternion(Quaternion q)
        {
            float W = Mathf.Atan2(2 * ((q.w * q.x) + (q.y * q.z)), 1 - 2 * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2))) * (180 / Mathf.PI);
            float P = Mathf.Asin(2 * ((q.w * q.y) - (q.z * q.x))) * (180 / Mathf.PI);
            float R = Mathf.Atan2(2 * ((q.w * q.z) + (q.x * q.y)), 1 - 2 * (Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2))) * (180 / Mathf.PI);

            return new Vector3(W, -P, -R);
        }
        public Quaternion CreateQuaternionFromFanucWPR(float W, float P, float R)
        {
            float Wrad = W * (Mathf.PI / 180);
            float Prad = P * (Mathf.PI / 180);
            float Rrad = R * (Mathf.PI / 180);

            float qx = (Mathf.Cos(Rrad / 2) * Mathf.Cos(Prad / 2) * Mathf.Sin(Wrad / 2)) - (Mathf.Sin(Rrad / 2) * Mathf.Sin(Prad / 2) * Mathf.Cos(Wrad / 2));
            float qy = (Mathf.Cos(Rrad / 2) * Mathf.Sin(Prad / 2) * Mathf.Cos(Wrad / 2)) + (Mathf.Sin(Rrad / 2) * Mathf.Cos(Prad / 2) * Mathf.Sin(Wrad / 2));
            float qz = (Mathf.Sin(Rrad / 2) * Mathf.Cos(Prad / 2) * Mathf.Cos(Wrad / 2)) - (Mathf.Cos(Rrad / 2) * Mathf.Sin(Prad / 2) * Mathf.Sin(Wrad / 2));
            float qw = (Mathf.Cos(Rrad / 2) * Mathf.Cos(Prad / 2) * Mathf.Cos(Wrad / 2)) + (Mathf.Sin(Rrad / 2) * Mathf.Sin(Prad / 2) * Mathf.Sin(Wrad / 2));

            return new Quaternion(qx, qy, qz, qw);
        }

        void OnDisable()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

    }
}