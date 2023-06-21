using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;
using VarjoExample;
using UnityEngine.UI;
using TMPro;

namespace Telexistence
{
    public class FanucHandler : MonoBehaviour
    {
        // Network and stream variables
        private TcpClient _client;
        private NetworkStream _stream;

        // Server connection settings
        private string _serverIP = "127.0.0.1";
        private int _port = 5000;

        // Transform objects for cursor and robot
        public Transform kinect_cursor;
        public Transform worldPosition;
        public List<Transform> robot = new List<Transform>();
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        // Temporary variables for position and rotation
        private Vector3 tempPos = new();
        private Vector3 tempRot = new();

        // Variable for previous message sent
        string previousMessage = null;

        // CancellationTokenSource for async operations
        private CancellationTokenSource _cancellationTokenSource;

        // Message reachability flag
        public bool messageReachability =true;
        private bool isYRotationInRange = true;
        public meshKinect meshKinect;

        public bool receiving;

        private bool isRunning = true;

        public TMP_InputField inputField;
        public Button sendButton;

        void Start()
        {
            // Initialize CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // Connect to the server and start reading data
            ConnectToServer();
            ReadDataFromServerAsync(_cancellationTokenSource.Token);

            // Save initial position and rotation of the Kinect cursor
            initialPosition = kinect_cursor.position;
            initialRotation = kinect_cursor.rotation;

            // Start coroutine for sending data
            StartCoroutine(SendDataCoroutine());
            sendButton.onClick.AddListener(SendAndClearInput);
        }

        private void SendAndClearInput()
        {
            string message = inputField.text;

            if (!string.IsNullOrEmpty(message))
            {
                SendMessageToServer(message);
                inputField.text = string.Empty;
            }
            //Debug.Log("Message sent: " + message);
        }

        void Update()
        {
            // Check if F10, F11 or F12 is pressed and then send corresponding message
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // Reset the position and rotation of the Kinect cursor
                kinect_cursor.position = initialPosition;
                kinect_cursor.rotation = initialRotation;
                SendMessageToServer("run");
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                SendMessageToServer("reset");
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                SendMessageToServer("stop");
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                // Reset the position and rotation of the Kinect cursor
                kinect_cursor.position = initialPosition;
                kinect_cursor.rotation = initialRotation;
                SendMessageToServer("home");
            }
        }

