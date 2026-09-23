using UnityEngine;
using UnityEngine.InputSystem;

public class TerrainGen : MonoBehaviour
{   
    [SerializeField] Mesh mesh;
    [SerializeField] float yHeight;
    [SerializeField] int xSize = 1000;
    [SerializeField] int zSize = 1000;

    Vector3[] vertices;
    int[] triangles;

    System.Random rand = new System.Random(42);

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateMesh();
    }

    void GenerateMesh()
    {
        GenerateVertices();
        GenerateTriangles();
        UpdateMesh();
    }

    void GenerateVertices()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        int idx = 0;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = generateNoiseHeight((float)x/xSize, (float)z/zSize);
                vertices[idx] = new Vector3(x, y, z);
                idx++;
            }
        }
    }

    float generateNoiseHeight(float x, float z)
    {
        float yVal = Mathf.PerlinNoise(x, z);

        if (yVal < 0.25) return yVal * -(yHeight*yHeight);
        else if (yVal < 0.5) return yVal;
        else if (yVal < 0.7) return yVal * yHeight * yHeight;
        else if (yVal < 0.9) return yVal * yHeight;
        else return yVal;
    }

    void GenerateTriangles()
    {
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void Update()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame) GenerateMesh();
    }
}
