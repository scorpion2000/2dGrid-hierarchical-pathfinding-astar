using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    TerrainGenerator terrain;

    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    public int MaxSize { get { return gridSizeX * gridSizeY; } }
    public Node[,] GetGrid { get { return grid; } }

    public event Action mapGenerationComplete;

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    private void Awake()
    {
        terrain = FindObjectOfType<TerrainGenerator>();
        SetGridSize(terrain.mapHeight * terrain.GetChunkSize, terrain.mapWidth * terrain.GetChunkSize);

        GridSetup();
    }

    public void SetGridSize(int x, int y)
    {
        gridWorldSize = new Vector2(x,y);
    }

    public void GridSetup()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value = walkableMask |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value,2), region.terrainPenalty);
        }

        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                /*if (walkable)
                {
                    RaycastHit2D hit = Physics2D.CircleCast(worldPoint, nodeRadius, Vector3.forward, 0, walkableMask);
                    if (hit)
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                    //Debug.Log(movementPenalty);
                }*/
                
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty, 0);
            }
        }

        terrain.StartMapGeneration();
        mapGenerationComplete?.Invoke();
    }

    public List<Node> GetNeigbours(Node node)
    {
        return GetNeigbours(node, grid);
    }

    public List<Node> GetNeigbours(Node node, Node[,] _grid)
    {
        List<Node> neighbours = new List<Node>();
        List<Node> noCornerNeighbours = new List<Node>();

        bool sendNoCorner = false;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX - _grid[0, 0].gridX + x;
                int checkY = node.gridY - _grid[0, 0].gridY + y;

                if (checkX >= 0 && checkX < _grid.GetLength(0) && checkY >= 0 && checkY < _grid.GetLength(1))
                {
                    if (((x == 0 && y == -1) || (x == 0 && y == 1)) || ((x == -1 && y == 0) || (x == 1 && y == 0)))
                    {
                        noCornerNeighbours.Add(_grid[checkX, checkY]);
                        if (!_grid[checkX, checkY].walkable) sendNoCorner = true;
                    }

                    neighbours.Add(_grid[checkX, checkY]);
                }
            }
        }

        if (sendNoCorner) return noCornerNeighbours;
        return neighbours;
    }

    public void UpdateNode(Node updateNode)
    {
        grid[updateNode.gridX, updateNode.gridY] = updateNode;
    }

    public Node NodeFromWorldPoint(Vector2 _worldPos)
    {
        return NodeFromWorldPoint(_worldPos, grid);
    }

    public Node NodeFromWorldPoint(Vector2 _worldPos, Node[,] _grid)
    {
        float percentX = (_worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (_worldPos.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return _grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, gridWorldSize);

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? new Color(1,1,1,1) : Color.red;
                Gizmos.DrawCube(n.worldPos, Vector2.one * (nodeDiameter - 0.2f));
            }
        }
    }
}
