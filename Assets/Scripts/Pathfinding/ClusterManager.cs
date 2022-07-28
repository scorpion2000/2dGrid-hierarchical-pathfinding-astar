using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterManager : MonoBehaviour
{
    [SerializeField] int entranceWidth = 6;
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawGizmosConnections = false;
    [SerializeField] UpdateUI updateUI;

    TerrainGenerator terrain;
    Grid grid;

    private int clusterSize;
    private Cluster[,] clusters;
    private Node[,] nodeGrid;

    int bottomX;
    int bottomY;

    public Dictionary<Vector2, Cluster> clusterNodes = new Dictionary<Vector2, Cluster>();
    public event Action ClusteringComplete;

    private void Awake()
    {
        terrain = FindObjectOfType<TerrainGenerator>();
        grid = FindObjectOfType<Grid>();

        terrain.terrainGenerationComplete += ClusterManagerSetup;
    }

    private void ClusterManagerSetup()
    {
        terrain.terrainGenerationComplete -= ClusterManagerSetup;
        nodeGrid = grid.GetGrid;

        clusterSize = terrain.GetChunkSize;
        StartCoroutine(GenerateClusters());
    }

    private IEnumerator GenerateClusters()
    {
        clusters = new Cluster[terrain.mapWidth, terrain.mapHeight];

        for (int x = 0; x < terrain.mapWidth; x++)
        {
            for (int y = 0; y < terrain.mapHeight; y++)
            {
                Vector2 nodeBtmLeftPos = nodeGrid[bottomX + x * clusterSize, bottomY + y * clusterSize].worldPos;
                Vector2 nodeTopRightPos = nodeGrid[bottomX + x * clusterSize + clusterSize - 1, bottomY + y * clusterSize + clusterSize - 1].worldPos;
                clusters[x, y] = new Cluster(bottomX + x * clusterSize, bottomY + y * clusterSize, nodeBtmLeftPos, nodeTopRightPos);
                updateUI.AddToProgress(1);
                yield return new WaitForSeconds(0.001f);
            }
        }
        StartCoroutine(BuildEntrances());
    }
    
    private IEnumerator BuildEntrances()
    {
        for (int x = 0; x < terrain.mapWidth; x++)
        {
            for (int y = 0; y < terrain.mapHeight; y++)
            {
                CreateEntrance(clusters[x, y]);
                updateUI.AddToProgress(1);
                yield return new WaitForSeconds(0.001f);
            }
        }

        //CreateEntranceNodes(clusters[0, 0]);
        //CreateEntranceNodes(clusters[0, 1]);
        //Need to do this seperately
        for (int x = 0; x < terrain.mapWidth; x++)
        {
            for (int y = 0; y < terrain.mapHeight; y++)
            {
                CreateEntranceNodes(clusters[x, y]);
                updateUI.AddToProgress(1);
                yield return new WaitForSeconds(0.001f);
            }
        }
        StartCoroutine(MakeClusterConnections());
    }

    //          up
    //        _ _ _ _
    //       |       |
    //  left |       | right
    //       |_ _ _ _|
    //         down

    private void CreateEntrance(Cluster cluster)
    {
        int clusterPosX = (int)cluster.GetClusterVectorPos.x;
        int clusterPosY = (int)cluster.GetClusterVectorPos.y;

        //left
        if (clusterPosX != 0)
        {
            Entrance newEntrance = new Entrance();
            List<Vector2> entranceVecs = new List<Vector2>();
            List<Vector2> symEntranceVecs = new List<Vector2>();
            for (int i = 0; i < clusterSize; i++)
            {
                entranceVecs.Add(nodeGrid[clusterPosX, clusterPosY + i].worldPos);
                symEntranceVecs.Add(nodeGrid[clusterPosX - 1, clusterPosY + i].worldPos);
            }
            newEntrance.position = 0;
            newEntrance.entrancePositions = entranceVecs;
            newEntrance.symEntrancePositions = symEntranceVecs;
            newEntrance.existingNodesInEntrance = new List<Node>();
            cluster.RegisterNewEntrance(newEntrance);
        }

        //right
        if (clusterPosX / clusterSize != terrain.mapWidth - 1)
        {
            Entrance newEntrance = new Entrance();
            List<Vector2> entranceVecs = new List<Vector2>();
            List<Vector2> symEntranceVecs = new List<Vector2>();
            for (int i = 0; i < clusterSize; i++)
            {
                entranceVecs.Add(nodeGrid[clusterPosX + clusterSize -1, clusterPosY + i].worldPos);
                symEntranceVecs.Add(nodeGrid[clusterPosX + clusterSize, clusterPosY + i].worldPos);
            }
            newEntrance.position = 1;
            newEntrance.entrancePositions = entranceVecs;
            newEntrance.symEntrancePositions = symEntranceVecs;
            newEntrance.existingNodesInEntrance = new List<Node>();
            cluster.RegisterNewEntrance(newEntrance);
        }

        //down
        if (clusterPosY != 0)
        {
            Entrance newEntrance = new Entrance();
            List<Vector2> entranceVecs = new List<Vector2>();
            List<Vector2> symEntranceVecs = new List<Vector2>();
            for (int i = 0; i < clusterSize; i++)
            {
                entranceVecs.Add(nodeGrid[clusterPosX + i, clusterPosY].worldPos);
                symEntranceVecs.Add(nodeGrid[clusterPosX + i, clusterPosY - 1].worldPos);
            }
            newEntrance.position = 2;
            newEntrance.entrancePositions = entranceVecs;
            newEntrance.symEntrancePositions = symEntranceVecs;
            newEntrance.existingNodesInEntrance = new List<Node>();
            cluster.RegisterNewEntrance(newEntrance);
        }

        //up
        if (clusterPosY / clusterSize != terrain.mapHeight - 1)
        {
            Entrance newEntrance = new Entrance();
            List<Vector2> entranceVecs = new List<Vector2>();
            List<Vector2> symEntranceVecs = new List<Vector2>();
            for (int i = 0; i < clusterSize; i++)
            {
                entranceVecs.Add(nodeGrid[clusterPosX + i, clusterPosY + clusterSize - 1].worldPos);
                symEntranceVecs.Add(nodeGrid[clusterPosX + i, clusterPosY + clusterSize].worldPos);
            }
            newEntrance.position = 3;
            newEntrance.entrancePositions = entranceVecs;
            newEntrance.symEntrancePositions = symEntranceVecs;
            newEntrance.existingNodesInEntrance = new List<Node>();
            cluster.RegisterNewEntrance(newEntrance);
        }
    }

    private void CreateEntranceNodes(Cluster cluster)
    {
        List<Entrance> clusterEntrances = cluster.GetClusterEntrances;

        foreach (Entrance entrance in clusterEntrances)
        {
            bool skip = false;
            foreach (Node entranceNode in cluster.GetEntranceNodes)
            {
                if (entrance.existingNodesInEntrance.Count != 0 && entrance.existingNodesInEntrance.Contains(entranceNode)) skip = true;
            }
            if (skip) continue;

            List<GenEntrance> foundEntrances = new List<GenEntrance>();
            GenEntrance genEntrance = new GenEntrance();
            GenEntrance backGenEntrance = new GenEntrance();
            float movementPenaltyH = Mathf.Infinity;
            float symMovementPenaltyH = Mathf.Infinity;
            int entranceSize = 0;

            for (int i = 0; i < entrance.entrancePositions.Count; i++)
            {
                Node gridNode = grid.NodeFromWorldPoint(entrance.entrancePositions[i], nodeGrid);
                Node symGridNode = grid.NodeFromWorldPoint(entrance.symEntrancePositions[i], nodeGrid);
                if (gridNode.walkable && entranceSize < entrance.entrancePositions.Count - 1)
                {
                    entranceSize++;
                    if ((gridNode.movementPenalty < movementPenaltyH || symGridNode.movementPenalty < symMovementPenaltyH) && symGridNode.walkable)
                    {
                        genEntrance.entrance = gridNode;
                        genEntrance.symEntrance = symGridNode;
                        movementPenaltyH = gridNode.movementPenalty;
                        symMovementPenaltyH = symGridNode.movementPenalty;
                    }
                }
                if (!gridNode.walkable || i == entrance.entrancePositions.Count - 1)
                {
                    if (entranceSize > 0) foundEntrances.Add(genEntrance);

                    if (entranceSize > entranceWidth)
                    {
                        movementPenaltyH = Mathf.Infinity;
                        symMovementPenaltyH = Mathf.Infinity;

                        for (int x = i; x >= 0; x--)
                        {
                            Node backGridNode = grid.NodeFromWorldPoint(entrance.entrancePositions[x], nodeGrid);
                            Node backSymGridNode = grid.NodeFromWorldPoint(entrance.symEntrancePositions[x], nodeGrid);
                            if (backGridNode == genEntrance.entrance)
                            {
                                break;
                            }

                            if ((backGridNode.movementPenalty < movementPenaltyH || backSymGridNode.movementPenalty < symMovementPenaltyH) && backSymGridNode.walkable)
                            {
                                backGenEntrance.entrance = backGridNode;
                                backGenEntrance.symEntrance = backSymGridNode;
                                movementPenaltyH = backGridNode.movementPenalty;
                                symMovementPenaltyH = backSymGridNode.movementPenalty;
                            }
                        }
                        foundEntrances.Add(backGenEntrance);
                    }

                    movementPenaltyH = Mathf.Infinity;
                    symMovementPenaltyH = Mathf.Infinity;
                    entranceSize = 0;
                }
            }

            foreach (GenEntrance foundEntrance in foundEntrances)
            {
                if (foundEntrance.entrance != null)
                {
                    Vector2 clusterPos = cluster.GetClusterVectorPos;
                    Cluster neighbourCluster;

                    Node newSymNode;
                    Node newNode = cluster.AddEntranceNode(foundEntrance.entrance);
                    //Debug.Log(newNode.worldPos);
                    if (!clusterNodes.ContainsKey(foundEntrance.entrance.worldPos))
                        clusterNodes.Add(foundEntrance.entrance.worldPos, cluster);

                    cluster.RegisterEntranceNodeToEntrance(cluster.GetEntranceByNeighbourNode(foundEntrance.symEntrance), newNode);

                    switch (entrance.position)
                    {
                        case 0:
                            neighbourCluster = clusters[(int)clusterPos.x / clusterSize - 1, (int)clusterPos.y / clusterSize];
                            newSymNode = neighbourCluster.AddEntranceNode(foundEntrance.symEntrance);
                            if (!clusterNodes.ContainsKey(foundEntrance.symEntrance.worldPos))
                                clusterNodes.Add(foundEntrance.symEntrance.worldPos, neighbourCluster);

                            neighbourCluster.RegisterEntranceNodeToEntrance(neighbourCluster.GetEntranceByNeighbourNode(foundEntrance.entrance), newSymNode);
                            
                            cluster.ConnectSymEntrance(newNode, newSymNode);
                            neighbourCluster.ConnectSymEntrance(newSymNode, newNode);

                            break;
                        case 1:
                            neighbourCluster = clusters[(int)clusterPos.x / clusterSize + 1, (int)clusterPos.y / clusterSize];
                            newSymNode = neighbourCluster.AddEntranceNode(foundEntrance.symEntrance);
                            if (!clusterNodes.ContainsKey(foundEntrance.symEntrance.worldPos))
                                clusterNodes.Add(foundEntrance.symEntrance.worldPos, neighbourCluster);

                            neighbourCluster.RegisterEntranceNodeToEntrance(neighbourCluster.GetEntranceByNeighbourNode(foundEntrance.entrance), newSymNode);

                            cluster.ConnectSymEntrance(newNode, newSymNode);
                            neighbourCluster.ConnectSymEntrance(newSymNode, newNode);

                            break;
                        case 2:
                            neighbourCluster = clusters[(int)clusterPos.x / clusterSize, (int)clusterPos.y / clusterSize - 1];
                            newSymNode = neighbourCluster.AddEntranceNode(foundEntrance.symEntrance);
                            if (!clusterNodes.ContainsKey(foundEntrance.symEntrance.worldPos))
                                clusterNodes.Add(foundEntrance.symEntrance.worldPos, neighbourCluster);

                            neighbourCluster.RegisterEntranceNodeToEntrance(neighbourCluster.GetEntranceByNeighbourNode(foundEntrance.entrance), newSymNode);

                            cluster.ConnectSymEntrance(newNode, newSymNode);
                            neighbourCluster.ConnectSymEntrance(newSymNode, newNode);

                            break;
                        case 3:
                            neighbourCluster = clusters[(int)clusterPos.x / clusterSize, (int)clusterPos.y / clusterSize + 1];
                            newSymNode = neighbourCluster.AddEntranceNode(foundEntrance.symEntrance);
                            if (!clusterNodes.ContainsKey(foundEntrance.symEntrance.worldPos))
                                clusterNodes.Add(foundEntrance.symEntrance.worldPos, neighbourCluster);

                            neighbourCluster.RegisterEntranceNodeToEntrance(neighbourCluster.GetEntranceByNeighbourNode(foundEntrance.entrance), newSymNode);

                            cluster.ConnectSymEntrance(newNode, newSymNode);
                            neighbourCluster.ConnectSymEntrance(newSymNode, newNode);

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    //ALWAYS STARTS FROM BOTTOM TO TOP
    //private void GenerateEntranceNodesVertical(int posX, int posY, bool checkLeft, Cluster genCluster)
    //{
    //    List<List<Node>> entranceList = new List<List<Node>>();
    //    int xClusterAdjust = (checkLeft) ? -1 : 1;
    //    int xAdjust = (checkLeft) ? 0 : clusterSize - 1;

    //    List<Node> entrance = new List<Node>();
    //    //List<GenEntrance> emptyList = new List<GenEntrance>();

    //    for (int i = 0; i < clusterSize; i++)
    //    {
    //        Node nodeToCheck = nodeGrid[posX * clusterSize + xAdjust, posY * clusterSize + i];
    //        if (genCluster.CheckForExistingEntranceNodeByPos(nodeToCheck.worldPos) != null && genCluster.CheckForExistingEntranceNodeByPos(nodeToCheck.worldPos).CheckClusterVertical)
    //            return;
            
    //        if (nodeToCheck.walkable)
    //        {
    //            entrance.Add(nodeToCheck);
    //        } else
    //        {
    //            if (entrance.Count > 0) entranceList.Add(entrance);
    //            entrance.Clear();
    //        }
    //    }

    //    if (entrance.Count > 0) entranceList.Add(entrance);

    //    List<GenEntrance> entranceNodes = new List<GenEntrance>();
    //    foreach (List<Node> iEntrance in entranceList)
    //    {
    //        int lowestMovePen = 100000;   //This should later be changed to Mathf.Infinity somehow
    //        GenEntrance genEntranceBottom = new GenEntrance();
    //        GenEntrance genEntranceTop = new GenEntrance();
    //        //genEntranceBottom.entrance = iEntrance[0];

    //        if (iEntrance.Count >= 1)
    //        {
    //            if (iEntrance.Count >= entranceWidth)
    //            {
    //                for (int i = 0; i < iEntrance.Count - 1; i++)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX + xClusterAdjust, iEntrance[i].gridY];

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceBottom.entrance = nodeToCheck;
    //                        genEntranceBottom.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //                //Check from top to bottom for second entrance
    //                //genEntranceTop.entrance = iEntrance[iEntrance.Count - 1];
    //                lowestMovePen = 100000;   //This should later be changed to Mathf.Infinity somehow
    //                for (int i = iEntrance.Count - 1; i >= 1; i--)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX + xClusterAdjust, iEntrance[i].gridY];

    //                    if (nodeToCheck == genEntranceBottom.entrance) break;

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceTop.entrance = nodeToCheck;
    //                        genEntranceTop.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //                entranceNodes.Add(genEntranceTop);
    //            } else
    //            {
    //                for (int i = 0; i <= iEntrance.Count - 1; i++)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX + xClusterAdjust, iEntrance[i].gridY];

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceBottom.entrance = nodeToCheck;
    //                        genEntranceBottom.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //            }
    //            entranceNodes.Add(genEntranceBottom);
    //        }
    //    }

    //    foreach (GenEntrance genEntrance in entranceNodes)
    //    {
    //        genCluster.AddEntranceNode(genEntrance.entrance.worldPos, true);

    //        //Updating the neighbour too
    //        Cluster neighbourCluster = clusters[posX + xClusterAdjust, posY];
    //        neighbourCluster.AddEntranceNode(genEntrance.symEntrance.worldPos, true);

    //        clusters[posX + xClusterAdjust, posY] = neighbourCluster;
    //        clusters[posX, posY] = genCluster;

    //        //genCluster.ConnectSymEntrance(genEntrance.entrance, genEntrance.symEntrance);
    //        //neighbourCluster.ConnectSymEntrance(genEntrance.symEntrance, genEntrance.entrance);
    //    }
    //}

    //ALWAYS STARTS FROM LEFT TO RIGHT
    //private void GenerateEntranceNodesHorizontal(int posX, int posY, bool checkBottom, Cluster genCluster)
    //{
    //    List<List<Node>> entranceList = new List<List<Node>>();
    //    int yClusterAdjust = (checkBottom) ? -1 : 1;
    //    int yAdjust = (checkBottom) ? 0 : clusterSize - 1;

    //    List<Node> entrance = new List<Node>();
    //    //List<GenEntrance> emptyList = new List<GenEntrance>();

    //    for (int i = 0; i < clusterSize; i++)
    //    {
    //        Node nodeToCheck = nodeGrid[posX * clusterSize + i, posY * clusterSize + yAdjust];
    //        if (genCluster.CheckForExistingEntranceNodeByPos(nodeToCheck.worldPos) != null && !genCluster.CheckForExistingEntranceNodeByPos(nodeToCheck.worldPos).CheckClusterVertical)
    //            return;

    //        if (nodeToCheck.walkable)
    //        {
    //            entrance.Add(nodeToCheck);
    //        }
    //        else
    //        {
    //            if (entrance.Count > 0) entranceList.Add(entrance);
    //            entrance.Clear();
    //        }
    //    }

    //    if (entrance.Count > 0) entranceList.Add(entrance);

    //    List<GenEntrance> entranceNodes = new List<GenEntrance>();
    //    foreach (List<Node> iEntrance in entranceList)
    //    {
    //        int lowestMovePen = 100000;   //This should later be changed to Mathf.Infinity somehow
    //        GenEntrance genEntranceLeft = new GenEntrance();
    //        GenEntrance genEntranceRight = new GenEntrance();
    //        //genEntranceBottom.entrance = iEntrance[0];

    //        if (iEntrance.Count >= 1)
    //        {
    //            if (iEntrance.Count >= entranceWidth)
    //            {
    //                for (int i = 0; i < iEntrance.Count - 1; i++)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY + yClusterAdjust];

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceLeft.entrance = nodeToCheck;
    //                        genEntranceLeft.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //                //Check from top to bottom for second entrance
    //                //genEntranceTop.entrance = iEntrance[iEntrance.Count - 1];
    //                lowestMovePen = 100000;   //This should later be changed to Mathf.Infinity somehow
    //                for (int i = iEntrance.Count - 1; i >= 1; i--)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY + yClusterAdjust];

    //                    if (nodeToCheck == genEntranceLeft.entrance) break;

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceRight.entrance = nodeToCheck;
    //                        genEntranceRight.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //                entranceNodes.Add(genEntranceRight);
    //            } else
    //            {
    //                for (int i = 0; i <= iEntrance.Count - 1; i++)
    //                {
    //                    Node nodeToCheck = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY];
    //                    Node nodeToCheckSym = nodeGrid[iEntrance[i].gridX, iEntrance[i].gridY + yClusterAdjust];

    //                    if (nodeToCheck.movementPenalty < lowestMovePen && nodeToCheckSym.walkable)
    //                    {
    //                        genEntranceLeft.entrance = nodeToCheck;
    //                        genEntranceLeft.symEntrance = nodeToCheckSym;
    //                        lowestMovePen = nodeToCheck.movementPenalty;
    //                    }
    //                }
    //            }
    //            entranceNodes.Add(genEntranceLeft);
    //        }
    //    }

    //    foreach (GenEntrance genEntrance in entranceNodes)
    //    {
    //        genCluster.AddEntranceNode(genEntrance.entrance.worldPos, true);

    //        //Updating the neighbour too
    //        Cluster neighbourCluster = clusters[posX , posY + yClusterAdjust];
    //        neighbourCluster.AddEntranceNode(genEntrance.symEntrance.worldPos, true);

    //        clusters[posX, posY + yClusterAdjust] = neighbourCluster;
    //        clusters[posX, posY] = genCluster;

    //        //genCluster.ConnectSymEntrance(genEntrance.entrance, genEntrance.symEntrance);
    //        //neighbourCluster.ConnectSymEntrance(genEntrance.symEntrance, genEntrance.entrance);
    //    }
    //}

    private IEnumerator MakeClusterConnections()
    {
        for (int x = 0; x < terrain.mapWidth; x++)
        {
            for (int y = 0; y < terrain.mapHeight; y++)
            {
                ConnectEntrances(clusters[x, y]);
                updateUI.AddToProgress(1);
                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        ClusteringComplete?.Invoke();
    }

    private void ConnectEntrances(Cluster cluster)
    {
        if (cluster.GetEntranceNodes == null) return;
        Node[,] clusterNodes = new Node[clusterSize,clusterSize];

        for (int x = 0; x < clusterSize; x++)
        {
            for (int y = 0; y < clusterSize; y++)
            {
                clusterNodes[x,y] = nodeGrid[(int)cluster.GetClusterVectorPos.x + x, (int)cluster.GetClusterVectorPos.y + y];
            }
        }

        foreach (Node entranceNode in cluster.GetEntranceNodes)
        {
            foreach (Node connectEntranceNode in cluster.GetEntranceNodes)
            {
                if (entranceNode.worldPos == connectEntranceNode.worldPos) continue;
                PathRequestManager.RequestPath(new PathRequest(entranceNode.worldPos, connectEntranceNode.worldPos, clusterNodes, false, ClusterPathFound));
            }
        }
    }

    private void ClusterPathFound(Vector2[] newPath, bool pathSuccessful, float pathCost, bool clusterSearch)
    {
        if (!pathSuccessful) return;
        Cluster cluster = clusterNodes[newPath[0]];

        foreach (Vector2 vector in newPath)
        {
            if (
                (vector.x < cluster.gridPosBtmLeft.x || vector.x > cluster.gridPosTopRight.x) ||
                (vector.y < cluster.gridPosBtmLeft.y || vector.y > cluster.gridPosTopRight.y)
            ) {
                Debug.Log("Path is outside the cluster");
                return;
            }
        }

        cluster.MakeNodeConnection(
            cluster.CheckForExistingEntranceNodeByPos(newPath[0]),
            cluster.CheckForExistingEntranceNodeByPos(newPath[newPath.Length - 1]),
            pathCost
        );
        clusters[(int)cluster.GetClusterVectorPos.x / clusterSize, (int)cluster.GetClusterVectorPos.y / clusterSize] = cluster;
    }

    public List<Node> GetConnectedNodes(Node node, Node nodeCheck)
    {
        Cluster cluster;
        if (clusterNodes.ContainsKey(node.worldPos))
            cluster = clusterNodes[node.worldPos];
        else
            cluster = GetClusterByNode(node);

        if (GetClusterByNode(nodeCheck) == cluster)
        {
            List<Node> nodes = GetConnectedNodes(node, cluster);
            nodes.Add(nodeCheck);

            return nodes;
        }

        return GetConnectedNodes(node, cluster);
    }

    public List<Node> GetConnectedNodes(Node node, Cluster cluster)
    {
        if (cluster.FindNodeInCluster(node))
        {
            return cluster.GetEntranceConnections(node);
        } else
        {
            return cluster.GetEntranceNodes;
        }
    }

    public Cluster GetClusterByNode(Node node)
    {
        return clusters[Mathf.FloorToInt(node.gridX / clusterSize), Mathf.FloorToInt(node.gridY / clusterSize)];
    }

    public bool CheckIfNeighbourCluster(Node nodeA, Node nodeB)
    {
        Cluster clusterA = GetClusterByNode(nodeA);
        Cluster clusterB = GetClusterByNode(nodeB);

        int x = Mathf.Abs((int)clusterA.GetClusterVectorPos.x - (int)clusterB.GetClusterVectorPos.x) / clusterSize;
        int y = Mathf.Abs((int)clusterA.GetClusterVectorPos.y - (int)clusterB.GetClusterVectorPos.y) / clusterSize;

        if (x <= 1 && y <= 1)
            return true;
        else
            return false;
    }

    //Might not actually need such a function
    /*public Cluster FindClusterByPosition(Vector2 pos)
    {

    }*/

    public void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            foreach (Cluster cluster in clusters)
            {
                Gizmos.color = Color.black;
                foreach (Node entranceNode in cluster.GetEntranceNodes)
                {
                    Gizmos.DrawCube(entranceNode.worldPos, Vector2.one / 2);
                    if (drawGizmosConnections)
                    {
                        foreach (Node vectorConnection in cluster.GetEntranceConnections(entranceNode))
                        {
                            Gizmos.DrawLine(entranceNode.worldPos, vectorConnection.worldPos);
                        }

                        Gizmos.DrawLine(entranceNode.worldPos, cluster.GetClusterSymEntrance(entranceNode).worldPos);
                    }
                }
            }
        }
    }
}

public struct GenEntrance
{
    public Node entrance;
    public Node symEntrance;
}


/*public struct Cluster
{
    public Entrance[] entrances;
}

public struct Entrance
{
    public List<Node> entranceNodes;
}*/