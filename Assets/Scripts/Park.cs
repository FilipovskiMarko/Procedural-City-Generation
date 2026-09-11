using UnityEngine;
using System.Collections.Generic;
using System;

public class Park 
{
    List<Vector2Int> parkCells;
    Mesh parkMesh;
    List<Vector3> vertices;
    List<int> triangles;
    bool drawMesh = false;
    RenderParams renderParams;
    int vertPointer = 0;
    Dictionary<Vector2Int, int> topLeftIndex;
    public HashSet<Tuple<Vector2Int, Vector2Int>> connected;

    public ParkManager parkManager;
    
    public Park(ParkManager parkManager)
    {
        parkCells = new();

        parkMesh = new Mesh();
        drawMesh = false;

        vertPointer = 0;
        vertices = new();
        triangles = new();
        topLeftIndex = new();

        this.parkManager = parkManager;
        renderParams = new(parkManager.mat);
    }

    void CreatePark()
    {  
        foreach(var pair in connected)
        {
            Vector2Int from = pair.Item1;
            Vector2Int to   = pair.Item2;

            if (from.x > to.x || from.y > to.y)
            {
                from = pair.Item2;
                to   = pair.Item1;
            }

            if (ManhattanDist(from, to) == 1) JoinParkEdges(from, to);
            else if (ManhattanDist(from, to) == 2) JoinParkCorners(from, to);
        }

        
    } 

    public void UpdateMesh()
    {
        parkMesh = new();
        parkMesh.Clear();
        parkMesh.vertices = vertices.ToArray();
        parkMesh.triangles = triangles.ToArray();
        parkMesh.RecalculateNormals();
        parkMesh.RecalculateTangents();
        
        drawMesh = true;
    }

    int ManhattanDist(Vector2Int first, Vector2Int second)
    {
        return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
    }

    public void CreateParkBlock(Vector2Int start, bool [] hasPark)
    {   
        parkCells.Add(start);
        topLeftIndex.Add(start, vertPointer);

        float bdSize = parkManager.buildingSize;
        float unitSize = parkManager.unitSize;
        float totalLength = bdSize;

        int vertexPerPark = parkManager.verticesPerPark;

        // X axis is for collumns and Z axis is for rows
        // start is a vector where .x is the row and .y is the collumn index
        float startX = (start.y * unitSize) - bdSize / 2f;
        float startZ = -((start.x * unitSize) - bdSize / 2f); // Negative because we want rows to go down
        float randomY = parkManager.defaultY;


        // Order: up, right, down, left, up-right, down-right, down-left, up-left
        // True if there is a park in that direction, false if not
        bool up    = hasPark[0];
        bool right = hasPark[1];
        bool down  = hasPark[2];
        bool left  = hasPark[3];
        bool upRight    = hasPark[4];
        bool downRight  = hasPark[5];
        bool downLeft   = hasPark[6];
        bool upLeft     = hasPark[7];
        

        // n vertexes mean n-1 segments of equal length between each vertex, 
        // because for the first segment 2 verteces are needed
        float offset = totalLength / (vertexPerPark - 1);

        
        for (int i = 0; i < vertexPerPark; i++)
        {   
            bool isTopEdge = i == 0;
            bool isBottomEdge = i == vertexPerPark - 1;

            for (int j = 0; j < vertexPerPark; j++)
            {   
                
                bool isLeftEdge = j == 0;
                bool isRightEdge = j == vertexPerPark - 1;

                // if it is an edge vertex and the park is not connected to another park in that direction,
                // then set the height to the default height
                if ((isTopEdge && !up) || (isBottomEdge && !down) || (isLeftEdge && !left) || (isRightEdge && !right) ||
                    (isTopEdge && isLeftEdge && !upLeft)          || (isTopEdge && isRightEdge && !upRight)           ||
                    (isBottomEdge && isLeftEdge && !downLeft)     || (isBottomEdge && isRightEdge && !downRight)
                ) 
                {
                    randomY = parkManager.defaultY;
                }
                else
                {
                    randomY = GetRandomNoiseHeight(startX + (offset * j), startZ - (offset * i));
                }

                vertices.Add(new(startX + (offset * j), randomY, startZ - (offset * i)));


                if (i != vertexPerPark - 1 && j != vertexPerPark - 1)
                {
                    triangles.Add(vertPointer + 0);
                    triangles.Add(vertPointer + 1);
                    triangles.Add(vertPointer + vertexPerPark + 1);

                    triangles.Add(vertPointer + 0);
                    triangles.Add(vertPointer + vertexPerPark + 1);
                    triangles.Add(vertPointer + vertexPerPark);
                }

                vertPointer++;
                
            }
        } 
    }

