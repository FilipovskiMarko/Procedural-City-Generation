using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class L_Systems : MonoBehaviour
{   
    [SerializeField] int seed = 42;
    [SerializeField] Mesh branch;
    [SerializeField] Mesh trunk;
    [SerializeField] Mesh leaf;
    [SerializeField] Material branchMaterial;
    [SerializeField] Material leafMaterial;
    [SerializeField] Transform pointer;
    [SerializeField] InputAction restartAction;
    [SerializeField] string axiom; 
    [SerializeField] string finalString;
    [SerializeField] bool useCustomSettings = false;
    [SerializeField] int generations = 2;
    [SerializeField] float rotationCoefficient = 25f;
    [SerializeField] float sizeCoefficient = 1.1f;
    [SerializeField] bool randomize = false;
    [SerializeField] float randomRotationRange = 15f;
    [SerializeField] int leafCount = 5;
    [SerializeField] float leafScale = 0.05f;
    [SerializeField] float trunkScale = 1.5f;
    
    List<Matrix4x4> branchMatrices;
    List<Matrix4x4> leafMatrices;
    Matrix4x4 trunkMatrix;

    RenderParams branchParams;
    RenderParams leafParams;
    RenderParams trunkParams;
    Stack<stateData> pointerStack;

    int branchCount;
    float branchHeight;
    float leafHeight;
    bool drawTree = false;
    [SerializeField] string Type;
    TreeType info;
    Dictionary<char, string[]> rules;

    void Awake()
    {
        // Set the random seed for reproducibility
        Random.InitState(seed);
    }
    void Start()
    {
        switch (Type)
        {
            case "BasicTree":
                info = new BasicTree(init: "FA");
                break;
            case "LeafyTree":
                info = new LeafyTree(init: "FA");
                break;
            case "ThreeDTree":
                info = new ThreeDTree(init: "FA");
                break;
            case "ThreeDBinaryTree":
                info = new ThreeDBinaryTree(init: "FA");
                break;
            case "FractalPlant":
                info = new FractalPlant(init: "FA");
                break;
            case "TwoDBinaryTree":
                info = new TwoDBinaryTree(init: "FA");
                break;
            case "CurvyTree":
                info = new CurvyTree(init: "FA");
                break;
            case "PineTree":
                info = new PineTree(init: "FA");
                break;
            default:
                info = new LeafyTree(init: "FA");
                break;
        }

        // Stop Drawing the tree until it is fully generated
        drawTree = false;

        // Initialize the branch and leaf matrices, trunk matrix, and pointer stack
        branchMatrices = new List<Matrix4x4>();
        leafMatrices = new List<Matrix4x4>();
        trunkMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one * trunkScale);
        pointerStack = new Stack<stateData>();

        // Initialize the render parameters for branches, leaves, and trunk
        branchParams =  new RenderParams(branchMaterial) {receiveShadows = true, shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On };
        leafParams   =  new RenderParams(leafMaterial)   {receiveShadows = true, shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On };;
        trunkParams  =  new RenderParams(branchMaterial);

        // If not using custom settings, use the default settings from the info struct
        if (!useCustomSettings)
        {   
            generations = info.generations;
            rotationCoefficient = info.rotationCoefficient;
            sizeCoefficient = info.sizeCoefficient;
        }
        axiom = info.axiom;
        finalString = axiom;
        rules = info.rules;

        // Reset the branch and leaf counts and heights
        branchCount = 0;
        branchHeight = branch.bounds.size.z;
        leafHeight = leaf.bounds.size.z * leafScale;

        // Set the pointer's position and rotation to the base of the trunk
        pointer.position   = transform.position;
        pointer.rotation   = transform.rotation;
        pointer.localScale = transform.localScale;

        // Get the height of the trunk band and move the pointer to the top of the trunk
        float trunkHeight = trunk.bounds.size.z * trunkScale;
        pointer.Translate(Vector3.forward * trunkHeight, Space.Self);

        // Generate the final string, and apply the transformations to create the tree
        PassGenerations();
        ReadString();

        // Start drawing the tree
        drawTree = true;
    }

    void Update()
    {   
        if (drawTree)
        {   
            Graphics.RenderMeshInstanced(branchParams, branch, 0, branchMatrices);
            if (leafMatrices.Count > 0) Graphics.RenderMeshInstanced(leafParams, leaf, 0, leafMatrices);
            Graphics.RenderMesh(trunkParams, trunk, 0, trunkMatrix);

            Debug.Log($"branchMatrices length: {branchMatrices.Count}, leafMatrices length: {leafMatrices.Count}, FinalString length: {finalString.Length}");
            
        }
    }

    void ReadString()
    {
            // NOTE: Variables:
            //  X -> branch
            //  L -> Leaf
            //  [ -> Push Pos and Rotation
            //  ] -> Pop  Pos and Rotation
            //  + -> Scale up by sizeCoefficient
            //  - -> Scale down by sizeCoefficient
            //  < -> Rotate left  (x-axis) by rotationCoefficient
            //  > -> Rotate right (x-axis) by rotationCoefficient
            //  ^ -> Rotate up    (y-axis) by rotationCoefficient
            //  _ -> Rotate down  (y-axis) by rotationCoefficient
            
        foreach (char c in finalString)
        {   
            switch (c)
            {
                case 'L': 
                case 'l': { CreateLeaf(); } break;
                case 'X': { CreateBranch(); } break;
                case '[': { Push(); } break;
                case ']': { Pop(); } break;
                case '_': { OffsetRotation(0,  rotationCoefficient,  0); } break;   
                case '^': { OffsetRotation(0, -rotationCoefficient,  0); } break;   
                case '>': { OffsetRotation( rotationCoefficient,   0, 0); } break;  
                case '<': { OffsetRotation(-rotationCoefficient,   0, 0); } break;  
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

            if (finalString.Length > 50_000)
            {
                Debug.Log("Final string is too long, stopping at generation " + i);
                break;
            }
        }
    }



    string TransformChar(char c)
    {
        if (rules.ContainsKey(c)) {
            string result = rules[c][Random.Range(0, rules[c].Length)];
            Debug.Log($"Transforming {c} to {result}");
            return result;


        }
        else return c.ToString();
    }

    void CreateBranch()
    {
        Matrix4x4 transformMatrix = Matrix4x4.TRS(pointer.position, pointer.rotation, pointer.localScale);
        branchMatrices.Add(transformMatrix);

        pointer.Translate(Vector3.forward * branchHeight * pointer.localScale.z);

        branchCount++;  
    }

    void CreateLeaf()
    {   
        Quaternion rotSnapshot = pointer.rotation;
        Vector3 scale = Vector3.one * leafScale;

        int randomLeafCount = Random.Range(leafCount, 2*leafCount);
        float randomRotationX = Random.Range(30f, 60f);

        for (int i = 0; i < randomLeafCount; i++)
        {   
            Matrix4x4 leafMatrix = Matrix4x4.TRS(pointer.position, pointer.rotation, scale);
            leafMatrices.Add(leafMatrix);

            float randomRotationY = Random.Range(30f, 60f);
            float randomRotationZ = Random.Range(30f, 60f);

            pointer.Rotate(
                randomRotationX,
                randomRotationY,
                randomRotationZ,
                Space.Self);
        }

        pointer.rotation = rotSnapshot;
    }

    void CreateFlower()
    {
        // TODO: Make use of this function for creating flower petals
        Vector3 rotate = Vector3.zero;
        float rotation = 360f / leafCount;
        for (int i = 0; i < leafCount; i++){
            pointer.Rotate(0, 0, rotation, Space.Self);
            Matrix4x4 leafMatrix = Matrix4x4.TRS(pointer.position, pointer.rotation , Vector3.one * leafScale);
            leafMatrices.Add(leafMatrix);
        }
    }

    void Push()
    {
        pointerStack.Push(new stateData(){
            position = pointer.position,
            rotation = pointer.rotation,
            localScale = pointer.localScale
        });
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
        if (randomize)
        {
            x += Random.Range(-randomRotationRange, randomRotationRange);
            y += Random.Range(-randomRotationRange, randomRotationRange);
            z += Random.Range(-randomRotationRange, randomRotationRange);
        }

        pointer.Rotate(x, y, z, Space.Self);
    }

    void ChangeSize(float coef)
    {   
        pointer.localScale *= coef;
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
