using System;
using System.Collections.Generic;
using UnityEngine;

public class ParkManager : MonoBehaviour
{   
    [System.NonSerialized] public float shift;
    [System.NonSerialized] public float buildingSize;
    [System.NonSerialized] public float unitSize;
    [System.NonSerialized] public float defaultY;

    List<Park> parks;
    List<GameObject> treeList;

    public int verticesPerPark = 10;
    public float heightScale = 0.1f;
    public Material parkMat;
    

    [SerializeField] bool drawTrees = false;
    [SerializeField] GameObject treePrefab;
    
    
    public void Init(float buildingSize, float unitSize, float defaultY)
    {
        parks = new();
        if (treeList != null && treeList.Count > 0) treeList.ForEach(obj => Destroy(obj));
        treeList = new();
        shift = UnityEngine.Random.value * 100f;

        this.buildingSize = buildingSize;
        this.unitSize = unitSize;
        this.defaultY = defaultY;
    }

    
    public void DrawParkMeshes()
    {
        foreach (var pk in parks) pk.DrawParkMesh(transform.localToWorldMatrix);  
    }

    public int CreateNewPark()
    {
        parks.Add(new Park(this));

        return parks.Count - 1;
    }

    public void AddParkBlockAndTrees(int parkIdx, Vector2Int cell, bool [] hasPark)
    {
        parks[parkIdx].CreateParkBlock(cell, hasPark);

        if (drawTrees) CreateTrees(cell); 
    }

    public void SetConnected(int parkIdx, HashSet<Tuple<Vector2Int, Vector2Int>> connected)
    {
        parks[parkIdx].connected = connected;
    }


    public void CreateParks()
    {
        foreach (var park in parks) park.CreateParkMesh();
    }


    void CreateTrees(Vector2Int cell)
    {   
        float randOffset = (UnityEngine.Random.value - 0.5f) / 2f;
        float randScale = (UnityEngine.Random.value / 4f) + 0.75f;
        int noiseDir;
        for (int i = 0; i < 2; i++){
            if (i % 2 == 0) noiseDir = 1;
            else noiseDir = -1;

            float startX  =  (cell.y * unitSize) + (randOffset * noiseDir);
            float startZ  = -(cell.x * unitSize) + (randOffset * noiseDir); // Negative because we want rows to go down
            float randomY =   GetRandomNoiseHeight(startX, startZ);

            Vector3 pos = new(startX, randomY, startZ);
            Quaternion rot = Quaternion.Euler(270, 0, 0);

            GameObject tree = Instantiate(treePrefab, pos, rot, transform);
            tree.transform.localScale *= randScale;

            treeList.Add(tree);
        }       
    }

    public float GetRandomNoiseHeight(float x, float z)
    {   
        // Shift vals between -0.6 and 1.4
        float randomHeight = (Mathf.PerlinNoise(x + shift, z + shift) - 0.3f) * 2f;
        return randomHeight * heightScale;
    }


}
