using UnityEngine;
using System.Collections.Generic;

public class RiverGenerator : MonoBehaviour
{
    public Terrain terrain;
    public int caveRadius = 10;
    public Vector3 caveCenter;

    [Header("Terrain Dimensions")]
    public int width = 512;
    public int length = 512;
    public int height = 50;

    [Header("General Noise Settings")]
    public float scale = 50f;

    [Header("Feature-Specific Parameters")]
    public float valleyDepth = 0.7f; // Increased depth for more pronounced valleys
    public float valleySharpness = 0.7f; // Sharper gradients for more steep slopes

    [Header("Terrain Layer Reference")]
    public TerrainLayer valleyLayer; // Reference to the TerrainLayer (e.g., texture for valleys)

    private float[,] heights;
    private List<Vector3> caveNetwork;  // List of points forming the cave network
    private float[,] valleyDepths; // To store the depth of each valley

    // Enum to select which part of the terrain to highlight
    public enum TerrainFeature
    {
        Valleys,
        Flat,
        Rivers,
        Highlands
    }

    public TerrainFeature featureToHighlight = TerrainFeature.Valleys; // Default to highlighting valleys

    void Start()
    {
        // Step 1: Generate Valleys with Sharp Slopes and Apply to Terrain
        float valleyDepthVariation = Random.Range(0.3f, 0.7f) * 0.4f + (1 - 0.4f);
        heights = GenerateValleys(valleyDepthVariation);
        ApplyHeightsToTerrain(heights);

        // Step 2: Paint the terrain based on the selected feature
        PaintTerrainFeature();
    }

    float[,] GenerateValleys(float valleyVariation)
    {
        float[,] heights = new float[width, length];
        valleyDepths = new float[width, length]; // Initialize valley depth array

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float gradient = Mathf.Abs((float)x / width - 0.5f);
                float noise = Mathf.PerlinNoise(x / (scale * valleyVariation), y / (scale * valleyVariation)); // Apply randomness to noise scale

                // Create valleys with sharper gradients and deeper depth
                float valleyHeight = noise * Mathf.Pow(1 - gradient * valleyDepth, valleySharpness); // Use pow to sharpen the valleys
                heights[x, y] = valleyHeight;

                // Calculate the valley depth for painting (negative values indicate lower areas)
                valleyDepths[x, y] = 1 - valleyHeight; // Deeper valleys have higher depth values
            }
        }

        return heights;
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

    void PaintTerrainFeature()
    {
        TerrainData terrainData = terrain.terrainData;

        // Ensure the terrain has at least one texture layer set via reference
        if (valleyLayer == null)
        {
            Debug.LogError("Please assign a Terrain Layer for the valley texture in the inspector.");
            return;
        }

        // Add the manual layer to the terrain's terrain layers
        terrainData.terrainLayers = new TerrainLayer[] { valleyLayer };

        // Get the splat map (alpha maps) data of the terrain
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, length);

        // Iterate through each pixel of the terrain's heightmap
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float depth = valleyDepths[x, y];

                // Based on selected feature, decide which area to highlight
                switch (featureToHighlight)
                {
                    case TerrainFeature.Valleys:
                        HighlightValleys(x, y, depth, splatmapData);
                        break;
                    case TerrainFeature.Flat:
                        HighlightFlatAreas(x, y, depth, splatmapData);
                        break;
                    case TerrainFeature.Rivers:
                        HighlightRivers(x, y, depth, splatmapData);
                        break;
                    case TerrainFeature.Highlands:
                        HighlightHighlands(x, y, depth, splatmapData);
                        break;
                }
            }
        }

        // Apply the modified splatmap to the terrain
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void HighlightValleys(int x, int y, float depth, float[,,] splatmapData)
    {
        if (depth > 0.8f)
        {
            splatmapData[x, y, 0] = 1f; // Light texture for deep valleys
        }
        else
        {
            splatmapData[x, y, 0] = 0.2f; // Darker texture for shallower valleys
        }
    }

    void HighlightFlatAreas(int x, int y, float depth, float[,,] splatmapData)
    {
        if (depth >= 0.4f && depth <= 0.6f)
        {
            splatmapData[x, y, 0] = 1f; // Light texture for flat areas
        }
        else
        {
            splatmapData[x, y, 0] = 0.2f; // Darker texture for non-flat areas
        }
    }

    void HighlightRivers(int x, int y, float depth, float[,,] splatmapData)
    {
        // Rivers are typically in the lowest parts of the terrain
        if (depth > 0.85f)
        {
            splatmapData[x, y, 0] = 0.8f; // Lighter texture for riverbeds
        }
        else
        {
            splatmapData[x, y, 0] = 0.3f; // Darker texture for higher land
        }
    }


    void HighlightHighlands(int x, int y, float depth, float[,,] splatmapData)
    {
        // Highland areas would have the highest depth values
        if (depth < 0.3f)
        {
            splatmapData[x, y, 0] = 0.9f; // Light texture for highlands
        }
        else
        {
            splatmapData[x, y, 0] = 0.4f; // Darker texture for non-highland areas
        }
    }
}
