using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Unity.ProjectAuditor.Editor.Core;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class CellAutoCities : MonoBehaviour
{
    [SerializeField] static int seed = 42;
    [SerializeField] int rows;
    [SerializeField] int cols;
    [SerializeField] GameObject building;
    [SerializeField] GameObject park;
    [SerializeField] GameObject road;
    float bdSize;
    float unitSize;
    float unitHeight;
    float roadSize;

    enum blockType {
        EMPTY,
        BUILDING,
        PARK            
    };

    struct Block
    {   
        public blockType type;
        public GameObject obj;

        public Block(blockType type)
        {   
            this.type = type;
            obj = null;
        }

        public void CreateBuilding(int zOff, int xOff, GameObject building, float size, float height, Transform parent)
        {
            // float randomHeight = Mathf.Abs(Mathf.PerlinNoise((float)i/rows, (float)j/cols));
            float randomHeight = rand.Next(10, 100) / 100f * height;

            Vector3 offset = new Vector3(xOff * size, 0, zOff * -size);
            obj = Instantiate(building, parent.position + offset, Quaternion.identity, parent);

            Vector3 scale = obj.transform.localScale;
            scale.y = randomHeight;
            obj.transform.localScale = scale;
        }

        public void CreatePark(int zOff, int xOff, GameObject park, float size, float height, Transform parent)
        {
            Vector3 offset = new Vector3(xOff * size, 0, zOff * -size);
            obj = Instantiate(park, parent.position + offset, Quaternion.identity, parent);   
        }   

        public void Reinitialize()
        {
            if (obj != null) Destroy(obj);
            type = blockType.EMPTY;
        }

        public void Print()
        {
            Debug.Log("OBJ: " + obj);
            Debug.Log("TYPE: " + type);
        }

    }
    Block [,] city;
    struct Crossroad
    {
        public bool hasBuilding;
        public bool hasRoadDown; 
        public bool hasRoadRight;

        GameObject crossroad;
        GameObject downRoad;
        GameObject rightRoad;
        
        public Crossroad(bool hasBuilding)
        {
            this.hasRoadDown = true;
            this.hasRoadRight = true;
            this.hasBuilding = hasBuilding;

            this.crossroad = null;
            this.rightRoad = null;
            this.downRoad = null;
        }

        public void createRoads(int zOff, int xOff, GameObject road, float unitSize, Transform parent)
        {
            if (hasRoadDown)
            {
                Vector3 offset = new Vector3(xOff * unitSize, 0, -zOff* unitSize) + (Vector3.left * unitSize / 2f);
                downRoad = Instantiate(road, parent.position + offset, Quaternion.identity, parent);
            }
            if (hasRoadRight)
            {
                Vector3 offset = new Vector3(xOff * unitSize, 0, -zOff* unitSize) + (Vector3.forward * unitSize / 2f);
                rightRoad = Instantiate(road, parent.position + offset, Quaternion.Euler(0, 90, 0), parent);
            }
        }

        public void Reinitialize()
        {
            if (downRoad != null) Destroy(downRoad);
            if (rightRoad != null) Destroy(rightRoad);

            if (crossroad != null) Destroy(crossroad);

            hasRoadDown = true;
            hasRoadRight = true;
        }
    }
    Crossroad [,] crossroads;

    [SerializeField] ParkManager parkManager;
    static System.Random rand = new System.Random(seed);
    [SerializeField] InputAction restartAction;

    Vector3 [] moves = 
    {
        Vector3.back,
        Vector3.forward,
        Vector3.right,
        Vector3.left
    };
    void Start()
    {   
        // Create Arrays that store city objects  
        crossroads = new Crossroad[rows + 1, cols + 1]; // 
        city = new Block[rows, cols];
        
        // Get size to offset buildings, their default height, and roadsizes
        bdSize = building.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        unitHeight = building.GetComponentInChildren<MeshRenderer>().bounds.size.y;
        roadSize = road.GetComponent<MeshRenderer>().bounds.size.x;

        // Each block will be slightly larger than the buildings, to make room for the roads
        unitSize = bdSize * 1.1f;

        // Init park manager default values
        parkManager.Init(bdSize, unitSize, transform.position.y);
        
        
        // Create the city
        InitBuildings();
        InitRoads();
        CreateCity();
    }

    void Initialize()
    {   
        // TODO: The amount should be random (but have at least 2-3), and the placement should be random as well, but for now we will just place a building in the first row with 50% chance
        for (int j = 0; j < cols; j++)
        {   
            if (rand.Next(100) < 50){
                city[0, j] = new Block(blockType.BUILDING);
            }
            else
            {
                city[0, j] = new Block(blockType.PARK);
            }
        }
        
    }

    void InitBuildings()
    {   
        Initialize();

        // Rules:
        List<int> placementVals = new List<int>{1, 2, 3, 4};

        for (int i = 0; i < rows - 1; i++)
        {   
            blockType left  = city[i, cols-1].type;
            blockType mid   = city[i, 0].type;
            blockType right = city[i, 1].type;
            for (int j = 0; j < cols; j++)
            {   
                int val = 0;
                if (left  == blockType.BUILDING) val += 4;
                if ( mid  == blockType.BUILDING) val += 2;
                if (right == blockType.BUILDING) val += 1;

                if (placementVals.Contains(val))
                {   
                    city[i+1, j] = new Block(blockType.BUILDING);
                }
                else
                {
                    city[i+1, j] = new Block(blockType.PARK);
                }

                left = mid;
                mid = right;
                right = city[i, (j + 2) % cols].type;
            }
        } 
    }

   

    void InitRoads()
    {   
       
        for (int i = 0; i < rows + 1; i++)
        {
            for (int j = 0; j < cols + 1; j++)
            {   

                crossroads[i, j] = new Crossroad(true);
                
                int idx_i = Mathf.Clamp(i , 0,  rows - 1);
                int idx_j = Mathf.Clamp(j , 0,  cols - 1);

                if (j == cols) crossroads[i, j].hasRoadRight = false;
                if (i == rows) crossroads[i, j].hasRoadDown = false;
                
                if (city[idx_i, idx_j].type != blockType.BUILDING)
                {   
                    if (j == cols) { crossroads[i, j].hasRoadDown = false;  continue; }
                    if (i == rows) { crossroads[i, j].hasRoadRight = false; continue; }

                    
                    if (!inbounds(i, j - 1) || city[i, j - 1].type != blockType.BUILDING) crossroads[i, j].hasRoadDown = false;
                    if (!inbounds(i - 1, j) || city[i - 1, j].type != blockType.BUILDING) crossroads[i, j].hasRoadRight = false;
                    if (!inbounds(i, j + 1) || city[i, j + 1].type != blockType.BUILDING) crossroads[i, j + 1].hasRoadDown = false;
                    if (!inbounds(i + 1, j) || city[i + 1, j].type != blockType.BUILDING) crossroads[i + 1, j].hasRoadRight = false;
                    
                }
            }
        }
    }

    void CreateCity()
    {
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < cols; j++)
            {    
                if (city[i,j].type == blockType.BUILDING) city[i,j].CreateBuilding(i, j, building, unitSize, unitHeight, transform);
                // else if (city[i,j].type == blockType.PARK) city[i,j].CreatePark(i, j, park, unitSize, unitHeight, transform);
            }
        }

        SearchAndCombineParks();

        for (int i = 0; i < rows + 1; i++){
            for (int j = 0; j < cols + 1; j++)
            {    
                crossroads[i,j].createRoads(i, j, road, unitSize, transform);
            }
        }

    }

    void SearchAndCombineParks()
    {
        HashSet<Vector2Int> visited = new();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {    
                if (city[i, j].type != blockType.PARK) continue;

                Vector2Int idx = new(i, j);

                if (visited.Contains(idx)) continue;

                CombineParkMeshes(idx, ref visited);

            }
        }

        parkManager.CreateParks();
    }

    void CombineParkMeshes(Vector2Int idx, ref HashSet<Vector2Int> visited)
    {
        Stack<Vector2Int> toVisit = new();
        List<Vector2Int> parkCells = new();

        Vector2Int [] moves =  
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down
        };

        toVisit.Push(idx);

        while (toVisit.Count != 0)
        {
            Vector2Int to = toVisit.Pop();
            if (visited.Contains(to)) continue;
            if (city[to.x, to.y].type != blockType.PARK) continue;


            parkCells.Add(to);
            visited.Add(to);


            foreach (var move in moves)
            {
                if (!inbounds(to + move)) continue;
                toVisit.Push(to + move);          
            }
        }

        parkManager.addPark(parkCells);

    }

    bool inbounds(int i, int j)
    {
        return  (i >= 0 && i < rows) && 
                (j >= 0 && j < cols);
    }

    bool inbounds(Vector2Int cell)
    {
        return  (cell.x >= 0 && cell.x < rows) && 
                (cell.y >= 0 && cell.y < cols);
    }


    void Restart(InputAction.CallbackContext inputAction)
    {      
        for (int i = 0; i < rows + 1; i++)
        {
            for (int j = 0; j < cols + 1; j++)
            {   
                if (i != rows && j != cols) city[i,j].Reinitialize();
                crossroads[i,j].Reinitialize();
            }
        }

        Start();
    }

    void OnEnable()
    {
        restartAction.Enable();
        restartAction.performed += Restart;
    }

    void OnDisable()
    {
        restartAction.performed -= Restart;
        restartAction.Disable();
    }


}
