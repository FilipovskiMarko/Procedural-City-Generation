# Procedural Generation using L-Systems and Cellular Automata

The goal of this project was to use multiple techniques for procedural generation to create a real looking city in Unity. It features a comprehensive explanation of the implementation of each of the systems,
and image results of said implementations,


## L Systems

The L-Systems use the turtle graphics principle by moving a pointer object through space using the following rules:

```
X   ->   Branch
L   ->   Leaf
[   ->   Push State
]   ->   Pop  State
+   ->   Scale up 
-   ->   Scale down
<   ->   Rotate left  (x-axis) 
>   ->   Rotate right (x-axis) 
^   ->   Rotate up    (y-axis) 
_   ->   Rotate down  (y-axis)
```
_The pointer object is is an Unity Transform object that is attached to the tree object as a child_



Stochastic rules are implemented by mapping every variable to an array of strings and randomly choosing an element from it

```c#
rules = new Dictionary<char, string[]>
            {   
                {'X' , new[] {"Output1", "Output2", "Output3"}},
            };
```

Each Tree Type is neatly sorted in a class that contains necessary data to create a tree of that type

```c#
public LeafyTree()
    {
        axiom = "A";
        generations = 9;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 25;
        rules = new Dictionary<char, string[]>
            {
                {'A' , new[] {"X-[-<B]>A", "X-[<A]->B", "X-[^B]-_A", "X-[-^A]_B"}},
                {'B' , new[] {"XL-[-<B]>B", "XL-[<B]->B", "XL-[^B]-_B", "XL-[-^B]_B"}},
                {'L' , new[] {"l"}},
                {'l' , new[] {""}}
            };
    }
```
#### How it works:
- The tree starts from an A branch (the Axiom), creating other A branches or B branches
- Branches are rotated and scaled by sizeCoef and rotCoef
- Eventually all the branches become B branches, producing other B branches and Leaves
- Each leaf decays going from "L" -> "l" -> "", (letting us have leaves only on the branches made by the last 2 generations)
- This repeats for 9 generations until the tree is successfully made

#### Drawing

Each of the branch and leaf states are transformed into a Matrix and stored, letting us draw all the meshes using GPU Instancing

```c#

Matrix4x4 transformMatrix = Matrix4x4.TRS(pointer.position, pointer.rotation, pointer.localScale);
branchMatrices.Add(transformMatrix);

// ... //

Graphics.RenderMeshInstanced(branchParams, branch, 0, branchMatrices);


```
_Here you can see how the pointer object is used to track where we want to draw the mesh_


Randomizing the pointer rotation by a user defined variable gives us trees that are less grid locked and have more natural branch flow

```c#
void OffsetRotation(float x, float y, float z)
    {  
        if (randomize)
        {
            x += Random.Range(-randomRotationRange, randomRotationRange);
            y += Random.Range(-randomRotationRange, randomRotationRange);
            z += Random.Range(-randomRotationRange, randomRotationRange);
        }

        pointer.Rotate(x, y, z, Space.Self);
    }
```
#### Tree Images:
<table>
  <tr>
    <td align="center">
            <img width="200" height="200" alt="LSys_Tree2" src="https://github.com/user-attachments/assets/f3b82d01-9b59-4414-881c-6103dddb2526" />
    </td>
    <td align="center">
            <img width="200" height="200" alt="Tree2" src="https://github.com/user-attachments/assets/05de3981-7c10-424f-9cfb-72dba717a003" />
    </td>
    <td align="center">
            <img width="200" height="200" alt="LSys_Tree3" src="https://github.com/user-attachments/assets/e456af15-f696-436d-bdcb-3229eb53e4ba" />
    </td>
 </tr>
 <tr>
    <td align="center">
            <img width="200" height="200" alt="LSys_Tree1" src="https://github.com/user-attachments/assets/36785926-ad90-407e-8125-e4396d206a68" />  
    </td>
    <td align="center">
            <img width="200" height="200" alt="Tree1" src="https://github.com/user-attachments/assets/5ba61197-68f4-4e47-91ce-983cf5cb3961" />
    </td>
    <td align="center">
            <img width="200" height="200" alt="Tree3" src="https://github.com/user-attachments/assets/6c47f48e-4764-42e8-aeac-c5a46e5dbdc8" />
    </td>

 </tr>
</table>

## Cellular Automation
#### Buildings
A grid of user defined sizes NxM is defined, and a minumum and maxiumum number of buildings in the first row
``` c#
[SerializeField] int rows;
[SerializeField] int cols;
[SerializeField] int minBuildingsInFirstRow = 2;
[SerializeField] int maxBuildingsInFirstRow = 3;
```

Then, after initializing the first row randomly, the values of the remaining rows are chosen programaticaly

To choose the type of the cell in position **(i+1, j)**, we look at the cells directly above it, so **(i, j-1), (i, j), (i, j+1)**,
Because we have 3 possible values that can either contain a building or not, we can represent the position as a 3bit binary number.

For example if the 3 above cells are all **buildings** we have a position that maps to the number: 111<sub>2</sub> = 7<sub>10</sub>,
if the cell furthest to the right is a Park and all other cells are buildings we have the number: 110<sub>2</sub> = 6<sub>10</sub>

We can represent every position like this and have a list of decimal numbers that represent rules for placing buildings in newly generated row

