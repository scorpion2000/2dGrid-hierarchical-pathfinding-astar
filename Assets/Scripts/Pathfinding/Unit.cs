using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    ClusterManager clusterManager;
    ChunkSpotter chunkSpotter;
    Grid grid;

    public bool drawGizmos;
    //public Transform target;
    float speed = 5f;
    Vector2[] path;
    Vector2[] clusterPath;
    Vector2 heading;
    int clusterIndex;
    int targetIndex;
    float penalty;
    Cluster currentCluster;

    public event Action pathFinished;
    public event Action pathfindFailed;

    private void Start()
    {
        clusterManager = FindObjectOfType<ClusterManager>();
        grid = FindObjectOfType<Grid>();
        chunkSpotter = FindObjectOfType<ChunkSpotter>();
        clusterManager.ClusterUpdating += HandleClusterUpdating;
        clusterManager.ClusterUpdated += HandleClusterUpdated;
        //FindPath(target);
    }

    public IEnumerator FindPath(Vector2 position)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1));
        StopCoroutine("FollowPath");
        heading = position;
        PathRequestManager.RequestPath(new PathRequest(transform.position, position, null, true, OnPathFound));
    }

    private void FollowClusterPath()
    {
        if (clusterIndex < clusterPath.Length)
        {
            currentCluster = clusterManager.GetClusterByPos(transform.position);
            Vector2[] pathFromTo = new Vector2[2] { transform.position, clusterPath[clusterIndex] };
            Vector2[] newPath = clusterManager.GetPathFromCluster(currentCluster, pathFromTo);
            if (newPath == null)
                PathRequestManager.RequestPath(new PathRequest(transform.position, clusterPath[clusterIndex], null, false, OnPathFound));
            else
                OnPathFound(newPath);
            clusterIndex++;
        } else
        {
            clusterIndex = 0;
            pathFinished?.Invoke();
        }
    }

    public void OnPathFound(Vector2[] newPath)
    {
        path = newPath;
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    public void OnPathFound(Vector2[] newPath, bool pathSuccessful, float pathCost, bool clusterSearch)
    {
        if (pathSuccessful)
        {
            if (clusterSearch)
            {
                clusterPath = newPath;
                FollowClusterPath();
            } else
            {
                path = newPath;
                
                Vector2[] pathFromTo = new Vector2[2] { path[0], path[path.Length - 1] };
                clusterManager.UpdateClusterPathCache(currentCluster, pathFromTo, path);

                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        } else
        {
            pathfindFailed?.Invoke();
        }
    }

    IEnumerator FollowPath()
    {
        Vector2 currentWaypoint = path[0];
        targetIndex = 0;

        while (true)
        {
            if ((Vector2)transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    FollowClusterPath();
                    yield break;
                }
                currentWaypoint = path[targetIndex];
                penalty = grid.NodeFromWorldPoint(currentWaypoint).movementPenalty;
                if (penalty < 1) penalty = 1f;
            }

            transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, speed / penalty * Time.deltaTime);
            yield return null;
        }
    }

    private void HandleClusterUpdating(Cluster cluster)
    {
        if (currentCluster == cluster)
            StopCoroutine("FollowPath");
    }

    private void HandleClusterUpdated(Cluster cluster)
    {
        if (currentCluster == cluster)
        {
            StopCoroutine("FollowPath");
            StartCoroutine(FindPath(heading));
        }
        /*else
        {
            foreach (Vector2 vector in clusterPath)
            {
                if (clusterManager.GetClusterByPos(vector) == cluster)
                {
                    StopCoroutine("FollowPath");
                    FindPath(heading);
                }
            }
        }*/
    }

    // Checks if the unit needs to be rendered or not.
    private void RenderCheck(Vector2 currentPos, Vector2 nextPos)
    {
        if (chunkSpotter.IsInRenderedChunk(currentPos) || chunkSpotter.IsInRenderedChunk(nextPos))
            GetComponent<SpriteRenderer>().enabled = true;
        else
            GetComponent<SpriteRenderer>().enabled = false;
    }

    public void OnDrawGizmos()
    {
        if (path != null && drawGizmos)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(path[i], Vector2.one / 2);

                if (i == targetIndex)
                    Gizmos.DrawLine(transform.position, path[i]);
                else
                    Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
}

/*public struct PathStruct {
    Vector2 waypoint;
    float penalty;

    public PathStruct(Vector2 _waypoint, float _penalty)
    {
        waypoint = _waypoint;
        penalty = _penalty;
    }
}*/