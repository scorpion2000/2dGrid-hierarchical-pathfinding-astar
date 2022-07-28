using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance;
    Pathfinding pathfinding;

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock(results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success, result.pathCost, result.clusterSearch);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock(results)
        {
            results.Enqueue(result);
        }
    }
}

public struct PathResult
{
    public Vector2[] path;
    public bool success;
    public float pathCost;
    public Action<Vector2[], bool, float, bool> callback;
    public bool clusterSearch;

    public PathResult(Vector2[] _path, bool _success, float _pathCost, bool _clusterSearch, Action<Vector2[], bool, float, bool> _callback)
    {
        path = _path;
        success = _success;
        callback = _callback;
        clusterSearch = _clusterSearch;
        pathCost = _pathCost;
    }
}

public struct PathRequest
{
    public Vector2 pathStart;
    public Vector2 pathEnd;
    public Action<Vector2[], bool, float, bool> callback;
    public Node[,] nodesToUse;
    public bool clusterSearch;

    public PathRequest(Vector2 _start, Vector2 _end, Node[,] _nodesToUse, bool _clusterSearch, Action<Vector2[], bool, float, bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        nodesToUse = _nodesToUse;
        clusterSearch = _clusterSearch;
        callback = _callback;
    }
}