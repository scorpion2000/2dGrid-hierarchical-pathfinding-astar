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
        clusterManager = FindObjectOfType<ClusterManager>();
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

        if (request.clusterSearch && clusterManager.CheckIfNeighbourCluster(startNode, targetNode))
        {
            Vector2[] newWaypoints = new Vector2[2];
            newWaypoints[0] = startNode.worldPos;
            newWaypoints[1] = targetNode.worldPos;
            callback(new PathResult(newWaypoints, true, 0, request.clusterSearch, request.callback));
            return;
        }

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

                /*if (request.clusterSearch)
                {
                    if (clusterManager.GetClusterByNode(currentNode))
                }*/

                if (currentNode == targetNode)
                {
                    //sw.Stop();
                    //UnityEngine.Debug.Log("Path found in " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                List<Node> neighbourNodeList;
                if (request.clusterSearch)
                    neighbourNodeList = clusterManager.GetConnectedNodes(currentNode, targetNode);
                else
                    neighbourNodeList = grid.GetNeigbours(currentNode, newPathGrid);

                foreach (Node neighbour in neighbourNodeList)
                {
                    if (neighbour != targetNode)
                        if (request.clusterSearch && closeSet.Contains(clusterManager.GetClusterByNode(neighbour).GetClusterSymEntrance(neighbour)))
                            continue;

                    if (!neighbour.walkable || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour;

                    if (request.clusterSearch)
                    {
                        if (currentNode == startNode || neighbour == targetNode)
                            newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                        else
                            newCostToNeighbour = currentNode.gCost + Mathf.FloorToInt(clusterManager.GetClusterByNode(currentNode).GetConnectionCost(currentNode, neighbour));
                    } else
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    }

                    Node neighbourToAdd;
                    if (request.clusterSearch && neighbour != targetNode)
                        neighbourToAdd = clusterManager.GetClusterByNode(neighbour).GetClusterSymEntrance(neighbour);
                    else
                        neighbourToAdd = neighbour;


                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbourToAdd))
                    {
                        closeSet.Add(neighbour);
                        neighbourToAdd.gCost = newCostToNeighbour;
                        neighbourToAdd.hCost = GetDistance(neighbour, targetNode);
                        neighbourToAdd.parent = currentNode;

                        if (!openSet.Contains(neighbourToAdd))
                            openSet.Add(neighbourToAdd);
                        else
                            openSet.UpdateItem(neighbourToAdd);
                    }
                }
            }
        }
        if (pathSuccess)
        {
            //waypoints = RetracePath(startNode, targetNode);
            RetracedPath retracedPath = RetracePath(startNode, targetNode, request.clusterSearch);
            waypoints = retracedPath.waypoints;
            pathCost = retracedPath.pathCost;
            pathSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSuccess, pathCost, request.clusterSearch, request.callback));
    }

    private RetracedPath RetracePath(Node startNode, Node endNode, bool simplify)
    {
        List<Node> path = new List<Node>();
        List<Vector2> vectors = new List<Vector2>();
        Node currentNode = endNode;
        RetracedPath retracedPath = new RetracedPath();
        float pathCost = endNode.gCost;

        while (currentNode != startNode)
        {
            if (!simplify)
                path.Add(currentNode);
            else
                vectors.Add(currentNode.worldPos);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        if (simplify)
            Debug.Log(vectors.Count);

        retracedPath.pathCost = pathCost;
        if (!simplify)
            retracedPath.waypoints = SimplifyPath(path);
        else
            retracedPath.waypoints = vectors.ToArray();
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
