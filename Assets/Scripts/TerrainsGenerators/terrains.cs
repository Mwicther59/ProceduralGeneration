using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class terrains : MonoBehaviour
{
    // Enum to switch between terrain generation styles
    public enum TerrainType { Mountains, Beaches, Mixed, Island }
    public TerrainType terrainType;

    #region Terrain Parameters

    [Header("Terrain Dimensions")]
    [SerializeField] private int width = 512;
    [SerializeField] private int length = 512;
    [SerializeField] private int height = 50;

    [Header("General Noise Settings")]
    [SerializeField] private float scale = 50f;

    [Header("Randomness Control")]
    [SerializeField] private float randomnessFactor = 0.5f;

    [Header("Feature-Specific Parameters")]
    [SerializeField] private float valleyDepth = 0.5f;
    [SerializeField] private float mountainRoughness = 0.8f;
    [SerializeField] private float beachHeight = 0.15f;
    [SerializeField] private float seaRatio = 0.3f;
    [SerializeField] private float seaLevel = -0.2f;

    [Header("Island Parameters")]
    [SerializeField] private int islandMinHeight = 75;
    [SerializeField] private int islandMaxHeight = 250;
    [SerializeField] private float islandMinValleyDepth = 0.2f;
    [SerializeField] private float islandMaxValleyDepth = 1f;
    [SerializeField] private float islandMinMountainRoughness = 0.5f;
    [SerializeField] private float islandMaxMountainRoughness = 1f;
    [SerializeField] private float islandMinBeachHeight = 0.1f;
    [SerializeField] private float islandMaxBeachHeight = 0.5f;
    [SerializeField] private float islandMinSeaRatio = 0.2f;
    [SerializeField] private float islandMaxSeaRatio = 0.7f;
    [SerializeField] private float islandMinRandomnessFactor = 0.1f;
    [SerializeField] private float islandMaxRandomnessFactor = 0.9f;

    #endregion

    #region Painting and Texture Parameters

    [Header("Painting Settings")]
    [SerializeField] private float sandHeight = -0.1f;
    [SerializeField] private float grassHeight = 0.3f;
    [SerializeField] private float rockHeight = 0.6f;

    [Header("Terrain Layers")]
    [SerializeField] private TerrainLayer sandLayer;
    [SerializeField] private TerrainLayer grassLayer;
    [SerializeField] private TerrainLayer rockLayer;
    [SerializeField] private TerrainLayer waterLayer;

    #endregion

    #region Generation Timing and UI

    [Header("Generation Settings")]
    [SerializeField] private int generationRepeats = 2;
    [SerializeField] private float generationInterval = 5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI textTimer;

    private Terrain terrain;

    #endregion

    #region Unity Events

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        GenerateTerrain();
        StartCoroutine(GenerateFast(generationRepeats));
    }

    private IEnumerator GenerateFast(int times)
    {
        for (int i = 0; i < times; i++)
        {
            GenerateTerrain();

            // Show countdown to next generation
            float countdown = generationInterval;
            int steps = 10;
            float interval = countdown / steps;

            for (int j = 0; j < steps; j++)
            {
                textTimer.text = $"Next generation in: {countdown - j * interval:F1} seconds";
                yield return new WaitForSeconds(interval);
            }

            textTimer.text = "Generating...";
            yield return new WaitForSeconds(0.5f);
        }

        textTimer.text = "Done!";
    }

    #endregion

    #region Terrain Generation Dispatcher

    public void GenerateTerrain()
    {
        float[,] heights;

        // Controlled variation for realism
        float noiseScaleVariation = Random.Range(0.8f, 1.5f) * randomnessFactor + (1 - randomnessFactor);
        float valleyDepthVariation = Random.Range(0.3f, 0.7f) * randomnessFactor + (1 - randomnessFactor);
        float mountainRoughnessVariation = Random.Range(0.5f, 1.5f) * randomnessFactor + (1 - randomnessFactor);

        switch (terrainType)
        {
            case TerrainType.Mountains:
                heights = GenerateMountains(noiseScaleVariation, mountainRoughnessVariation);
                break;

            case TerrainType.Beaches:
                heights = BlendFeaturesWithBeaches(noiseScaleVariation, valleyDepthVariation);
                break;

            case TerrainType.Island:
                // Randomize island-specific parameters
                height = Random.Range(islandMinHeight, islandMaxHeight);
                valleyDepth = Random.Range(islandMinValleyDepth, islandMaxValleyDepth);
                mountainRoughness = Random.Range(islandMinMountainRoughness, islandMaxMountainRoughness);
                beachHeight = Random.Range(islandMinBeachHeight, islandMaxBeachHeight);
                seaRatio = Random.Range(islandMinSeaRatio, islandMaxSeaRatio);
                randomnessFactor = Random.Range(islandMinRandomnessFactor, islandMaxRandomnessFactor);

                float islandNoiseScale = Random.Range(0.8f, 1.5f) * randomnessFactor + (1 - randomnessFactor);
                heights = GenerateIsland(islandNoiseScale, 0.1f);
                PaintTerrainTextures(heights);
                break;

            default:
                heights = GenerateValleys(valleyDepthVariation);
                break;
        }

        ApplyHeightsToTerrain(heights);
        PaintTerrainTextures(heights);
    }

    #endregion

    #region Terrain Feature Generators

    float[,] GenerateValleys(float valleyVariation)
    {
        float[,] heights = new float[width, length];
        int layers = 6;
        float persistence = 0.5f;
        float lacunarity = 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float gradient = Mathf.Abs((float)x / width - 0.5f);
                float noise = GenerateLayeredPerlinNoise(x, y, layers, scale * valleyVariation, persistence, lacunarity);
                heights[x, y] = noise * (1 - gradient * valleyDepth);
            }
        }

        return heights;
    }

    float[,] GenerateMountains(float noiseVariation, float roughness)
    {
        float[,] heights = new float[width, length];
        int layers = 6;
        float persistence = 0.5f;
        float lacunarity = 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float noise = GenerateLayeredPerlinNoise(x, y, layers, scale * noiseVariation, persistence, lacunarity);
                heights[x, y] = Mathf.Pow(noise, roughness);
            }
        }

        return heights;
    }

    float[,] GenerateBeaches(float[,] baseHeights)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (baseHeights[x, y] < beachHeight)
                {
                    baseHeights[x, y] = beachHeight;
                }
            }
        }
        return baseHeights;
    }

    float[,] BlendFeaturesWithBeaches(float noiseVariation, float valleyVariation)
    {
        float[,] valleyHeights = GenerateValleys(valleyVariation);
        float[,] mountainHeights = GenerateMountains(noiseVariation, mountainRoughness);
        float[,] blendedHeights = new float[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float blendMask = Mathf.PerlinNoise(x / 100f, y / 100f);
                blendedHeights[x, y] = Mathf.Lerp(valleyHeights[x, y], mountainHeights[x, y], blendMask);

                float beachGradient = Mathf.Min((float)x / width, (float)y / length);
                if (beachGradient < seaRatio)
                {
                    blendedHeights[x, y] = Mathf.Lerp(blendedHeights[x, y], seaLevel, (1 - beachGradient / seaRatio));
                }
            }
        }

        return blendedHeights;
    }

    float[,] GenerateIsland(float variation, float mountainPercentage)
    {
        float[,] heights = new float[width, length];
        int layers = 5;
        float persistence = 0.5f;
        float lacunarity = 2f;

        Vector2 peak = new Vector2(Random.Range(0f, width), Random.Range(0f, length));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float valley = GenerateLayeredPerlinNoise(x, y, layers, scale * variation, persistence, lacunarity);
                float mountain = GenerateLayeredPerlinNoise(x, y, layers, scale * variation, persistence, lacunarity);
                float blend = Mathf.Lerp(valley, mountain, mountainPercentage);

                float distToPeak = Vector2.Distance(new Vector2(x, y), peak) / Mathf.Max(width, length);
                float peakMod = Mathf.Exp(-distToPeak * 5f);

                heights[x, y] = blend + peakMod * (1 - distToPeak) * 0.5f;

                float distToEdge = Mathf.Min(x / (float)width, (width - x) / (float)width, y / (float)length, (length - y) / (float)length);
                if (distToEdge < seaRatio)
                {
                    heights[x, y] = Mathf.Lerp(heights[x, y], seaLevel, (1 - distToEdge / seaRatio));
                }
            }
        }

        return heights;
    }

    float GenerateLayeredPerlinNoise(float x, float y, int layers, float baseScale, float persistence, float lacunarity)
    {
        float value = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue = 0f;

        for (int i = 0; i < layers; i++)
        {
            float perlin = Mathf.PerlinNoise(x / (baseScale / frequency), y / (baseScale / frequency));
            value += perlin * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return value / maxValue;
    }

    #endregion

    #region Texture Painting

    void PaintTerrainTextures(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.terrainLayers = new TerrainLayer[] { sandLayer, grassLayer, rockLayer, waterLayer };

        int mapW = terrainData.alphamapWidth;
        int mapH = terrainData.alphamapHeight;
        float[,,] alphaMap = new float[mapW, mapH, terrainData.terrainLayers.Length];

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                float normX = (float)x / mapW;
                float normY = (float)y / mapH;
                int heightX = Mathf.FloorToInt(normX * (terrainData.heightmapResolution - 1));
                int heightY = Mathf.FloorToInt(normY * (terrainData.heightmapResolution - 1));
                float h = heights[heightX, heightY];

                float[] weights = new float[terrainData.terrainLayers.Length];

                if (h <= 0f) weights[3] = 1f;        // Water
                else if (h < sandHeight) weights[0] = 1f;  // Sand
                else if (h < grassHeight) weights[1] = 1f; // Grass
                else if (h < rockHeight) weights[2] = 1f;  // Rock

                float total = 0;
                foreach (var w in weights) total += w;

                for (int i = 0; i < weights.Length; i++)
                    alphaMap[x, y, i] = weights[i] / total;
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    #endregion

    #region Terrain Application

    void ApplyHeightsToTerrain(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);
        terrainData.SetHeights(0, 0, heights);

        terrain.terrainData = terrainData;

        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.terrainData = terrainData;
        }
    }

    #endregion
}