        // Coroutine to send data to the server
        IEnumerator SendDataCoroutine()
        {
            while (isRunning)
            {

                // Check if the local position or rotation of the cursor has changed
                if (kinect_cursor.localPosition != tempPos || kinect_cursor.localEulerAngles != tempRot)
                {
                    // Update temporary position and rotation
                    tempPos = kinect_cursor.localPosition;
                    tempRot = kinect_cursor.localEulerAngles;

                    float yRotation = kinect_cursor.localRotation.eulerAngles.y;

                    // Check if yRotation is within the desired range
                    if (yRotation >= 5 && yRotation <= 70)
                    {
                        isYRotationInRange = true; // Update bool

                        // Convert rotation to Fanuc WPR representation
                        Vector3 wpr = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation);

                        // Check if none of the WPR components are NaN
                        if (!float.IsNaN(wpr.x) && !float.IsNaN(wpr.y) && !float.IsNaN(wpr.z))
                        {
                            // Create the message to send
                            string message = $"{-tempPos.x * 1000},{tempPos.y * 1000},{tempPos.z * 1000},{wpr.x},{wpr.y},{wpr.z}";

                            // Send the message if it's different from the previous one
                            if (previousMessage == null || previousMessage != message)
                            {
                                SendMessageToServer(message);
                                previousMessage = message;
                            }
                        }
                    }
                    else
                    {
                        isYRotationInRange = false; // Update bool
                    }
                }

                // Wait for a fixed time interval before sending the next update
                yield return new WaitForSeconds(0.1f);
            }

        }

        // Function to send a message to the server
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

        // Function to connect to the server
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

        // Function to read data from the server asynchronously
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

                        // Append received data to the accumulatedData StringBuilder
                        accumulatedData.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                        StringBuilder dataBuilder = new StringBuilder();

                        // Process accumulated data line by line
                        while (accumulatedData.ToString().Contains("\n"))
                        {
                            dataBuilder.Clear();

                            int newlineIndex = accumulatedData.ToString().IndexOf("\n");
                            dataBuilder.Append(accumulatedData.ToString().Substring(0, newlineIndex));
                            accumulatedData.Remove(0, newlineIndex + 1);

                            string data = dataBuilder.ToString();

                            // Parse and handle the received data
                            string[] values = data.Split(',');

                            // Handle message with position, joint angles, and digital input value
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

                                // Set receiving to true and start the reset coroutine
                                receiving = true;
                                StartCoroutine(ResetReceivingCoroutine());
                            }
                            // Handle message with reachability information
                            else if (values.Length == 1)
                            {
                                if (isYRotationInRange)
                                {
                                    bool.TryParse(values[0], out messageReachability);
                                    //Debug.Log("Message Reachability: " + messageReachability);
                                }
                                else
                                {
                                    messageReachability = false;
                                }
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
                    catch (IOException e)
                    {
                        Debug.LogError("I/O exception occurred while reading data from server: " + e.Message);
                        Dispose();
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to read data from server: " + e.Message);
                    }
                }
            }
        }

        private IEnumerator ResetReceivingCoroutine()
        {
            // Wait for 0.5 seconds
            yield return new WaitForSeconds(2f);

            // Set receiving back to false
            receiving = false;
        }

        // Function to update robot transforms based on received joint angles and position
        private void UpdateRobotTransforms(float[] jointAngles, Vector3 position, Vector3 rotation)
        {
            if (robot.Count != jointAngles.Length)
            {
                Debug.LogError("Robot joint count doesn't match the joint angles received.");
                return;
            }

            // Update robot joint angles
            for (int i = 0; i < jointAngles.Length; i++)
            {
                Vector3 rot = Vector3.zero;
                switch (i)
                {
                    case 0:
                        rot = new Vector3(0, 0, -jointAngles[i]);
                        break;
                    case 1:
                        rot = new Vector3(0, -jointAngles[i], 0);
                        break;
                    case 2:
                        rot = new Vector3(0, jointAngles[i] + jointAngles[i - 1], 0);
                        break;
                    case 3:
                        rot = new Vector3(-jointAngles[i], 0, 0);
                        break;
                    case 4:
                        rot = new Vector3(0, jointAngles[i], 0);
                        break;
                    case 5:
                        rot = new Vector3(-jointAngles[i], 0, 0);
                        break;
                }
                robot[i].localEulerAngles = rot;
            }

            // Update robot position and rotation
            worldPosition.localPosition = new Vector3(-position.x / 1000, position.y / 1000, position.z / 1000);
            Vector3 eulerAngles = CreateQuaternionFromFanucWPR(rotation.x, rotation.y, rotation.z).eulerAngles;
            worldPosition.localEulerAngles = new Vector3(eulerAngles.x, -eulerAngles.y, -eulerAngles.z);
        }

        // Function to convert a Quaternion to FANUC WPR (Wrist, Pitch, Roll) angles
        private Vector3 CreateFanucWPRFromQuaternion(Quaternion q)
        {
            float W = Mathf.Atan2(2 * ((q.w * q.x) + (q.y * q.z)), 1 - 2 * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2))) * (180 / Mathf.PI);
            float P = Mathf.Asin(2 * ((q.w * q.y) - (q.z * q.x))) * (180 / Mathf.PI);
            float R = Mathf.Atan2(2 * ((q.w * q.z) + (q.x * q.y)), 1 - 2 * (Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2))) * (180 / Mathf.PI);

            return new Vector3(W, -P, -R);
        }

        // Function to convert FANUC WPR (Wrist, Pitch, Roll) angles to a Quaternion
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

        // Function to be called when the script is disabled
        void OnDisable()
        {
            // Remove the SendAndClearInput method from the button's OnClick event
            sendButton.onClick.RemoveListener(SendAndClearInput);

            Dispose();
        }

        // Function to clean up resources when disposing
        private void Dispose()
        {
            isRunning = false;

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