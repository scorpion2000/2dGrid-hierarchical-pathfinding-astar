using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    int chunkSize;
    Dictionary<int, GameObject> tileset;
    Dictionary<GameObject, GameObject[,]> chunkTiles = new Dictionary<GameObject, GameObject[,]>();

    public int SetChunkSize { set { chunkSize = value; } }
    public Dictionary<int, GameObject> SetTileset { set { tileset = value; } }

    public void GenerateChunkObjects(List<int> terrainValues, GameObject chunkParent)
    {
        GameObject[,] tiles = new GameObject[chunkSize,chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int value = terrainValues[x * chunkSize + y];
                GameObject tile = Instantiate(tileset[value], chunkParent.transform);
                tile.transform.localPosition = new Vector2(x,y);
                tiles[x, y] = tile;
            }
        }
        chunkTiles.Add(chunkParent, tiles);
    }

    public void DeleteChunkObjects(GameObject chunkParent)
    {
        foreach (Transform child in chunkParent.transform)
        {
            Destroy(child.gameObject);
        }

        Destroy(chunkParent.gameObject);
        chunkTiles.Remove(chunkParent);
    }

    public void UpdateChunkObject(GameObject chunkParent, int posX, int posY, int index)
    {
        GameObject tile = Instantiate(tileset[index], chunkParent.transform);
        tile.transform.localPosition = new Vector2(posX, posY);
        Destroy(chunkTiles[chunkParent][posX, posY]);
    }
}
