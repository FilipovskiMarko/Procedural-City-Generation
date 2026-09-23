using System.Collections.Generic;
using UnityEngine;

enum blockType {
    EMPTY,
    BUILDING,
    PARK            
};
struct Block
{   
    public blockType type;

    public Block(blockType type)
    {   
        this.type = type;
    }

    public void CreateBuilding(int zOff, int xOff, GameObject building, float size, float height, Transform parent, List<GameObject> objs)
    {
        Vector3 offset = new Vector3(xOff * size, 0, zOff * -size);
        float randomRot = Random.Range(0, 3 + 1) * 90f;
        objs.Add(GameObject.Instantiate(building, parent.position + offset, Quaternion.Euler(270, randomRot, 0), parent));
    }

    public void Reinitialize()
    {
        type = blockType.EMPTY;
    }

    public void Print()
    {
        Debug.Log("TYPE: " + type);
    }

}
struct Crossroad
{
    public bool hasCrossroad;
    public bool hasRoadDown; 
    public bool hasRoadRight;
    
    public Crossroad(bool hasCrossroad)
    {
        this.hasRoadDown = true;
        this.hasRoadRight = true;
        this.hasCrossroad = true;

    }

    public void CreateRoads(int zOff, int xOff, RoadManager roadManager, float unitSize, float roadSize, Transform parent)
    {   
        // TODO: Simplify scaling issue, i don't know why the size is so small
        Vector3 scale = new Vector3(roadSize / 10f, 1, roadSize);

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
        if (hasCrossroad) { 
            Vector3 offset = new Vector3((xOff - 0.5f) * unitSize, 0, (-zOff + 0.5f) * unitSize);
            scale = new Vector3(roadSize / 10f, 1, roadSize / 10f);
            roadManager.AddCrossroad(parent.position + offset, Quaternion.identity, scale);
        }
    }

    public void Reinitialize()
    {   
        hasCrossroad = true;
        hasRoadDown = true;
        hasRoadRight = true;
    }
}
struct stateData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

}


