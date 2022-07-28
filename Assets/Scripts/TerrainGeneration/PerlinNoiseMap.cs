using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseMap : MonoBehaviour
{
    Grid pathfindingGrid;

    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tileGroups;
    public GameObject prefabPlains;
    public GameObject prefabForest;
    public GameObject prefabHills;
    public GameObject prefabMountains;

    //This * chunk size. So setting this to 10 with the chunk size being 16 would result in a 160x160 grid map
    public int mapWidth = 10;
    public int mapHeight = 10;

    List<List<int>> noiseGrid = new List<List<int>>();
    List<List<GameObject>> tileGrid = new List<List<GameObject>>();

    //recommended between 4 and 20
    [SerializeField] float magnification = 7f;

    //moving the terrain horizontally and vertically
    [SerializeField] int xOffset = 0;
    [SerializeField] int yOffset = 0;

    //Chunk size setter. Both X and Y axis
    [SerializeField] int chunkSize = 16;
    public int GetChunkSize { get { return chunkSize; } }

    void Start()
    {
        pathfindingGrid = FindObjectOfType<Grid>();
        pathfindingGrid.SetGridSize(mapWidth * chunkSize, mapHeight * chunkSize);

        CenterMap();
        CreateTileset();
        CreateTileGroups();
        StartCoroutine(GenerateMap());
    }

    private void CenterMap()
    {
        gameObject.transform.position = new Vector2(0 - (mapWidth * chunkSize)/2 + 0.5f, 0 - (mapHeight * chunkSize)/ 2 + 0.5f);
    }

    private void CreateTileset()
    {
        tileset = new Dictionary<int, GameObject>();
        tileset.Add(0, prefabPlains);
        tileset.Add(1, prefabForest);
        tileset.Add(2, prefabHills);
        tileset.Add(3, prefabMountains);
    }

    private void CreateTileGroups()
    {
        tileGroups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefabPair in tileset)
        {
            GameObject tileGroup = new GameObject(prefabPair.Value.name);
            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector2(0,0);
            tileGroups.Add(prefabPair.Key, tileGroup);
        }
    }

    private IEnumerator GenerateMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int i = 0; i < chunkSize; i++)
                {

                    noiseGrid.Add(new List<int>());
                    tileGrid.Add(new List<GameObject>());

                    for (int j = 0; j < chunkSize; j++)
                    {
                        int tileId = GetIdUsingPerlin(i + x * chunkSize, j + y * chunkSize);
                        noiseGrid[x].Add(tileId);
                        CreateTile(tileId, i + x* chunkSize, j + y * chunkSize);
                    }
                }

                //CombineMeshes();

                yield return new WaitForSeconds(0.01f);
            }
        }

        StartCoroutine(DelayedGridSetup());
        yield return new WaitForSeconds(0.5f);
    }

    private void CombineMeshes()
    {

    }

    private int GetIdUsingPerlin(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise(
            (x - xOffset) / magnification,
            (y - yOffset) / magnification
        );

        float clampPerlin = Mathf.Clamp(rawPerlin, 0f, 1f);
        float scaledPerlin = clampPerlin * tileset.Count;
        if (scaledPerlin == tileset.Count)
            scaledPerlin = tileset.Count - 1;

        return Mathf.FloorToInt(scaledPerlin);
    }

    private void CreateTile(int tileId, int x, int y)
    {
        GameObject tilePrefab = tileset[tileId];
        GameObject tileGroup = tileGroups[tileId];
        GameObject tile = Instantiate(tilePrefab, tileGroup.transform);

        tile.name = string.Format("tile_x{0}_y{1}",x,y);
        //tile.layer = tileId + 9;
        tile.transform.localPosition = new Vector2(x, y);
        tileGrid[x].Add(tile);
    }

    private IEnumerator DelayedGridSetup()
    {
        yield return new WaitForSeconds(2f);
        pathfindingGrid.GridSetup();
    }
}
