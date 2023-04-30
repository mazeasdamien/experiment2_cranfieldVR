using FRRobot;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telexistence
{
    public class FanucHandler : MonoBehaviour
    {
        public string robotIpAddress = "127.0.0.1";
        public Transform kinect_cursor;
        public Transform worldPosition;
        FRCRobot _robot;
        public List<Transform> robot = new List<Transform>();
        private Vector3 tempPos = new();
        private Vector3 tempRot = new();

        void Start()
        {
            ConnectToRobot();
        }

        void Update()
        {
            if (_robot != null && _robot.IsConnected)
            {
                GetCurrentJointPositions();
                RegisterNewPoseIfChanged();
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

        void GetCurrentJointPositions()
        {
            var curPosition = _robot.CurPosition;
            var groupPositionJoint = curPosition.Group[1, FRECurPositionConstants.frJointDisplayType];
            var groupPositionWorld = curPosition.Group[1, FRECurPositionConstants.frWorldDisplayType];
            groupPositionJoint.Refresh();
            var joint = (FRCJoint)groupPositionJoint.Formats[FRETypeCodeConstants.frJoint];
            var xyzWpr = (FRCXyzWpr)groupPositionWorld.Formats[FRETypeCodeConstants.frXyzWpr];

            worldPosition.localPosition = new Vector3(-(float)xyzWpr.X/1000, (float)xyzWpr.Y/1000, (float)xyzWpr.Z/1000);
            Vector3 eulerAngles = CreateQuaternionFromFanucWPR((float)xyzWpr.W, (float)xyzWpr.P, (float)xyzWpr.R).eulerAngles;
            worldPosition.localEulerAngles = new Vector3(eulerAngles.x, -eulerAngles.y, -eulerAngles.z);

            robot[0].localEulerAngles = new Vector3(0, 0, -(float)joint[1]);
            robot[1].localEulerAngles = new Vector3(0, -(float)joint[2], 0);
            robot[2].localEulerAngles = new Vector3(0, (float)joint[3] + (float)joint[2], 0);
            robot[3].localEulerAngles = new Vector3(-(float)joint[4], 0, 0);
            robot[4].localEulerAngles = new Vector3(0, (float)joint[5], 0);
            robot[5].localEulerAngles = new Vector3(-(float)joint[6], 0, 0);
        }

        void RegisterNewPoseIfChanged()
        {
            if (kinect_cursor.localPosition != tempPos || kinect_cursor.localEulerAngles != tempRot)
            {
                FRCSysPositions sysPositions = _robot.RegPositions;
                FRCSysPosition sysPosition = sysPositions[1];
                FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                FRCXyzWpr xyzWpr = (FRCXyzWpr)sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];
                xyzWpr.X = -kinect_cursor.localPosition.x * 1000;
                xyzWpr.Y = kinect_cursor.localPosition.y * 1000;
                xyzWpr.Z = kinect_cursor.localPosition.z * 1000;
                xyzWpr.W = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).x;
                xyzWpr.P = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).y;
                xyzWpr.R = CreateFanucWPRFromQuaternion(kinect_cursor.localRotation).z;
                sysGroupPosition.Update();

                if (sysGroupPosition.IsReachable[Type.Missing, FREMotionTypeConstants.frJointMotionType, FREOrientTypeConstants.frAESWorldOrientType, Type.Missing, out _])
                {

                }
                tempPos = gameObject.transform.localPosition;
                tempRot = gameObject.transform.localEulerAngles;
            }
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
    }
}