using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;


namespace Telexistence
{
    public class FanucHandler : MonoBehaviour
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private string _serverIP = "127.0.0.1";
        private int _port = 5000;

        public Transform kinect_cursor;
        public Transform worldPosition;
        public List<Transform> robot = new List<Transform>();

        void Start()
        {
            ConnectToServer();
            ReadDataFromServerAsync();
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient(_serverIP, _port);
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());
                Debug.Log("Connected to server successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to server: " + e.Message);
            }
        }

        private async void ReadDataFromServerAsync()
        {
            if (_reader == null) return;

            char[] buffer = new char[1024];
            while (true)
            {
                if (_client.Connected)
                {
                    int bytesRead = await _reader.ReadAsync(buffer, 0, buffer.Length);
                    string data = new string(buffer, 0, bytesRead);
                    Debug.Log("Received data: " + data); // Add this line to log the received data

                    string[] values = data.Split(',');

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
            robot[2].localEulerAngles = new Vector3(0, jointAngles[3] + jointAngles[2], 0);
            robot[3].localEulerAngles = new Vector3(-jointAngles[4], 0, 0);
            robot[4].localEulerAngles = new Vector3(0, jointAngles[5], 0);
            robot[5].localEulerAngles = new Vector3(-jointAngles[6], 0, 0);

            worldPosition.localPosition = new Vector3(-position.x / 1000, position.y / 1000, position.z / 1000);
            Vector3 eulerAngles = CreateQuaternionFromFanucWPR(rotation.x, rotation.y, rotation.z).eulerAngles;
            worldPosition.localEulerAngles = new Vector3(eulerAngles.x, -eulerAngles.y, -eulerAngles.z);
        }

        void OnApplicationQuit()
        {
            if (_client != null)
            {
                _reader.Close();
                _writer.Close();
                _client.Close();
            }
        }        

        /*
        void RegisterNewPoseIfChanged()
        {
            if (kinect_cursor.localPosition != tempPos || kinect_cursor.localEulerAngles != tempRot)
            {
                xyzWpr.X = -kinect_cursor.localPosition.x * 1000;
                xyzWpr.Y = kinect_cursor.localPosition.y * 1000;
                xyzWpr.Z = kinect_cursor.localPosition.z * 1000;
                xyzWpr.W = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).x;
                xyzWpr.P = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).y;
                xyzWpr.R = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).z;
                sysGroupPosition.Update();
                tempPos = gameObject.transform.localPosition;
                tempRot = gameObject.transform.localEulerAngles;
            }
        }*/

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
    }
}