```c#

void InitBuildings()
    {   
        InitializeFirstRow();

        // Rules
        List<int> ruleset = new List<int>{1, 2, 3, 4};

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

                if (ruleset.Contains(val))
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
```
#### Roads
After that, we need to create roads between buidlings, for that we use an array just like the buildings, with one extra row and column added

The squares in this image represent the buildings, and the dots represent the crossroads so that every building has a crossroad on it's upper left corner

<img width="640" height="384" alt="CrossroadSC" src="https://github.com/user-attachments/assets/e83f4a76-f7b2-4d28-aa2a-79770958fe2f" />

Every crossroad object has 3 boolean flags, that look like this

```c#
struct Crossroad
{
    public bool hasCrossroad;
    public bool hasRoadDown; 
    public bool hasRoadRight;
}
```
_Only 3 flags are necessary to map every crossroad on the grid_

However, there only need to be crossroads/roads surrounding the tiles where the buildings were placed, so we need to loop through the city array and decide where we want there to be crossroads/roads
```c#
 void InitRoads()
    {   
       
        for (int i = 0; i < rows + 1; i++)
        {
            for (int j = 0; j < cols + 1; j++)
            {   
                // By default, assume every road/crossroad is needed, and remove the unnecessary ones  
                crossroads[i, j] = new Crossroad(true);    

                if (j == cols) { crossroads[i, j].hasRoadRight = false;}
                if (i == rows) { crossroads[i, j].hasRoadDown  = false;}

                if (i == rows || j == cols) continue;

                // If the cell is a park, we need to check the surrounding cells/crossroads to
                // determine if roads are necessary
                if (city[i, j].type != blockType.BUILDING)
                {   
                    if (InboundsCity(i, j - 1) && !IsBuilding(i, j - 1)) crossroads[i, j].hasRoadDown = false;
                    if (InboundsCity(i - 1, j) && !IsBuilding(i - 1, j)) crossroads[i, j].hasRoadRight = false;
                    
                    if (InboundsCity(i - 1, j) && InboundsCity(i, j - 1) &&
                        !crossroads[i - 1, j].hasRoadDown && !crossroads[i, j - 1].hasRoadRight)
                    {
                        crossroads[i, j].hasCrossroad = false;
                    }
                }
            }
        }
    }
```
<br>

As you can see the last row and column only contain one direction and always contain a crossroad, this is because we want there to be a border
around the city


<img width="640" height="384" alt="CrossroadBorder" src="https://github.com/user-attachments/assets/75e9dfaf-9a0c-4988-ae57-7ccf243486ab" />


<br>
After all the cells and roads are initialized, creating the city is a simple as going through the grid and running the custom Create function for each object

```c#
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
```
<br>
For the buildings a prefab is used, chosen randomly from an array filled manually by the user, also the building is rotated on its Y-Axis for variety

```c3
public void CreateBuilding(int zOff, int xOff, GameObject building, float size, float height, Transform parent, List<GameObject> objs)
    {
        Vector3 offset = new Vector3(xOff * size, 0, zOff * -size);
        float randomRot = Random.Range(0, 3 + 1) * 90f;

        // Keep referance to object so that it can be safely deleted later
        objs.Add(GameObject.Instantiate(building, parent.position + offset, Quaternion.Euler(270, randomRot, 0), parent));
    }
```
<br>
For the roads, the transformation matrices are sent to a RoadManager object, that uses GPUInstancing to create them 

```c#
public void CreateRoads(int zOff, int xOff, RoadManager roadManager, float unitSize, float roadSize, Transform parent)
    {   
        Vector3 scale = new Vector3(roadSize / 10f, 1, roadSize);

        if (hasRoadDown)
        {
            Vector3 offset = new Vector3(xOff * unitSize, 0, -zOff* unitSize) + (Vector3.left * unitSize / 2f);
            roadManager.AddRoad(parent.position + offset, Quaternion.identity, scale);
        }

        //...
    } 
```

All of the objects are drawn on the XZ Plane where the row coordinate is inverted on the Z-Axis [0, -inf) and the column coordinate is mapped to the X-Axis [0, +inf)


#### Parks

All of the park cells that are directly adjacent to each other, are connected into one singular mesh spanning all of the cells. The height value of each vertex for the mesh is obtained using the Perlin Noise Function (transformed from [0, 1] to [-0.6, 1.4] so that the parks go bellow as well as above the defaultY level) and scaled by a used defined scalar


```c#
 public float GetRandomNoiseHeight(float x, float z)
    {   
        // Shift vals between -0.6 and 1.4
        float randomHeight = (Mathf.PerlinNoise(x, z) - 0.3f) * 2f;
        return randomHeight * heightScale;
    }
```

## City Images:
<table>
  <tr>
    <td align="center">
            <img width="400" height="400" alt="TopView1" src="https://github.com/user-attachments/assets/669d421c-075b-4339-bf90-0082f9455c26" />
    </td>
    <td align="center">
            <img width="400" height="400" alt="TopView2" src="https://github.com/user-attachments/assets/bf69d51f-9bd5-4f98-b85e-48e76ffee91b" />
    </td>
 </tr>
</table>

<img width="1595" height="775" alt="Side-View" src="https://github.com/user-attachments/assets/31b1b159-2bee-4ac4-93a5-a3c466f3c216" />




## Final Results:
