using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CellAutoCities : MonoBehaviour
{
    [SerializeField] static int seed = 42;
    [SerializeField] int rows;
    [SerializeField] int cols;
    [SerializeField] GameObject building;
    [SerializeField] GameObject crossroad;
    [SerializeField] GameObject road;
    float bdSize;
    float unitSize;
    float unitHeight;
    float roadSize;
    bool drawCity = false;
    [SerializeField] static float roadWidth = 0.1f;

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
        public bool hasCrossroad;
        public bool hasRoadDown; 
        public bool hasRoadRight;

        GameObject crossroad;
        GameObject downRoad;
        GameObject rightRoad;
        
        public Crossroad(bool hasCrossroad)
        {
            this.hasRoadDown = true;
            this.hasRoadRight = true;
            this.hasCrossroad = true;

            this.crossroad = null;
            this.rightRoad = null;
            this.downRoad = null;
        }

        public void CreateRoads(int zOff, int xOff, RoadManager roadManager, float unitSize, Transform parent)
        {   
            // TODO: Simplify scaling issue, i don't know why the size is so small
            Vector3 scale = new Vector3(roadWidth / 10f, 1, roadWidth);

            if (hasRoadDown)
            {
                Vector3 offset = new Vector3(xOff * unitSize, 0, -zOff* unitSize) + (Vector3.left * unitSize / 2f);
                roadManager.AddRoad(parent.position + offset, Quaternion.identity, scale);
            }
            if (hasRoadRight)
            {
                Vector3 offset = new Vector3(xOff * unitSize, 0, -zOff* unitSize) + (Vector3.forward * unitSize / 2f);
                roadManager.AddRoad(parent.position + offset, Quaternion.Euler(0, 90, 0), scale);

            }
        }

        public void CreateCrossroad(int zOff, int xOff, RoadManager roadManager, float unitSize, Transform parent)
        {   
            if (!hasCrossroad) return;
            
            Vector3 scale = new Vector3(roadWidth / 10f, 1, roadWidth / 10f);
            Vector3 offset = new Vector3((xOff - 0.5f) * unitSize, 0, (-zOff + 0.5f) * unitSize);

            roadManager.AddCrossroad(parent.position + offset, Quaternion.identity, scale);
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
    [SerializeField] RoadManager roadManager;
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
        // Stop Drawing city if reseting, so that the meshes don't get drawn while they are being destroyed and recreated
        drawCity = false;

        // Create Arrays that store city objects  
        crossroads = new Crossroad[rows + 1, cols + 1]; // 
        city = new Block[rows, cols];
        
        // Get size to offset buildings, their default height, and roadsizes
        bdSize = building.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        unitHeight = building.GetComponentInChildren<MeshRenderer>().bounds.size.y;
        roadSize = road.GetComponent<MeshRenderer>().bounds.size.x;

        // Each block will be slightly larger than the buildings, to make room for the roads
        unitSize = bdSize + roadWidth;

        // Init park and road manager default values
        parkManager.Init(bdSize, unitSize, transform.position.y);
        roadManager.Init();
        
        
        // Create the city
        InitBuildings();
        InitRoads();
        CreateCity();

        drawCity = true;
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

                if (j == cols) { crossroads[i, j].hasRoadRight = false;}
                if (i == rows) { crossroads[i, j].hasRoadDown  = false;}

                if (i == cols || j == rows) continue;
                
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
                crossroads[i,j].CreateRoads(i, j, roadManager, unitSize, transform);
                crossroads[i,j].CreateCrossroad(i, j, roadManager, unitSize, transform);
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
            parkManager.AddParkBlock(parkIdx, to, hasPark);
            

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

    void Update()
    {   
        if (drawCity)
        {
            parkManager.DrawParkMeshes();
            roadManager.DrawRoads();
        }
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
