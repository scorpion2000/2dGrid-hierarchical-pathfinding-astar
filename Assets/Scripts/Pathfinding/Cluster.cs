using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    Dictionary<Node, EntranceNode> entranceNodes = new Dictionary<Node, EntranceNode>();
    List<Entrance> entrances = new List<Entrance>();

    public int posX;
    public int posY;

    public Vector2 gridPosBtmLeft;
    public Vector2 gridPosTopRight;

    public Cluster(int _posX, int _posY, Vector2 _gridPosBtmLeft, Vector2 _gridPosTopRight)
    {
        posX = _posX;
        posY = _posY;
        gridPosBtmLeft = _gridPosBtmLeft;
        gridPosTopRight = _gridPosTopRight;
}

    public Vector2 GetClusterVectorPos { get { return new Vector2(posX, posY); } }
    public List<Entrance> GetClusterEntrances { get { return entrances; } }
    //for gizmos draws
    public List<Node> GetEntranceNodes { 
        get {
            List<Node> entranceNodeList = new List<Node>();
            foreach (var entNode in entranceNodes)
            {
                entranceNodeList.Add(entNode.Key);
            }
            return entranceNodeList;
        } 
    }

    public List<Vector2> GetClusterEntranceVectors
    {
        get
        {
            List<Vector2> entranceVectors = new List<Vector2>();
            foreach (Entrance entrance in entrances)
            {
                foreach (Vector2 vector in entrance.entrancePositions)
                {
                    entranceVectors.Add(vector);
                }
            }
            return entranceVectors;
        }
    }

    public Node AddEntranceNode(Node node)
    {
        Node newNode = new Node(true, node.worldPos, node.gridX, node.gridY, 0, 1);
        EntranceNode newEntranceNode = new EntranceNode();
        newEntranceNode.entrace = newNode;
        newEntranceNode.connectedNodeValues = new Dictionary<Node, float>();

        entranceNodes.Add(newNode, newEntranceNode);
        return newNode;
    }

    public void ConnectSymEntrance(Node nodeA, Node nodeB)
    {
        EntranceNode entranceNode = entranceNodes[nodeA];
        entranceNode.symEntrance = nodeB;
        entranceNodes[nodeA] = entranceNode;
    }

    public void MakeNodeConnection(Node[] nodes, Node[] connectToNodes, float movementCost)
    {
        EntranceNode entranceNode;
        int rnd = Random.Range(0, 100000);
        for (int i = 0; i < nodes.Length; i++)
        {
            entranceNode = entranceNodes[nodes[0]];
            for (int y = 0; y < connectToNodes.Length; y++)
            {
                if (!entranceNode.connectedNodeValues.ContainsKey(connectToNodes[y]))
                {
                    entranceNode.connectedNodeValues.Add(connectToNodes[y], movementCost);
                    entranceNodes[nodes[i]] = entranceNode;
                    return;
                }
            }
        }
    }

    public void RemoveEntrance(Node node)
    {
        foreach (var entrance in entranceNodes)
        {
            if (entrance.Key == node) continue;

            EntranceNode entranceNode = entranceNodes[entrance.Key];
            //entranceNode.connectedNodeValues.Remove(node.worldPos);
            entranceNodes[entrance.Key] = entranceNode;
        }

        entranceNodes.Remove(node);
    }

    public void RegisterNewEntrance(Entrance entrance)
    {
        entrances.Add(entrance);
    }

    public void RegisterEntranceNodeToEntrance(Entrance entrance, Node node)
    {
        if (!entrances.Contains(entrance) || !entranceNodes.ContainsKey(node)) return;
        entrance.existingNodesInEntrance.Add(node);
    }

    public Node[] CheckForExistingEntranceNodeByPos(Vector2 position)
    {
        List<Node> nodeList = new List<Node>();
        foreach (var entranceNodes in entranceNodes)
        {
            if (entranceNodes.Key.worldPos == position) nodeList.Add(entranceNodes.Key);
        }
        //Debug.Log(nodeList.Count);
        return nodeList.ToArray();
    }

    public List<Node> GetEntranceConnections(Node entranceNode)
    {
        List<Node> nodes = new List<Node>();

        foreach (var item in entranceNodes[entranceNode].connectedNodeValues)
        {
            nodes.Add(item.Key);
        }
        return nodes;
    }

    public float GetConnectionCost(Node fromNode, Node toNode)
    {
        //Debug.Log(entranceNodes[fromNode].connectedNodeValues.ContainsKey(toNode));
        return entranceNodes[fromNode].connectedNodeValues[toNode];
    }

    public Node GetClusterSymEntrance(Node node)
    {
        return entranceNodes[node].symEntrance;
    }

    public Entrance GetEntranceByNeighbourNode(Node neighbourNode)
    {
        foreach (Entrance entrance in entrances)
        {
            if (entrance.symEntrancePositions.Contains(neighbourNode.worldPos)) return entrance;
        }
        return new Entrance();
    }

    public bool FindNodeInCluster(Node node)
    {
        return entranceNodes.ContainsKey(node);
    }
}

public struct EntranceNode
{
    public Node entrace;
    public Node symEntrance;
    public Dictionary<Node, float> connectedNodeValues;
}

public struct Entrance
{
    public int position;    //0 = left, 1 = right, 2 = down, 3 = up
    public List<Vector2> entrancePositions;
    public List<Vector2> symEntrancePositions;
    public List<Node> existingNodesInEntrance;
}