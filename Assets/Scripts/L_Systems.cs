using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class L_Systems : MonoBehaviour
{   
    // Variables:
    // B -> branch
    // L -> Leaf
    // Constants:
    // [ -> Push Pos and Rotation
    // ] -> Pop  Pos and Rotation
    // + -> Rotate 25 degrees cc-wise
    // - -> Rotate 25 degrees cl-wise
    // Rules:
    // B -> BR
    public static int seed = 42;
    System.Random rand = new System.Random(seed);

    [SerializeField] GameObject branch;
    [SerializeField] GameObject leaf;
    Vector3 pos;
    Vector3 rot;
    Vector3 size;
    
    [SerializeField] InputAction restartAction;
    List<GameObject> objs;

    Stack<Vector3> Rotations;
    Stack<Vector3> Positions;
    Stack<Vector3> Sizes;
    int branchCount;
    float branchHeight;

    [SerializeField] string input; 
    [SerializeField] string finalString;
    [SerializeField] bool useCustomSettings = false;
    [SerializeField] int generations = 2;
    [SerializeField] float rotationCoefficient = 25f;
    [SerializeField] float sizeCoefficient = 1.1f;
    [SerializeField] bool randomRotation = false;
    static FractalPlant info = new(init: "X");
    Dictionary<char, string> rules;
    void Start()
    {   
        objs = new List<GameObject>();
        Positions = new Stack<Vector3>();
        Rotations = new Stack<Vector3>();
        Sizes     = new Stack<Vector3>();

        if (!useCustomSettings)
        {   
            generations = info.generations;
            rotationCoefficient = info.rotationCoefficient;
            sizeCoefficient = info.sizeCoefficient;
        }
        input = info.axiom;
        finalString = input;
        rules = info.rules;


        pos  = Vector3.zero;
        rot  = Vector3.zero;
        size = branch.GetComponentInChildren<MeshRenderer>().bounds.size * sizeCoefficient;
        gameObject.GetComponent<MeshFilter>().sharedMesh = null;

        generations = Mathf.Min(generations, 10);


        branchCount = 0;
        branchHeight = size.y;


        

        PassGenerations();
        ReadString();
        CombineMeshes();
    }

    void ReadString()
    {
        foreach (char c in finalString)
        {   
            switch (c)
            {
                case 'F': { CreateBranch(); } break;
                case 'X': { CreateBranch(); } break;
                case '[': { Push(); } break;
                case ']': { Pop(); } break;
                case '>': { OffsetRotation(0,  rotationCoefficient,  0); } break;
                case '<': { OffsetRotation(0, -rotationCoefficient,  0); } break;
                case '^': { OffsetRotation(0,   0, rotationCoefficient); } break;
                case '&': { OffsetRotation(0,   0,-rotationCoefficient); } break;
                case '+': { ChangeSize(sizeCoefficient); } break;
                case '-': { ChangeSize(1/sizeCoefficient); } break;
                       
                default : { Debug.Log("Character: " + c + " not recognized!"); } break;
            }
        }
    }

    void PassGenerations()
    {   
        for (int i = 0; i < generations; i++){
            string newString = "";

            foreach (char c in finalString)
            {
                newString += TransformChar(c);
            }

            finalString = newString;
        }
    }

    string TransformChar(char c)
    {
        if (rules.ContainsKey(c)) return rules[c];

        else return c.ToString();
    }

    void CreateBranch()
    {
        GameObject obj = Instantiate(branch, pos, Quaternion.Euler(rot), transform);
        objs.Add(obj);

        obj.transform.localScale = size;
        pos += obj.transform.up * 2 * size.y;

        

        branchCount++;  
 
   
    }

    void CreateLeaf()
    {   
        CreateBranch();

        // GameObject lf = Instantiate(leaf, pos, Quaternion.Euler(rot), transform);
        // objs.Add(lf);

    }

    void Push()
    {
        Positions.Push(pos);
        Rotations.Push(rot);
        Sizes.Push(size);
    }

    void Pop()
    {
        pos  = Positions.Pop();
        rot  = Rotations.Pop();
        size = Sizes.Pop();
    }

    void OffsetRotation(float x, float y, float z)
    {   
        rot.x += x; rot.y += y; rot.z += z;

        if (Positions.Count > 2)
        {   
            int min = -5; int max = 5;

            rot.x += rand.Next(min, max);
            rot.y += rand.Next(min, max);
            rot.z += rand.Next(min, max);
        }
    }

    void ChangeSize(float coef)
    {
        size *= coef;
    }

    void CombineMeshes()
    {
        MeshFilter [] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance [] instances = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var meshFilter = meshFilters[i];

            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix
            };

            meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        combinedMesh.CombineMeshes(instances);
        gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        gameObject.SetActive(true);

        foreach(GameObject obj in objs) Destroy(obj);

        
    }

    void OnDestroy()
    {
        foreach (GameObject obj in objs) Destroy(obj);
    }

    void Restart(InputAction.CallbackContext context)
    {
        foreach (GameObject obj in objs) Destroy(obj);
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
