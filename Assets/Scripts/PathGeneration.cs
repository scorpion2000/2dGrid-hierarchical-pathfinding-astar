using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    Unit unit;
    //PerlinNoiseMap perlinNoiseMap;
    TerrainGenerator terrain;
    ClusterManager clusterManager;

    int worldHeight;
    int worldBottom;
    int worldLeft;
    int worldWidth;

    void Awake()
    {
        terrain = FindObjectOfType<TerrainGenerator>();
        clusterManager = FindObjectOfType<ClusterManager>();
        unit = GetComponent<Unit>();

        clusterManager.ClusteringComplete += StartPathfinding;
        unit.pathfindFailed += StartPathfinding;
        unit.pathFinished += StartPathfinding;

        worldHeight = 0 + terrain.mapHeight * terrain.GetChunkSize;
        worldWidth = 0 + terrain.mapWidth * terrain.GetChunkSize;
    }

    public void StartPathfinding()
    {
        clusterManager.ClusteringComplete -= StartPathfinding;
        StartCoroutine(FindNewTarget());
        //StartCoroutine(CreateNewTarget());
    }

    private IEnumerator FindNewTarget()
    {
        yield return new WaitForSeconds(Random.Range(1, 6));

        Vector2 pathTo = new Vector2(Random.Range(0 - worldHeight / 2, worldHeight/2), Random.Range(0 - worldWidth / 2, worldWidth/2));
        while (pathTo == (Vector2)transform.position)
        {
            pathTo = new Vector2(Random.Range(0 - worldHeight / 2, worldHeight / 2), Random.Range(0 - worldWidth / 2, worldWidth / 2));
        }

        StartCoroutine(unit.FindPath(pathTo));
    }

    /*private IEnumerator CreateNewTarget()
    {
        while (true)
        {
            unit.FindPath(new Vector2(Random.Range(worldTop, worldBottom), Random.Range(worldLeft, worldRight)));

            yield return new WaitForSeconds(Random.Range(10f, 30f));
        }
    }*/
}
