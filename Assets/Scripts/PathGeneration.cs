using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    Unit unit;
    //PerlinNoiseMap perlinNoiseMap;
    TerrainGenerator terrain;

    int worldHeight;
    int worldBottom;
    int worldLeft;
    int worldWidth;

    void Awake()
    {
        terrain = FindObjectOfType<TerrainGenerator>();
        unit = GetComponent<Unit>();

        terrain.terrainGenerationComplete += StartPathfinding;
        unit.pathfindFailed += FindNewTarget;
        unit.pathFinished += FindNewTarget;

        worldHeight = 0 + terrain.mapHeight * terrain.GetChunkSize;
        worldWidth = 0 + terrain.mapWidth * terrain.GetChunkSize;
    }

    public void StartPathfinding()
    {
        terrain.terrainGenerationComplete -= StartPathfinding;
        FindNewTarget();
        //StartCoroutine(CreateNewTarget());
    }

    private void FindNewTarget()
    {
        Vector2 pathTo = new Vector2(Random.Range(0 - worldHeight / 2, worldHeight/2), Random.Range(0 - worldWidth / 2, worldWidth/2));
        while (pathTo == (Vector2)transform.position)
        {
            pathTo = new Vector2(Random.Range(0 - worldHeight / 2, worldHeight / 2), Random.Range(0 - worldWidth / 2, worldWidth / 2));
        }

        //Debug.Log(pathTo);
        unit.FindPath(pathTo);
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
