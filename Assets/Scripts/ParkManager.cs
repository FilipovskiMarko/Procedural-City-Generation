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


    // NOTE: The parks are searched twice, once to add them to the list and once to find those that are next to each other
    //       Make both of those functionalities into one and move it to the park manager class
    //
    //       Adding ParkBlock = adding one park square to the grid
    //       Adding Park      = Adding a whole park with multiple or one parkblock/s
    public void Init(float buildingSize, float unitSize, float defaultY)
    {
        parks = new();

        this.buildingSize = buildingSize;
        this.unitSize = unitSize;
        this.defaultY = defaultY;
    }

    void Update()
    {   
        foreach (var pk in parks)
        {   
            Debug.Log(pk);
            if (pk.drawMesh) pk.DrawMesh(mat, transform.localToWorldMatrix);
        }
    }

    public int CreateNewPark()
    {
        parks.Add(new Park(this));

        return parks.Count - 1;
    }

    public void AddParkBlock(int parkIdx, Vector2Int cell, bool left, bool right, bool up, bool down)
    {
        parks[parkIdx].CreateParkBlock(cell, left, right, up, down);
    }

    public void SetConnected(int parkIdx, HashSet<Tuple<Vector2Int, Vector2Int>> connected)
    {
        parks[parkIdx].connected = connected;
    }


    public void CreateParks()
    {
        foreach (var park in parks) park.CreateMesh();
    }




   

    
}
