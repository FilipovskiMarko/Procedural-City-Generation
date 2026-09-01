using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkManager : MonoBehaviour
{   
    public float buildingSize;
    public float unitSize;
    public float defaultY;
    public int verticesPerPark = 10;
    public float heightIncrease = 1f;
    public Material mat;
    List<Park> parks;


    public void Init(float buildingSize, float unitSize, float defaultY)
    {
        parks = new();

        this.buildingSize = buildingSize;
        this.unitSize = unitSize;
        this.defaultY = defaultY;
    }

    void Update()
    {   
        foreach (var pk in parks) pk.DrawParkMesh(mat, transform.localToWorldMatrix);  
    }

    public int CreateNewPark()
    {
        parks.Add(new Park(this));

        return parks.Count - 1;
    }

    public void AddParkBlock(int parkIdx, Vector2Int cell, bool [] hasPark)
    {
        parks[parkIdx].CreateParkBlock(cell, hasPark);
    }

    public void SetConnected(int parkIdx, HashSet<Tuple<Vector2Int, Vector2Int>> connected)
    {
        parks[parkIdx].connected = connected;
    }


    public void CreateParks()
    {
        foreach (var park in parks) park.CreateParkMesh();
    }




   

    
}
