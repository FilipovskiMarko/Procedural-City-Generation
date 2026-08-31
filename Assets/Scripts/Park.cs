using UnityEngine;
using System.Collections.Generic;
using System;

public class Park 
{
    List<Vector2Int> parkCells;

    Mesh parkMesh;

    List<Vector3> vertices;
    List<int> triangles;

    public bool drawMesh = false;

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
    }

    void CreatePark()
    {  
        // TODO: Change with one method that finds all the parks and creates the vertexes and triangles at once
        // HashSet<Tuple<Vector2Int, Vector2Int>> connected = new();
        // HashSet<Tuple<Vector2Int, Vector2Int>> diagonal  = new();

        // vertPointer = 0;
        // for (int i = 0; i < parkCells.Count; i++)
        // {   
        //     Vector2Int cell = parkCells[i];
        //     topLeftIndex.Add(cell, vertPointer);

        //     Debug.Log("Creating park mesh for " + cell);
        //     CreateParkBlock(cell, new bool[]{false, false, false, false});

        //     for (int j = i; j < parkCells.Count; j++)
        //     {   
        //         Vector2Int to = parkCells[j];
        //         if (manhattanDist(cell, to) != 1) continue;
        //         if (connected.Contains(new(cell, to)) || connected.Contains(new (to, cell)) ) continue;

        //         connected.Add(new(cell, to));
        //     }
        // }

        foreach(var pair in connected)
        {
            Vector2Int from = pair.Item1;
            Vector2Int to   = pair.Item2;

            if (from.x > to.x || from.y > to.y)
            {
                from = pair.Item2;
                to   = pair.Item1;
            }

            if (manhattanDist(from, to) == 1) JoinParks(from, to);
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

    int manhattanDist(Vector2Int first, Vector2Int second)
    {
        return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
    }


    public void CreateParkBlock(Vector2Int start, bool left, bool right, bool up, bool down)
    {   
        parkCells.Add(start);
        topLeftIndex.Add(start, vertPointer);

        LogParkBlockFlags(start, left, right, up, down);

        float bdSize = parkManager.buildingSize;
        float unitSize = parkManager.unitSize;

        float totalLength = bdSize;
        int vertexPerPark = parkManager.verticesPerPark;

        // X axis is for collumns and Z axis is for rows
        // start is a vector where .x is the row and .y is the collumn index
        float startX = (start.y * unitSize) - bdSize / 2f;
        float startZ = -((start.x * unitSize) - bdSize / 2f); // Negative because we want rows to go down
        float startY = parkManager.defaultY;


        
        

        // n vertexes mean n-1 segments of equal length between each vertex, 
        // because for the first segment 2 verteces are needed
        float offset = totalLength / (vertexPerPark - 1);

    
        for (int i = 0; i < vertexPerPark; i++)
        {
            for (int j = 0; j < vertexPerPark; j++)
            {   
                
                float randomY;
                if ((i == 0 && !up) || (i == vertexPerPark-1 && !down) || (j == 0 && !left) || (j == vertexPerPark-1 && !right)) randomY = startY;
                else randomY = GetRandomNoiseHeight(startX + (offset * j), startZ - (offset * i));
                //

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

    void JoinParks(Vector2Int from, Vector2Int to)
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

    public void CreateMesh()
    {   
        Debug.Log("Creating park mesh");
        CreatePark();
        UpdateMesh();

    }

    public void DrawMesh(Material mat, Matrix4x4 matrix)
    {   
        RenderParams rp = new(mat);
        Graphics.RenderMesh(rp, parkMesh, 0, matrix);

        Debug.Log("Drawing mesh " + parkMesh);
    }

    float GetRandomNoiseHeight(float x, float z)
    {
        return (Mathf.PerlinNoise(x, z) - 0.5f) * parkManager.heightIncrease;
    }

}
