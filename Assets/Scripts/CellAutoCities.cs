using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class CellAutoCities : MonoBehaviour
{
    [SerializeField] static int seed = 42;
    [SerializeField] int rows;
    [SerializeField] int cols;
    [SerializeField] int minBuildingsInFirstRow = 2;
    [SerializeField] int maxBuildingsInFirstRow = 3;
    [SerializeField] GameObject [] buildingPrefabs;
    List<GameObject> objs;
    float bdSize;
    float unitSize;
    float unitHeight;
    float roadSize;
    bool drawCity = false;

    Block [,] city;
    Crossroad [,] crossroads;

    [SerializeField] ParkManager parkManager;
    [SerializeField] RoadManager roadManager;
    static System.Random rand = new System.Random(seed);
    [SerializeField] InputAction restartAction;

    void Start()
    {   
        // Stop Drawing city if reseting, so that the meshes don't get drawn while they are being destroyed and recreated
        drawCity = false;

        // Create Arrays that store city objects  
        objs = new();
        crossroads = new Crossroad[rows + 1, cols + 1]; // 
        city = new Block[rows, cols];
        
        // Get size to offset buildings, their default height, and roadsizes
        bdSize = 1f;
        roadSize = bdSize / 10f;

        // Each unit will be slightly larger than the buildings, to make room for the roads
        unitSize = bdSize + roadSize;

        // Init park and road manager default values
        parkManager.Init(bdSize, unitSize, transform.position.y);
        roadManager.Init();
            
        // Create the city
        InitBuildings();
        InitRoads();
        CreateCity();
        CreateParks();

        drawCity = true;
    }

    void Update()
    {   
        if (drawCity)
        {
            parkManager.DrawParkMeshes();
            roadManager.DrawRoads();
        }
    }

    void InitializeFirstRow()
    {   
        maxBuildingsInFirstRow = Mathf.Min(cols + 1, maxBuildingsInFirstRow);
        int randBuildingCount = UnityEngine.Random.Range(minBuildingsInFirstRow, maxBuildingsInFirstRow);

        List<int> randParks = new(cols);
        for(int i = 0; i < cols; i++) randParks.Add(i); 

        for (int i = 0; i < randBuildingCount; i++)
        {
            int indx = UnityEngine.Random.Range(0, randParks.Count);
            randParks.RemoveAt(indx);
        }

        for (int j = 0; j < cols; j++)
        {   
            if (randParks.Contains(j)) city[0, j] = new Block(blockType.PARK);
            else city[0, j] = new Block(blockType.BUILDING);
        } 
    }

    void InitBuildings()
    {   
        InitializeFirstRow();

        // Rules
        // If row above contains is P(ark), P(ark), B(uilding) it is 1 because it's 001 in binary (P = 0, B = 1)
        // So only PPB, PBP, PBB, BPP produce a building on the row below
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

                if (j == cols) { crossroads[i, j].hasRoadRight = false;}
                if (i == rows) { crossroads[i, j].hasRoadDown  = false;}

                if (i == rows || j == cols) continue;
                
                if (city[i, j].type != blockType.BUILDING)
                {   
                    if (InboundsCity(i, j - 1) && !IsBuilding(i, j - 1)) crossroads[i, j].hasRoadDown = false;
                    if (InboundsCity(i - 1, j) && !IsBuilding(i - 1, j)) crossroads[i, j].hasRoadRight = false;
                    

                    // Could be replaced with a binary system that also gives information
                    // On the direction of the crossroad, but for now we will just check if the crossroad is valid or not
                    if (InboundsCity(i - 1, j) && InboundsCity(i, j - 1) &&
                        !crossroads[i - 1, j].hasRoadDown && !crossroads[i, j - 1].hasRoadRight)
                    {
                        crossroads[i, j].hasCrossroad = false;
                    }
                }
            }
        }
    }

    void CreateCity()
    {
        for (int i = 0; i < rows + 1; i++){
            for (int j = 0; j < cols + 1; j++)
            {    
                if (i != rows && j != cols && city[i,j].type == blockType.BUILDING) {
                    GameObject building = buildingPrefabs[UnityEngine.Random.Range(0, buildingPrefabs.Length)];
                    city[i,j].CreateBuilding(i, j, building, unitSize, unitHeight, transform, objs);
                }

                crossroads[i,j].CreateRoads(i, j, roadManager, unitSize, roadSize, transform);
            }
        }
    }

    void CreateParks()
    {
        HashSet<Vector2Int> visited = new();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {    
                if (city[i, j].type != blockType.PARK) continue;

                Vector2Int idx = new(i, j);

                if (visited.Contains(idx)) continue;

                CombineParkMeshes(idx, visited);

            }
        }

        parkManager.CreateParks();
    }

    void CombineParkMeshes(Vector2Int idx, HashSet<Vector2Int> visited)
    {
        Stack<Vector2Int> toVisit = new();
        HashSet<Tuple<Vector2Int, Vector2Int>> connected = new();

        // up, right, down, left
        Vector2Int [] moves4dir =  
        {
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down
        };

        // up, right, down, left, up-right, down-right, down-left, up-left
        Vector2Int [] moves8dir =  
        {
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left + Vector2Int.up,
            Vector2Int.right + Vector2Int.up,
            Vector2Int.right + Vector2Int.down,
            Vector2Int.left + Vector2Int.down,
        };

        int parkIdx = parkManager.CreateNewPark();


        toVisit.Push(idx);
        visited.Add(idx);

        while (toVisit.Count != 0)
        {
            var to = toVisit.Pop();
            
            foreach (var move in moves4dir)
            {   
                var neighbor = to + move;
                if (!InboundsCity(neighbor) || !IsPark(neighbor)) continue;

                if (!connected.Contains(new(to, neighbor)) && !connected.Contains(new(neighbor, to))) {
                    connected.Add(new (to, neighbor));   
                }

                if (!visited.Contains(neighbor)) {
                    toVisit.Push(neighbor);
                    visited.Add(neighbor);
                }
            }

            bool [] hasPark = new bool[8];
            for (int i = 0; i < moves8dir.Length; i++)
            {
                var move = moves8dir[i];
                if (!InboundsCity(to + move) || !IsPark(to + move)) continue;

                hasPark[i] = true;
            }
            
            // Finaly add the park block to the park manager, which will create the mesh for it
            parkManager.AddParkBlockAndTrees(parkIdx, to, hasPark);
            

            // Only need to check the diagonal direction that is down and right, 
            // since the other 3 directions will be checked when the other cells are visited
            var diagNeighbor = to + new Vector2Int(1, 1);

            // has park down, right, and down-right
            bool isValid = hasPark[1] && hasPark[2] && hasPark[5];

            if (isValid && !connected.Contains(new(to, diagNeighbor)) && 
                           !connected.Contains(new(diagNeighbor, to)))
            {
                connected.Add(new (to, diagNeighbor));
            } 
        }
        // Set the connected park blocks for the park, 
        // so that when the mesh is created, it will know which edges/corners to connect to other parks
        parkManager.SetConnected(parkIdx, connected);
    }

    void Restart(InputAction.CallbackContext inputAction)
    {   
        int rows = city.GetLength(0);
        int cols = city.GetLength(1);   
        for (int i = 0; i < rows + 1; i++)
        {
            for (int j = 0; j < cols + 1; j++)
            {   
                if (i != rows && j != cols) city[i,j].Reinitialize();
                crossroads[i,j].Reinitialize();
            }
        }

        objs.ForEach(obj => Destroy(obj));

        Start();
    }

    bool InboundsCity(int i, int j)
    {
        return  (i >= 0 && i < rows) && 
                (j >= 0 && j < cols);
    }

    bool InboundsCity(Vector2Int cell)
    {
        return  (cell.x >= 0 && cell.x < rows) && 
                (cell.y >= 0 && cell.y < cols);
    }

    bool IsPark(Vector2Int cell)
    {
        return city[cell.x, cell.y].type == blockType.PARK;
    }

    bool IsBuilding(Vector2Int cell)
    {
        return city[cell.x, cell.y].type == blockType.BUILDING;
    }

    bool IsBuilding(int i, int j)
    {
        return city[i, j].type == blockType.BUILDING;
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