    void LogParkBlockFlags(Vector2Int start, bool left, bool right, bool up, bool down)
    {
        Debug.Log(
            $"Park start vertex: {start} | left={left} | right={right} | up={up} | down={down}"
        );
    }

    void JoinParkEdges(Vector2Int from, Vector2Int to)
    {
        float bdSize = parkManager.buildingSize;
        float unitSize = parkManager.unitSize;

        int vertexPerPark = parkManager.verticesPerPark;

        float maxSize = bdSize / (vertexPerPark - 1);

        bool goingRight = from.x == to.x;

        int vertexPointerFrom = topLeftIndex[from];
        int vertexPointerTo   = topLeftIndex[to];


        // NOTE: If the road gets wider, there may be a need for added vertexes in between
        if (goingRight) 
        {
            vertexPointerFrom += vertexPerPark - 1;

            for (int i = 0; i < vertexPerPark - 1; i++)
            {
                triangles.Add(vertexPointerFrom);
                triangles.Add(vertexPointerTo);
                triangles.Add(vertexPointerTo + vertexPerPark);

                triangles.Add(vertexPointerFrom);
                triangles.Add(vertexPointerTo + vertexPerPark);
                triangles.Add(vertexPointerFrom + vertexPerPark);

                vertexPointerFrom += vertexPerPark;
                vertexPointerTo   += vertexPerPark;
            }   
        }
        else 
        {
            vertexPointerFrom += vertexPerPark * (vertexPerPark - 1);

            for (int i = 0; i < vertexPerPark - 1; i++)
            {
                triangles.Add(vertexPointerFrom);
                triangles.Add(vertexPointerFrom + 1);
                triangles.Add(vertexPointerTo + 1);

                triangles.Add(vertexPointerFrom);
                triangles.Add(vertexPointerTo + 1);
                triangles.Add(vertexPointerTo);

                vertexPointerFrom += 1;
                vertexPointerTo   += 1;
            }
        }
    }

    void JoinParkCorners(Vector2Int from, Vector2Int to)
    {
        int vertexPerPark = parkManager.verticesPerPark;

        Debug.Log(
            $"Park start vertex: {from} , Park end vertex: {to} | Joining corners"
        );

        int topLeftCorner = topLeftIndex[from] + (vertexPerPark * vertexPerPark) - 1;
        int bottomRightCorner = topLeftIndex[to];

        int topRightCorner = topLeftIndex[from + Vector2Int.up] + (vertexPerPark * (vertexPerPark - 1));
        int bottomLeftCorner = topLeftIndex[from + Vector2Int.right] + vertexPerPark - 1;

        triangles.Add(topLeftCorner);
        triangles.Add(topRightCorner);
        triangles.Add(bottomRightCorner );

        triangles.Add(topLeftCorner);
        triangles.Add(bottomRightCorner);
        triangles.Add(bottomLeftCorner);
    }
    public void CreateParkMesh()
    {   
        CreatePark();
        UpdateMesh();
    }

    public void DrawParkMesh(Material mat, Matrix4x4 matrix)
    {   
        if (!drawMesh) return;
        Graphics.RenderMesh(renderParams, parkMesh, 0, matrix);
    }

    float GetRandomNoiseHeight(float x, float z)
    {   
        float shift = parkManager.shift;

        // Shift vals between -0.6 and 1.4
        float randomHeight = (Mathf.PerlinNoise(x + shift, z + shift) - 0.3f) * 2f;
        
        return randomHeight * parkManager.heightScale;
    }

}
