using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private Grid grid;

    [SerializeField] GameObject terrainParent;  //Stores chunks for easy hierarchy in editor
    [SerializeField] GameObject[] terrainPrefabs;
    [SerializeField] UpdateUI updateUI;

    //recommended between 4 and 20
    [SerializeField] float magnification = 7f;

    //moving the terrain horizontally and vertically
    [SerializeField] int xOffset = 0;
    [SerializeField] int yOffset = 0;

    //Chunk size setter. Both X and Y axis
    [SerializeField] int chunkSize = 16;

    //This * chunk size. So setting this to 10 with the chunk size being 16 would result in a 160x160 grid map
    public int mapWidth = 10;
    public int mapHeight = 10;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    Dictionary<int, GameObject> tileset;
    List<Vector2> chunkLocations = new List<Vector2>();

    public event Action terrainGenerationComplete;
    public event Action<Vector2, int, int, int> terrainUpdated;

    public float SetMagnification { set { magnification = value; } }
    public int SetMapSize { set { mapWidth = value * 2; mapHeight = value * 2; } }
    public Dictionary<Vector2, TerrainChunk> GetTerrainChunks { get { return terrainChunks; } }
    public Dictionary<int, GameObject> GetTileset { get { return tileset; } }
    public List<Vector2> GetChunkLocations { get { return chunkLocations; } }
    public int GetChunkSize { get { return chunkSize; } }

    private void Awake()
    {
        grid = FindObjectOfType<Grid>();

        //CenterMap();
        //CreateTileset();
    }

    public void StartMapGeneration()
    {
        StartCoroutine(GenerateMap());
    }

    public void CenterMap()
    {
        terrainParent.transform.position = new Vector2(0 - (mapWidth * chunkSize) / 2 + 0.5f, 0 - (mapHeight * chunkSize) / 2 + 0.5f);
    }

    public void CreateTileset()
    {
        tileset = new Dictionary<int, GameObject>();
        for (int i = 0; i < terrainPrefabs.Length; i++)
        {
            tileset.Add(i, terrainPrefabs[i]);
        }
    }

    private IEnumerator GenerateMap()
    {
        updateUI.SetNeededProgress = mapHeight * mapWidth * 5;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                List<int> terrainValues = new List<int>();

                chunkLocations.Add(new Vector2(x * chunkSize, y * chunkSize));

                for (int i = 0; i < chunkSize; i++)
                {
                    for (int j = 0; j < chunkSize; j++)
                    {
                        int tileId = GetIdUsingPerlin(i + x * chunkSize, j + y * chunkSize);
                        terrainValues.Add(tileId);

                        Node node = grid.NodeFromWorldPoint(new Vector2(terrainParent.transform.position.x + i + x * chunkSize, terrainParent.transform.position.y + j + y * chunkSize));
                        node.SetWalkable = (tileId == 3) ? false : true;
                        node.movementPenalty = tileId * 5;
                        grid.UpdateNode(node);
                    }
                }

                TerrainChunk terrainChunk = new TerrainChunk(terrainValues);
                //Debug.Log(new Vector2(x * 16, y * 16));
                terrainChunks.Add(new Vector2(x * chunkSize, y * chunkSize), terrainChunk);

                updateUI.AddToProgress(1);

                yield return new WaitForSeconds(0.001f);
            }
        }
        yield return new WaitForSeconds(0.5f);

        terrainGenerationComplete?.Invoke();
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

    public void UpdateChunkByGridPos(int x, int y, int terrainIndex)
    {
        int chunkPosX = x / chunkSize;
        int chunkPosY = y / chunkSize;

        TerrainChunk terrainChunk = terrainChunks[new Vector2(chunkPosX * chunkSize, chunkPosY * chunkSize)];
        terrainChunk.UpdateTerrainValue(x % chunkSize * chunkSize + y % chunkSize, terrainIndex);
        terrainChunks[new Vector2(chunkPosX * chunkSize, chunkPosY * chunkSize)] = terrainChunk;

        terrainUpdated?.Invoke(new Vector2(chunkPosX * chunkSize, chunkPosY * chunkSize), x % chunkSize, y % chunkSize, terrainIndex);
    }

    public Vector2 GetTerrainChunkFromWorldPos(Vector2 _worldPos)
    {
        int divisorX = 0 - (mapWidth * chunkSize) / 2;
        int divisorY = 0 - (mapHeight * chunkSize) / 2;

        int x = Mathf.FloorToInt(_worldPos.x / chunkSize) * chunkSize - divisorX;
        int y = Mathf.FloorToInt(_worldPos.y / chunkSize) * chunkSize - divisorY;

        return new Vector2(x, y);
    }
}

public struct TerrainChunk
{
    List<int> terrainValues;

    public TerrainChunk(List<int> _terrainValues)
    {
        terrainValues = _terrainValues;
    }

    public List<int> GetTerrainValues { get { return terrainValues; } }
    
    public void UpdateTerrainValue(int index, int value)
    {
        terrainValues[index] = value;
    }
}
