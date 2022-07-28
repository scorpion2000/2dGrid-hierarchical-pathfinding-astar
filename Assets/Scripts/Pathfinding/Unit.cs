using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool drawGizmos;
    //public Transform target;
    float speed = 5f;
    Vector2[] path;
    Vector2[] clusterPath;
    int clusterIndex;
    int targetIndex;

    public event Action pathFinished;
    public event Action pathfindFailed;

    private void Start()
    {
        //FindPath(target);
    }

    public void FindPath(Vector2 position)
    {
        StopCoroutine("FollowPath");
        PathRequestManager.RequestPath(new PathRequest(transform.position, position, null, true, OnPathFound));
    }

    private void FollowClusterPath()
    {
        if (clusterIndex < clusterPath.Length)
        {
            PathRequestManager.RequestPath(new PathRequest(transform.position, clusterPath[clusterIndex], null, false, OnPathFound));
            clusterIndex++;
        } else
        {
            clusterIndex = 0;
            pathfindFailed?.Invoke();
        }
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
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        } else
        {
            //pathfindFailed?.Invoke();
            FollowClusterPath();
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
                    pathFinished?.Invoke();
                    yield break;
                }

                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
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
