using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    void Start()
    {
        // Set a default position count
        lineRenderer.positionCount = 0;
        InvokeRepeating("UpdatePath", 0f, 0.1f);  // Call UpdatePath immediately and then every 1 second
    }

    void UpdatePath()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("RobotData");
        PositionData positionData = JsonUtility.FromJson<PositionData>(jsonText.text);

        // Set the number of points
        lineRenderer.positionCount = positionData.positions.Count;

        // Set the positions
        for (int i = 0; i < positionData.positions.Count; i++)
        {
            Vector3 pos = new Vector3(positionData.positions[i].X, positionData.positions[i].Y, positionData.positions[i].Z);
            lineRenderer.SetPosition(i, pos);
        }
    }
}
