using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainEditor : MonoBehaviour
{
    Grid grid;

    [SerializeField] Button[] buttons;
    int terrainIndex;

    private void Awake()
    {
        grid = FindObjectOfType<Grid>();
        buttons[0].Select();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Node node = grid.NodeFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            node.SetWalkable = (terrainIndex == 3) ? false : true;
            node.movementPenalty = terrainIndex * 15;

            grid.UpdateNode(node, terrainIndex);
        }
    }

    public void HandleButtonSelection(int index)
    {
        //buttons[index].Select();
        terrainIndex = index;
    }
}
