using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public bool clusterVerical;
    public Vector2 worldPos;
    public int gridX, gridY;
    public int movementPenalty;
    public int clusterLevel;

    public int gCost, hCost;
    public Node parent;
    int heapIndex;

    public int HeapIndex { get { return heapIndex; } set { heapIndex = value; } }
    public int fCost { get { return gCost + hCost; } }
    public bool SetWalkable { set { walkable = value; } }
    public bool CheckClusterVertical { get { return clusterVerical; } }

    public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY, int _penalty, int _clusterLevel)
    {
        walkable = _walkable;
        worldPos = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
        clusterLevel = _clusterLevel;
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}
