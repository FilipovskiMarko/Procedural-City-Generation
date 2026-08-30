using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkManager : MonoBehaviour
{   
    public CellAutoCities cellAuto;
    public float buildingSize;
    public float unitSize;
    public float defaultY;
    public int verticesPerPark = 10;
    public float heightIncrease = 1f;
    public Material mat;
    List<Park> parks;


    // NOTE: The parks are searched twice, once to add them to the list and once to find those that are next to each other
    //       Make both of those functionalities into one and move it to the park manager class
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

    public void addPark(List<Vector2Int> cells)
    {
        parks.Add(new Park(cells, this));
    }


    public void CreateParks()
    {
        foreach (var park in parks) park.CreateMesh();
    }




   

    
}
