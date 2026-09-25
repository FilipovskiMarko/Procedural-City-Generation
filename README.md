# Procedural Generation using L-Systems and Cellular Automata

This is a project i made for generating Trees using l-systems and grid based cities with cell auto 

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

Stochastic rules are implemented by mapping every variable to an array of strings and randomly choosing an element from it

```c#
rules = new Dictionary<char, string[]>
            {   
                {'X' , new[] {"Output1", "Output2", "Output3"}},
            };
```

Leafy Tree class that contains necessary data to create a tree of that type

```c#
public LeafyTree(string init = "FA")
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
#### Explanation:
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
Here you can see how the pointer object is used to track where we want to draw the mesh,
The pointer object is is an Unity Transform object that is attached to the tree object as a child, all the transformations are applied to it

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
#### Results



## Cell Auto

Cell Auto Explanation


## Challenges and Solutions
- Parks
- Optimisation
- etc

## 
