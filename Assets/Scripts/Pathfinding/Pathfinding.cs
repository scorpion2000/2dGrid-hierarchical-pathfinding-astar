using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    ClusterManager clusterManager;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        clusterManager = GetComponent<ClusterManager>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        Vector2[] waypoints = new Vector2[0];
        float pathCost = 0;
        bool pathSuccess = false;
        Node[,] newPathGrid = (Node[,])grid.GetGrid.Clone();
        if (request.nodesToUse != null)
            newPathGrid = request.nodesToUse;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        startNode.gCost = 0;

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closeSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                /*
                 * Replaced with HEAP optimization
                 * for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);*/
                closeSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    //sw.Stop();
                    //UnityEngine.Debug.Log("Path found in " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeigbours(currentNode, newPathGrid))
                {
                    if (!neighbour.walkable || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        if (pathSuccess)
        {
            //waypoints = RetracePath(startNode, targetNode);
            RetracedPath retracedPath = RetracePath(startNode, targetNode);
            waypoints = retracedPath.waypoints;
            pathCost = retracedPath.pathCost;
            pathSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSuccess, pathCost, request.clusterSearch, request.callback));
    }

    private RetracedPath RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        RetracedPath retracedPath = new RetracedPath();
        float pathCost = endNode.gCost;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);

        retracedPath.pathCost = pathCost;
        retracedPath.waypoints = SimplifyPath(path);
        Array.Reverse(retracedPath.waypoints);
        return retracedPath;
    }

    private Vector2[] SimplifyPath(List<Node> path)
    {
        List<Vector2> waypoints = new List<Vector2>();
        Vector2 directionOld = Vector2.zero;
        int penaltyOld = 0;
        waypoints.Add(path[0].worldPos);

        for (int i = 1; i < path.Count; i++)
        {
            int penaltyNew = path[i].movementPenalty;
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld || penaltyNew != penaltyOld)
            {
                waypoints.Add(path[i].worldPos);
            }
            directionOld = directionNew;
            penaltyOld = penaltyNew;
        }
        if (!waypoints.Contains(path[path.Count - 1].worldPos))
            waypoints.Add(path[path.Count - 1].worldPos);
        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        /*int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstY);*/

        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}

public struct RetracedPath {
    public Vector2[] waypoints;
    public float pathCost;
}
