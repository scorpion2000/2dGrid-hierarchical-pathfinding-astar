using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    Dictionary<Node, EntranceNode> entranceNodes = new Dictionary<Node, EntranceNode>();
    Dictionary<Vector2[], Vector2[]> pathCache = new Dictionary<Vector2[], Vector2[]>();
    List<Vector2[]> cacheKeys = new List<Vector2[]>();
    List<Entrance> entrances = new List<Entrance>();
    Node[,] clusterNodeList;

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
    public Node[,] GetClusterNodeList { get { return clusterNodeList; } }
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
        List<Node> keys = new List<Node>(entranceNodes.Keys);
        foreach (Node key in keys)
        {
            if (key == node) continue;

            EntranceNode entranceNode = entranceNodes[key];
            entranceNode.connectedNodeValues.Remove(node);
            entranceNodes[key] = entranceNode;
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

    public void RemoveAllNodesFromEntrance(Entrance entrance)
    {
        foreach (Node node in entrance.existingNodesInEntrance)
        {
            RemoveEntrance(node);
        }

        entrance.existingNodesInEntrance.Clear();
    }

    public void UpdateClusterNodeList(Node[,] nodes)
    {
        clusterNodeList = nodes;
    }
    public void UpdatePathCache(Vector2[] pathFromTo, Vector2[] path)
    {
        if (pathCache.Count >= 3)
        {
            pathCache.Remove(cacheKeys[0]);
            cacheKeys.Remove(cacheKeys[0]);
        }

        pathCache.Add(pathFromTo, path);
        cacheKeys.Add(pathFromTo);
    }

    public Node[] GetExistingEntranceNodeByPos(Vector2 position)
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

    public List<Entrance> GetEntrancesByPos(Vector2 pos)
    {
        List<Entrance> entranceList = new List<Entrance>();
        foreach (Entrance entrance in entrances)
        {
            if (entrance.entrancePositions.Contains(pos))
                entranceList.Add(entrance);
        }

        return entranceList;
    }

    public Entrance GetEntranceByNode(Node node)
    {
        foreach (Entrance entrance in entrances)
        {
            if (entrance.existingNodesInEntrance.Contains(node))
                return entrance;
        }

        return new Entrance();
    }

    public Vector2[] GetPathFromCache(Vector2[] pathFromTo)
    {
        if (pathCache.ContainsKey(pathFromTo))
            return pathCache[pathFromTo];
        else
            return null;
    }

    public Node GetNodeBySymNodePos(Vector2 fromPos, Vector2 toPos)
    {
        foreach (Node node in entranceNodes.Keys)
        {
            if (node.worldPos == toPos && entranceNodes[node].symEntrance.worldPos == fromPos)
                return node;
        }

        return null;
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