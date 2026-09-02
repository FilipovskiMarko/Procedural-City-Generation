using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] Mesh branch;
    [SerializeField] Mesh trunk;
    [SerializeField] Material branchMaterial;
    [SerializeField] InputAction restartAction;
    [SerializeField] Transform pointer;
    List<Matrix4x4> transformMatrices;

    struct stateData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public stateData(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            position = pos;
            rotation = rot;
            localScale = scale;
        }
    }
    Stack<stateData> pointerStack;


    bool drawTree = false;

    int branchCount;
    float branchHeight;

    [SerializeField] string input; 
    [SerializeField] string finalString;
    [SerializeField] bool useCustomSettings = false;
    [SerializeField] int generations = 2;
    [SerializeField] float rotationCoefficient = 25f;
    [SerializeField] float sizeCoefficient = 1.1f;
    static FractalPlant info = new(init: "X");
    Dictionary<char, string> rules;
    void Start()
    {   
        drawTree = false;
        transformMatrices = new List<Matrix4x4>();
        pointerStack = new Stack<stateData>();

        if (!useCustomSettings)
        {   
            generations = info.generations;
            rotationCoefficient = info.rotationCoefficient;
            sizeCoefficient = info.sizeCoefficient;
        }
        input = info.axiom;
        finalString = input;
        rules = info.rules;


        pointer.position  = transform.position;
        pointer.rotation  = transform.rotation;
        pointer.localScale = transform.localScale;

        branchCount = 0;
        branchHeight = branch.bounds.size.z;

        Debug.Log($"Branch Height: {branchHeight * pointer.localScale.z}, Branch bound size: {branch.bounds.size}");

        PassGenerations();
        ReadString();
        drawTree = true;
    }

    void Update()
    {   
        if (drawTree)
        {
            Graphics.DrawMeshInstanced(branch, 0, branchMaterial, transformMatrices);
        }
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
                case '^': { OffsetRotation( rotationCoefficient,   0, 0); } break;
                case '&': { OffsetRotation(-rotationCoefficient,   0, 0); } break;
                case '+': { ChangeSize(sizeCoefficient); } break;
                case '-': { ChangeSize(1/sizeCoefficient); } break;
                       
                default : { Debug.Log($"Character: {c} not recognized!"); } break;
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

            if (finalString.Length > 10000)
            {
                Debug.Log("Final string is too long, stopping at generation " + i);
                break;
            }
        }
    }

    string TransformChar(char c)
    {
        if (rules.ContainsKey(c)) return rules[c];

        else return c.ToString();
    }

    void CreateBranch()
    {
        Matrix4x4 transformMatrix = Matrix4x4.TRS(pointer.position, pointer.rotation, pointer.localScale);
        transformMatrices.Add(transformMatrix);

        pointer.Translate(Vector3.forward * branchHeight * pointer.localScale.z);

        branchCount++;  
    }

    void CreateLeaf()
    {   
        CreateBranch();

        // GameObject lf = Instantiate(leaf, pos, Quaternion.Euler(rot), transform);
        // transformMatrices.Add(lf);

    }

    void Push()
    {
        pointerStack.Push(new stateData(pointer.position, pointer.rotation, pointer.localScale));
    }

    void Pop()
    {
        stateData state = pointerStack.Pop();
        pointer.position = state.position;
        pointer.rotation = state.rotation;
        pointer.localScale = state.localScale;
    }

    void OffsetRotation(float x, float y, float z)
    {   
        Vector3 rot = pointer.rotation.eulerAngles;
        rot.x += x; rot.y += y; rot.z += z;

        pointer.rotation = Quaternion.Euler(rot);
    }

    void ChangeSize(float coef)
    {   
        Vector3 size = pointer.localScale;
        size *= coef;
        pointer.localScale = size;
    }

    void Restart(InputAction.CallbackContext context)
    {
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
