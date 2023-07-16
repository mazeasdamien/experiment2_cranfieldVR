using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Telexistence;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class Position
{
    public float X;
    public float Y;
    public float Z;
}

[System.Serializable]
public class PositionData
{
    public List<Position> positions;
}

public class pathUpdater : MonoBehaviour
{
    public GameObject prefab;
    private List<GameObject> gameObjects = new List<GameObject>();
    public GameObject robot_controller;
    public float robotSpeed = 1f;
    public float pause = 2f;
    private bool isFollowingPath = false;
    public videoKinect VideoKinect;
    public GameObject Canvas;
    public GameObject RawimageInstance;

    private Vector3 initialStartPos;
    private Vector3 initialEndPos;

    private string path;
    private DateTime lastRead = DateTime.MinValue;

    void Start()
    {
        string jsonDirectory = Path.Combine(Path.GetTempPath(), "RobotData");
        string jsonFileName = "RobotData.json";
        path = Path.Combine(jsonDirectory, jsonFileName);

        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }

        string jsonString = File.ReadAllText(path);
        PositionData positionData = JsonUtility.FromJson<PositionData>(jsonString);
        initialStartPos = robot_controller.transform.position;
        initialEndPos = robot_controller.transform.position;

        for (int i = 0; i < positionData.positions.Count; i++)
        {
            Vector3 pos = new Vector3(positionData.positions[i].X, positionData.positions[i].Y, positionData.positions[i].Z);
            GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(-100, -90, 0));
            gameObjects.Add(obj);
        }
    }

    void Update()
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }

        var lastWriteTime = File.GetLastWriteTimeUtc(path);

        if (lastWriteTime > lastRead)
        {
            lastRead = lastWriteTime;

            string jsonString = File.ReadAllText(path);
            PositionData positionData = JsonUtility.FromJson<PositionData>(jsonString);

            // Add or remove gameObjects as necessary
            while (gameObjects.Count < positionData.positions.Count)
            {
                Vector3 pos = new Vector3(positionData.positions[gameObjects.Count].X, positionData.positions[gameObjects.Count].Y, positionData.positions[gameObjects.Count].Z);
                GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(-100, -90, 0));
                gameObjects.Add(obj);
            }
            while (gameObjects.Count > positionData.positions.Count)
            {
                GameObject toRemove = gameObjects[gameObjects.Count - 1];
                gameObjects.RemoveAt(gameObjects.Count - 1);
                Destroy(toRemove);
            }

            // Update positions of all gameObjects
            for (int i = 0; i < positionData.positions.Count; i++)
            {
                Vector3 pos = new Vector3(positionData.positions[i].X, positionData.positions[i].Y, positionData.positions[i].Z);
                gameObjects[i].transform.position = pos;
            }
        }
    }

    // method to start the robot following the path
    public void StartPathFollowing()
    {
        if (!isFollowingPath)
        {
            // Destroy all RawImageInstance objects
            foreach (Transform child in Canvas.transform)
            {
                if (child.gameObject.CompareTag("RawImageInstance"))
                {
                    Destroy(child.gameObject);
                }
            }

            StopAllCoroutines();
            StartCoroutine(FollowPath());
        }
    }
    private IEnumerator FollowPath()
    {
        isFollowingPath = true;

        // Save initial position and rotation
        Vector3 initialPos = robot_controller.transform.localPosition;
        Quaternion initialRot = robot_controller.transform.localRotation;

        // Follow the path defined by gameObjects
        foreach (GameObject target in gameObjects)
        {
            // convert target position into the robot's parent's local space
            Vector3 targetPos = robot_controller.transform.parent.InverseTransformPoint(target.transform.position);
            Quaternion targetRot = target.transform.rotation; // use target's rotation

            // calculate the duration based on the distance to the target and the robotSpeed
            float duration = Vector3.Distance(robot_controller.transform.localPosition, targetPos) / robotSpeed;

            // move and rotate the robot towards the target using lerping
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                robot_controller.transform.localPosition = Vector3.Lerp(robot_controller.transform.localPosition, targetPos, elapsedTime / duration);
                robot_controller.transform.rotation = Quaternion.Slerp(robot_controller.transform.rotation, targetRot, elapsedTime / duration); // use global rotation
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(pause);

            // Instantiate RawImageInstance and set its parent to Canvas
            GameObject rawImageObj = Instantiate(RawimageInstance, Canvas.transform);

            // Set the position of the RawImageInstance to arrange them horizontally
            RectTransform rectTransform = rawImageObj.GetComponent<RectTransform>();
            int index = gameObjects.IndexOf(target);
            int row = index / 5; // Integer division to get the row number
            int column = index % 5; // Modulo to get the column number within the row
            rectTransform.anchoredPosition = new Vector2(100 + column * 200, -270 - row * 220); // Adjust these values as needed

            // Get the RawImage component
            RawImage rawImage = rawImageObj.GetComponent<RawImage>();

            Texture2D texture = new Texture2D(200, 200, TextureFormat.RGB24, false);

            VideoKinect.UpdateTexture(VideoKinect.bgrMat, texture);

            // Apply the cropped image to the RawImage
            rawImage.texture = texture;

            // Update the instance number
            TextMeshProUGUI instanceNumber = rawImageObj.GetComponentInChildren<TextMeshProUGUI>(); // Assumes the Text component is a child of RawImageInstance
            if (instanceNumber != null)
            {
                instanceNumber.text = (index + 1).ToString(); // +1 because index is 0-based
            }
        }

        // Return the robot controller to its initial position and rotation
        float returnDuration = Vector3.Distance(robot_controller.transform.localPosition, initialPos) / robotSpeed;
        float returnElapsedTime = 0f;
        while (returnElapsedTime < returnDuration)
        {
            robot_controller.transform.localPosition = Vector3.Lerp(robot_controller.transform.localPosition, initialPos, returnElapsedTime / returnDuration);
            robot_controller.transform.localRotation = Quaternion.Slerp(robot_controller.transform.localRotation, initialRot, returnElapsedTime / returnDuration);
            returnElapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the robot reaches the exact initial position and rotation
        robot_controller.transform.localPosition = initialPos;
        robot_controller.transform.localRotation = initialRot;

        isFollowingPath = false;
    }
}