using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SetupHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI aiCounter;
    [SerializeField] TextMeshProUGUI mapSizeText;
    [SerializeField] TextMeshProUGUI aiCountText;
    [SerializeField] TextMeshProUGUI magnificationText;
    [SerializeField] TextMeshProUGUI chunkSizeText;
    [SerializeField] TextMeshProUGUI nodeCounter;
    [SerializeField] Slider mapSizeSlider;
    [SerializeField] Slider aiCountSlider;
    [SerializeField] Slider magnificationSlider;
    [SerializeField] Slider chunkSizeSlider;
    [SerializeField] GameObject aiHolder;
    [SerializeField] GameObject aiPrefab;

    TerrainGenerator terrainGenerator;
    Grid grid;
    ClusterManager clusterManager;

    private void Awake()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        grid = FindObjectOfType<Grid>();
        clusterManager = FindObjectOfType<ClusterManager>();

        mapSizeSlider.onValueChanged.AddListener(delegate{ OnMapSizeChange(); });
        aiCountSlider.onValueChanged.AddListener(delegate{ OnAICountChange(); });
        magnificationSlider.onValueChanged.AddListener(delegate{ OnMagnificationChange(); });
        chunkSizeSlider.onValueChanged.AddListener(delegate{ OnChunkSizeChange(); });
    }

    private void OnMapSizeChange()
    {
        mapSizeText.text = mapSizeSlider.value.ToString();
        CountNodes();
    }
    private void OnAICountChange()
    {
        aiCountText.text = aiCountSlider.value.ToString();
        CountNodes();
    }
    private void OnMagnificationChange()
    {
        magnificationText.text = magnificationSlider.value.ToString();
        CountNodes();
    }
    private void OnChunkSizeChange()
    {
        chunkSizeText.text = chunkSizeSlider.value.ToString();
        CountNodes();
    }
    private void CountNodes()
    {
        float nodeInChunk = chunkSizeSlider.value * chunkSizeSlider.value;
        float chunkCount = (mapSizeSlider.value * 2) * (mapSizeSlider.value * 2);
        float total = nodeInChunk * chunkCount;
        nodeCounter.text = total.ToString();
    }

    public void StartMapGeneration()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-400, 0, 0);

        terrainGenerator.SetMagnification = magnificationSlider.value;
        terrainGenerator.SetMapSize = Mathf.RoundToInt(mapSizeSlider.value);

        terrainGenerator.CenterMap();
        terrainGenerator.CreateTileset();

        grid.GridSetup();
        clusterManager.ClusteringComplete += HandleAISpawn;
    }

    private void HandleAISpawn()
    {
        StartCoroutine(SpawnAI());
    }

    private IEnumerator SpawnAI()
    {
        int aiSpawned = 0;
        while (aiSpawned < aiCountSlider.value)
        {
            Instantiate(aiPrefab, aiHolder.transform);
            aiSpawned++;
            aiCounter.text = aiSpawned + "/" + aiCountSlider.value;
            yield return new WaitForSeconds(0.001f);
        }
    }
}
