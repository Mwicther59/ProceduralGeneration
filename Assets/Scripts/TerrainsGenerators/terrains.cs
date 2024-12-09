using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class terrains : MonoBehaviour
{
    public enum TerrainType { Mountains, Beaches, Mixed, Island }
    public TerrainType terrainType;

    [Header("Terrain Dimensions")]
    [SerializeField] private int width = 512;
    [SerializeField] private int length = 512;
    [SerializeField] private int height = 50;

    [Header("General Noise Settings")]
    [SerializeField] private float scale = 50f;

    [Header("Randomness Control")]
    [SerializeField] private float randomnessFactor = 0.5f; // Controls the degree of randomness (0 = no randomness, 1 = full randomness)

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

    [Header("Painting Settings")]
    [SerializeField] private float sandHeight = -0.1f;
    [SerializeField] private float grassHeight = 0.3f;
    [SerializeField] private float rockHeight = 0.6f;

    [Header("Terrain Layers")]
    [SerializeField] private TerrainLayer sandLayer;
    [SerializeField] private TerrainLayer grassLayer;
    [SerializeField] private TerrainLayer rockLayer;
    [SerializeField] private TerrainLayer waterLayer;

    [Header("Generation Settings")]
    [SerializeField] private int generationRepeats = 2;
    [SerializeField] private float generationInterval = 5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI textTimer;
    private Terrain terrain;
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

            // Countdown loop for 5 seconds
            float countdown = 5f;
            while (countdown > 0)
            {
                textTimer.text = $"Next generation in: {countdown:F1} seconds";
                countdown -= Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            textTimer.text = "Generating..."; // Optional: Indicate when terrain is being generated
            yield return new WaitForSeconds(0.5f); // Small pause to display "Generating..." message
        }

        textTimer.text = "Done!"; // Optional: Indicate completion
    }
    public void GenerateTerrain()
    {
        float[,] heights;

        float noiseScaleVariation = Random.Range(0.8f, 1.5f) * randomnessFactor + (1 - randomnessFactor);
        float valleyDepthVariation = Random.Range(0.3f, 0.7f) * randomnessFactor + (1 - randomnessFactor);
        float mountainRoughnessVariation = Random.Range(0.5f, 1.5f) * randomnessFactor + (1 - randomnessFactor);

        switch (terrainType)
        {
            case TerrainType.Mountains:
                heights = GenerateMountains(noiseScaleVariation, mountainRoughnessVariation);
                break;
            case TerrainType.Beaches:
                heights = GenerateBeaches(GenerateValleys(valleyDepthVariation));
                PaintTerrainTextures(heights);
                break;
            case TerrainType.Mixed:
                heights = BlendFeaturesWithBeaches(noiseScaleVariation, valleyDepthVariation);
                break;
            case TerrainType.Island:
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

    //float[,] GenerateValleys(float valleyVariation)
    //{
    //    float[,] heights = new float[width, length];
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < length; y++)
    //        {
    //            float gradient = Mathf.Abs((float)x / width - 0.5f);
    //            float noise = Mathf.PerlinNoise(x / (scale * valleyVariation), y / (scale * valleyVariation)); // Apply randomness to noise scale
    //            heights[x, y] = noise * (1 - gradient * valleyDepth);
    //        }
    //    }
    //    return heights;
    //}

    //float[,] GenerateMountains(float noiseVariation, float roughness)
    //{
    //    float[,] heights = new float[width, length];
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < length; y++)
    //        {
    //            float baseNoise = Mathf.PerlinNoise(x / (scale * noiseVariation), y / (scale * noiseVariation));
    //            float detailNoise = Mathf.PerlinNoise(x / (scale * 0.5f), y / (scale * 0.5f)) * 0.5f;
    //            float combinedNoise = Mathf.Pow(baseNoise + detailNoise, roughness); // Apply randomness to mountain roughness
    //            heights[x, y] = combinedNoise;
    //        }
    //    }
    //    return heights;
    //}
    float[,] GenerateValleys(float valleyVariation)
    {
        float[,] heights = new float[width, length];

        // Parameters for layered Perlin noise
        int layers = 4;
        float persistence = 0.6f;
        float lacunarity = 2.2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float gradient = Mathf.Abs((float)x / width - 0.5f);
                float layeredNoise = GenerateLayeredPerlinNoise(x, y, layers, scale * valleyVariation, persistence, lacunarity);
                heights[x, y] = layeredNoise * (1 - gradient * valleyDepth); // Combine noise with gradient
            }
        }

        return heights;
    }

    float[,] GenerateMountains(float noiseVariation, float roughness)
    {
        float[,] heights = new float[width, length];

        // Parameters for layered Perlin noise
        int layers = 5;               // Number of noise layers
        float persistence = 0.5f;     // Controls amplitude reduction
        float lacunarity = 2.0f;      // Controls frequency increase

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float layeredNoise = GenerateLayeredPerlinNoise(x, y, layers, scale * noiseVariation, persistence, lacunarity);
                heights[x, y] = Mathf.Pow(layeredNoise, roughness); // Apply roughness for sharper peaks
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

    //float[,] GenerateIsland(float variation, float mountainPercentage)
    //{
    //    // Generate the base terrain heights
    //    float[,] valleyHeights = GenerateValleys(variation);
    //    float[,] mountainHeights = GenerateMountains(variation, mountainRoughness);
    //    float[,] islandHeights = new float[width, length];

    //    // Randomize the peak position within the island
    //    Vector2 peakPosition = new Vector2(UnityEngine.Random.Range(0f, width), UnityEngine.Random.Range(0f, length));

    //    // Blend the heights for the island based on the randomized peak position
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < length; y++)
    //        {
    //            // Blend the base valley and mountain heights based on mountainPercentage
    //            // The higher the mountainPercentage, the more influence the mountainHeights have
    //            float blendFactor = Mathf.Lerp(0f, 1f, mountainPercentage); // Controls the blend from valley to mountain
    //            islandHeights[x, y] = Mathf.Lerp(valleyHeights[x, y], mountainHeights[x, y], blendFactor);

    //            // Calculate the distance to the randomized peak position
    //            float distanceToPeak = Vector2.Distance(new Vector2(x, y), peakPosition) / Mathf.Max(width, length);

    //            // Adjust the height based on the distance to the peak
    //            // Closer to the peak, the height increases, farther away, it decreases
    //            float peakHeightModifier = Mathf.Exp(-distanceToPeak * 5f); // Exponential fall-off for the peak height
    //            islandHeights[x, y] += peakHeightModifier * (1 - distanceToPeak) * 0.5f; // Apply modifier to height

    //            // If within the sea ratio, transition to the sea level
    //            float distanceToEdge = Mathf.Min(x / (float)width, (float)(width - x) / width, y / (float)length, (float)(length - y) / length);
    //            if (distanceToEdge < seaRatio)
    //            {
    //                islandHeights[x, y] = Mathf.Lerp(islandHeights[x, y], seaLevel, (1 - distanceToEdge / seaRatio));
    //            }
    //        }
    //    }

    //    return islandHeights;
    //}
    float[,] GenerateIsland(float variation, float mountainPercentage)
    {
        float[,] heights = new float[width, length];

        int layers = 5; // Shared layers for both valleys and mountains
        float persistence = 0.5f;
        float lacunarity = 2.0f;

        Vector2 peakPosition = new Vector2(UnityEngine.Random.Range(0f, width), UnityEngine.Random.Range(0f, length));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float valleyNoise = GenerateLayeredPerlinNoise(x, y, layers, scale * variation, persistence, lacunarity);
                float mountainNoise = GenerateLayeredPerlinNoise(x, y, layers, scale * variation, persistence, lacunarity);

                // Blend based on mountainPercentage
                float blendedNoise = Mathf.Lerp(valleyNoise, mountainNoise, mountainPercentage);

                // Modify heights based on proximity to the peak
                float distanceToPeak = Vector2.Distance(new Vector2(x, y), peakPosition) / Mathf.Max(width, length);
                float peakModifier = Mathf.Exp(-distanceToPeak * 5f);
                heights[x, y] = blendedNoise + peakModifier * (1 - distanceToPeak) * 0.5f;

                // Transition to sea level if near edges
                float distanceToEdge = Mathf.Min(x / (float)width, (float)(width - x) / width, y / (float)length, (float)(length - y) / length);
                if (distanceToEdge < seaRatio)
                {
                    heights[x, y] = Mathf.Lerp(heights[x, y], seaLevel, (1 - distanceToEdge / seaRatio));
                }
            }
        }

        return heights;
    }
    float GenerateLayeredPerlinNoise(float x, float y, int layers, float baseScale, float persistence, float lacunarity)
    {
        float noiseValue = 0f;
        float amplitude = 1f; // Initial amplitude
        float frequency = 1f; // Initial frequency (inverse of scale)
        float maxAmplitude = 0f; // Used for normalization

        for (int i = 0; i < layers; i++)
        {
            float perlin = Mathf.PerlinNoise(x / (baseScale / frequency), y / (baseScale / frequency));
            noiseValue += perlin * amplitude;

            maxAmplitude += amplitude;
            amplitude *= persistence; // Reduce amplitude for the next layer
            frequency *= lacunarity;  // Increase frequency for the next layer
        }

        return noiseValue / maxAmplitude; // Normalize to range [0, 1]
    }



    void PaintTerrainTextures(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;

        // Set terrain layers for painting, including water
        terrainData.terrainLayers = new TerrainLayer[] { sandLayer, grassLayer, rockLayer, waterLayer };

        // Alpha map should be the same resolution as the terrain
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;
        float[,,] alphaMap = new float[alphamapWidth, alphamapHeight, terrainData.terrainLayers.Length];

        for (int x = 0; x < alphamapWidth; x++)
        {
            for (int y = 0; y < alphamapHeight; y++)
            {
                // Normalize x and y to the heightmap resolution
                float normX = (float)x / alphamapWidth;
                float normY = (float)y / alphamapHeight;

                // Convert normalized coordinates to heightmap resolution
                int heightMapX = Mathf.FloorToInt(normX * (terrainData.heightmapResolution - 1));
                int heightMapY = Mathf.FloorToInt(normY * (terrainData.heightmapResolution - 1));

                // Get the height value at the current position
                float height = heights[heightMapX, heightMapY];

                // Assign texture weights based on height
                float[] textureWeights = new float[terrainData.terrainLayers.Length];

                if (height <= 0f)
                {
                    textureWeights[3] = 1f; // Water texture
                }
                else if (height < sandHeight)
                {
                    textureWeights[0] = 1f; // Sand texture
                }
                else if (height < grassHeight)
                {
                    textureWeights[1] = 1f; // Grass texture
                }
                else if (height < rockHeight)
                {
                    textureWeights[2] = 1f; // Rock texture
                }

                // Normalize texture weights so that their sum equals 1
                float sum = 0f;
                for (int i = 0; i < textureWeights.Length; i++)
                {
                    sum += textureWeights[i];
                }

                if (sum > 0)
                {
                    for (int i = 0; i < textureWeights.Length; i++)
                    {
                        textureWeights[i] /= sum;
                    }
                }

                // Set the alpha values for the current x, y position
                alphaMap[x, y, 0] = textureWeights[0]; // Sand
                alphaMap[x, y, 1] = textureWeights[1]; // Grass
                alphaMap[x, y, 2] = textureWeights[2]; // Rock
                alphaMap[x, y, 3] = textureWeights[3]; // Water
            }
        }

        // Apply the alpha map to the terrain
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }


    void ApplyHeightsToTerrain(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);

        terrainData.SetHeights(0, 0, heights);
        terrain.terrainData = terrainData;

        // Ensure the TerrainCollider uses the same TerrainData
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.terrainData = terrainData;
        }
    }
}