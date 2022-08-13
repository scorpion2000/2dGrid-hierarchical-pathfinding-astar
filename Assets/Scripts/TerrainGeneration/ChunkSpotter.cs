using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSpotter : MonoBehaviour
{
    [SerializeField] Camera camToCheck; //Holds the camera to evaluate (in case it differs from the main camera)
    [SerializeField] GameObject chunkInstance;  //Stores chunk gameobject to be instentiated
    [SerializeField] int renderChunksVertical = 1;
    [SerializeField] int renderChunksHorizontal = 1;
    //[SerializeField] float renderDistance = 32; //A maximum render distance. In case our chunk size is too big, so we don't render chunks from far away
    [SerializeField] float chunkSpotFrequency = 0.25f;

    TerrainGenerator terrainGenerator;
    ChunkRenderer chunkRenderer;
    List<Vector2> allChunks = new List<Vector2>();
    List<Vector2> renderedChunks = new List<Vector2>();
    List<Vector2> evaluatedChunks = new List<Vector2>();
    Dictionary<Vector2, GameObject> chunkObjects = new Dictionary<Vector2, GameObject>();

    private int chunkSize;
    private int mapSizeX;
    private int mapSizeY;

    /*GameObject newChunk = Instantiate(chunkInstance, terrainParent.transform);
    newChunk.transform.localPosition = new Vector2(x * 16, y * 16);
    newChunk.name = x + ";" + y;
    chunkObjects.Add(newChunk);*/

    private void Awake()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        chunkRenderer = GetComponent<ChunkRenderer>();

        terrainGenerator.terrainGenerationComplete += StartChunkSpotting;
        terrainGenerator.terrainUpdated += HandleTerrainChange;
    }

    private void StartChunkSpotting()
    {
        terrainGenerator.terrainGenerationComplete -= StartChunkSpotting;

        chunkSize = terrainGenerator.GetChunkSize;
        mapSizeX = terrainGenerator.mapWidth;
        mapSizeY = terrainGenerator.mapHeight;

        chunkRenderer.SetChunkSize = terrainGenerator.GetChunkSize;
        chunkRenderer.SetTileset = terrainGenerator.GetTileset;

        allChunks = terrainGenerator.GetChunkLocations;

        StartCoroutine(EvaluateChunks());
    }

    private IEnumerator EvaluateChunks()
    {
        Vector2 activeChunk = new Vector2();
        Vector2 newActiveChunk = new Vector2();

        while (true)
        {
            newActiveChunk = terrainGenerator.GetTerrainChunkFromWorldPos(camToCheck.transform.position);

            if (activeChunk != newActiveChunk)
            {
                //Vector2 chunkCameraPos = (Vector2)camToCheck.transform.position + new Vector2(mapSizeX * chunkSize / 2, mapSizeY * chunkSize / 2);
                for (int i = 0 - renderChunksHorizontal; i <= renderChunksHorizontal; i++)
                {
                    for (int y = 0 - renderChunksVertical; y <= renderChunksVertical; y++)
                    {
                        //if ((i == 0 - renderChunksHorizontal || i == renderChunksHorizontal) && (y == 0 - renderChunksVertical || y == renderChunksVertical)) continue;
                        Vector2 evaluatedChunk = terrainGenerator.GetTerrainChunkFromWorldPos((Vector2)camToCheck.transform.position + new Vector2(i * chunkSize, y * chunkSize));
                        if (allChunks.Contains(evaluatedChunk) /*&& Vector2.Distance(evaluatedChunk + new Vector2(chunkSize/2, chunkSize/2), chunkCameraPos) <= renderDistance*/)
                            evaluatedChunks.Add(evaluatedChunk);
                    }
                }

                List<Vector2> toRemove = new List<Vector2>();
                foreach (Vector2 chunk in renderedChunks)
                {
                    if (!evaluatedChunks.Contains(chunk))
                    {
                        GameObject chunkParent = chunkObjects[chunk];
                        chunkRenderer.DeleteChunkObjects(chunkParent);
                        chunkObjects.Remove(chunk);
                        toRemove.Add(chunk);
                    }
                }

                foreach (Vector2 chunk in toRemove) renderedChunks.Remove(chunk);

                foreach (Vector2 chunk in evaluatedChunks)
                {
                    if (!renderedChunks.Contains(chunk))
                    {
                        GameObject chunkParent = Instantiate(chunkInstance, transform);
                        chunkParent.name = chunk.x + " ; " + chunk.y;
                        chunkParent.transform.localPosition = chunk;
                        chunkObjects.Add(chunk, chunkParent);

                        chunkRenderer.GenerateChunkObjects(
                            terrainGenerator.GetTerrainChunks[chunk].GetTerrainValues,
                            chunkParent
                        );

                        renderedChunks.Add(chunk);
                    }
                }

                evaluatedChunks.Clear();

                activeChunk = newActiveChunk;
            }

            yield return new WaitForSeconds(chunkSpotFrequency);
        }
    }

    private void HandleTerrainChange(Vector2 pos, int chunkX, int chunkY, int terrainType)
    {
        GameObject chunkParent = chunkObjects[pos];
        chunkRenderer.UpdateChunkObject(chunkParent, chunkX, chunkY, terrainType);
    }

    public bool IsInRenderedChunk(Vector2 pos)
    {
        if (renderedChunks.Contains(terrainGenerator.GetTerrainChunkFromWorldPos(pos)))
            return true;
        else
            return false;
    }


    // Old method
    /*private IEnumerator EvaluateChunks()
    {
        while (true)
        {
            foreach (Vector2 chunk in openChunks)
            {
                if (Vector2.Distance(chunk + (Vector2)transform.position + new Vector2(chunkSize / 2, chunkSize / 2), camToCheck.transform.position) < camDistance)
                {
                    GameObject chunkParent = Instantiate(chunkInstance, transform);
                    chunkParent.name = chunk.x + " ; " + chunk.y;
                    chunkParent.transform.localPosition = chunk;
                    chunkObjects.Add(chunk, chunkParent);

                    chunkRenderer.GenerateChunkObjects(
                        terrainGenerator.GetTerrainChunks[chunk].GetTerrainValues,
                        chunkParent
                    );
                    Debug.Log(chunk);
                    evaluatedChunks.Add(chunk);
                    yield return new WaitForSeconds(0.025f);
                }
            }

            //ExchangeVectors(evaluatedChunks, closedChunks, openChunks);
            foreach (Vector2 chunk in evaluatedChunks)
            {
                closedChunks.Add(chunk);
                openChunks.Remove(chunk);
            }

            evaluatedChunks.Clear();

            foreach (Vector2 chunk in closedChunks)
            {
                if (Vector2.Distance(chunk + (Vector2)transform.position + new Vector2(chunkSize / 2, chunkSize / 2), camToCheck.transform.position) > camDistance)
                {
                    GameObject chunkParent = chunkObjects[chunk];
                    chunkObjects.Remove(chunk);
                    chunkRenderer.DeleteChunkObjects(chunkParent);
                    evaluatedChunks.Add(chunk);
                    yield return new WaitForSeconds(0.025f);
                }
            }

            //ExchangeVectors(evaluatedChunks, openChunks, closedChunks);
            foreach (Vector2 chunk in evaluatedChunks)
            {
                openChunks.Add(chunk);
                closedChunks.Remove(chunk);
            }

            evaluatedChunks.Clear();

            yield return new WaitForSeconds(chunkSpotFrequency);
        }
    }*/

    /*private void ExchangeVectors(List<Vector2> toExchange, List<Vector2> from, List<Vector2> to)
    {
        foreach (Vector2 vector in toExchange)
        {
            to.Add(vector);
            from.Remove(vector);
        }

        toExchange.Clear();
    }*/
}
