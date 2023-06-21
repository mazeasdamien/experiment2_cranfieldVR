using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Telexistence;
using UnityEngine.UI;

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
    public LineRenderer lineRenderer;
    public GameObject prefab;
    private List<GameObject> gameObjects = new List<GameObject>();
    public GameObject robot_controller;
    // the speed at which the robot moves between positions
    public float robotSpeed = 1f;
    public float pause = 2f;
    private bool isFollowingPath = false;
    public RawImage feed;
    public GameObject Canvas;
    public GameObject RawimageInstance;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "RobotData.json");
        string jsonString = File.ReadAllText(path);
        PositionData positionData = JsonUtility.FromJson<PositionData>(jsonString);

        // Initialize the number of points
        lineRenderer.positionCount = positionData.positions.Count;

        // Instantiate gameObjects for all positions
        for (int i = 0; i < positionData.positions.Count; i++)
        {
            Vector3 pos = new Vector3(positionData.positions[i].X, positionData.positions[i].Y, positionData.positions[i].Z);
            GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(-100, -90, 0));
            gameObjects.Add(obj);
        }
    }

    void Update()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "RobotData.json");
        string jsonString = File.ReadAllText(path);
        PositionData positionData = JsonUtility.FromJson<PositionData>(jsonString);

            // Update the number of points
            lineRenderer.positionCount = positionData.positions.Count;

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
            lineRenderer.SetPosition(i, pos);
            gameObjects[i].transform.position = pos;
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

            // Ensure the robot reaches the exact target position and rotation
            robot_controller.transform.localPosition = targetPos;
            robot_controller.transform.rotation = targetRot; // use global rotation

            yield return new WaitForSeconds(pause);

            // Instantiate RawImageInstance and set its parent to Canvas
            GameObject rawImageObj = Instantiate(RawimageInstance, Canvas.transform);

            // Set the position of the RawImageInstance to arrange them horizontally
            RectTransform rectTransform = rawImageObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(100 + gameObjects.IndexOf(target) * 183, -270);

            // Get the RawImage component and apply the snapshot of the feed
            RawImage rawImage = rawImageObj.GetComponent<RawImage>();
            Texture2D snapshot = new Texture2D(feed.texture.width, feed.texture.height);
            RenderTexture.active = feed.texture as RenderTexture;

            // Yield a frame to ensure the RenderTexture is ready
            yield return new WaitForEndOfFrame();

            snapshot.ReadPixels(new Rect(0, 0, feed.texture.width, feed.texture.height), 0, 0);
            snapshot.Apply();
            rawImage.texture = snapshot;
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