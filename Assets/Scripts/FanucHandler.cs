using FRRobot;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Telexistence
{
    public class FanucHandler : MonoBehaviour
    {
        public string robotIpAddress = "127.0.0.1";
        public Transform mover;
        FRCRobot _robot;
        public List<Transform> robot = new List<Transform>();
        public Vector3 currentPosition;
        public Quaternion currentRotation;

        void Start()
        {
            ConnectToRobot();
        }

        void Update()
        {
            if (_robot != null && _robot.IsConnected)
            {
                GetCurrentJointPositions();
            }
        }

        public void ConnectToRobot()
        {
            try
            {
                _robot = new();
                _robot.ConnectEx(robotIpAddress, false, 10, 1);
                Debug.Log("Connected to robot successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to robot: " + e.Message);
            }
        }

        public bool CheckConnection()
        {
            if (_robot != null)
            {
                if (_robot.IsConnected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        void GetCurrentJointPositions()
        {
            var curPosition = _robot.CurPosition;
            var groupPositionJoint = curPosition.Group[1, FRECurPositionConstants.frJointDisplayType];
            var groupPositionWorld = curPosition.Group[1, FRECurPositionConstants.frWorldDisplayType];
            groupPositionJoint.Refresh();
            var joint = (FRCJoint)groupPositionJoint.Formats[FRETypeCodeConstants.frJoint];
            var xyzWpr = (FRCXyzWpr)groupPositionWorld.Formats[FRETypeCodeConstants.frXyzWpr];

            robot[0].localEulerAngles = new Vector3(0, 0, -(float)joint[1]);
            robot[1].localEulerAngles = new Vector3(0, -(float)joint[2], 0);
            robot[2].localEulerAngles = new Vector3(0, (float)joint[3] + (float)joint[2], 0);
            robot[3].localEulerAngles = new Vector3(-(float)joint[4], 0, 0);
            robot[4].localEulerAngles = new Vector3(0, (float)joint[5], 0);
            robot[5].localEulerAngles = new Vector3(-(float)joint[6], 0, 0);

            Debug.Log(-mover.localPosition.x * 1000);
            Debug.Log(mover.localPosition.y * 1000);
            Debug.Log(mover.localPosition.z * 1000);
            Debug.Log(CreateFanucWPRFromQuaternion(mover.localRotation).x* 1000);
            Debug.Log(CreateFanucWPRFromQuaternion(mover.localRotation).y);
            Debug.Log(CreateFanucWPRFromQuaternion(mover.localRotation).z);

            // Set currentPosition and currentRotation
            currentPosition = new Vector3((float)xyzWpr.X, (float)xyzWpr.Y, (float)xyzWpr.Z);
            currentRotation = Quaternion.Euler((float)xyzWpr.W, (float)xyzWpr.P, (float)xyzWpr.R);
        }
        private Vector3 CreateFanucWPRFromQuaternion(Quaternion q)
        {
            float W = Mathf.Atan2(2 * ((q.w * q.x) + (q.y * q.z)), 1 - 2 * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2))) * (180 / Mathf.PI);
            float P = Mathf.Asin(2 * ((q.w * q.y) - (q.z * q.x))) * (180 / Mathf.PI);
            float R = Mathf.Atan2(2 * ((q.w * q.z) + (q.x * q.y)), 1 - 2 * (Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2))) * (180 / Mathf.PI);

            return new Vector3(W, -P, -R);
        }
    }
}