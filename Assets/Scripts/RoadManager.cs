using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [SerializeField] Mesh roadMesh;
    [SerializeField] Mesh crossroadMesh;

    [SerializeField] Material roadMat;
    [SerializeField] Material crossroadMat;

    List<Matrix4x4> roadMatrices;
    List<Matrix4x4> crossroadMatrices;

    public void Init()
    {
        roadMatrices = new List<Matrix4x4>();
        crossroadMatrices = new List<Matrix4x4>();
    }

    public void AddRoad(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
        roadMatrices.Add(matrix);
    }

    public void AddCrossroad(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
        crossroadMatrices.Add(matrix);
    }

    public void DrawRoads()
    {
        if (roadMatrices.Count > 0)
        {
            Graphics.DrawMeshInstanced(roadMesh, 0, roadMat, roadMatrices);
        }

        if (crossroadMatrices.Count > 0)
        {
            Graphics.DrawMeshInstanced(crossroadMesh, 0, crossroadMat, crossroadMatrices);
        }
    }
}
