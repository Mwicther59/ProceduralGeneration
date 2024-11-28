using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CanyonGenerator : MonoBehaviour
{
    public int width = 100;
    public int depth = 100;
    public float scale = 20f;
    public float maxTerrainHeight = 10f;
    public float canyonWidth = 15f;
    public float canyonDepth = 20f;
    public float cliffSteepness = 5f;
    public float riverHeightOffset = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateCanyon();
    }

    void GenerateCanyon()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
        CreateRiver();
    }

    void CreateShape()
    {
        // Create vertices array
        vertices = new Vector3[(width + 1) * (depth + 1)];

        // Generate base Perlin noise terrain
        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                float xCoord = (float)x / width * scale;
                float zCoord = (float)z / depth * scale;

                // Layered Perlin noise for more variation
                float primaryNoise = Mathf.PerlinNoise(xCoord, zCoord) * maxTerrainHeight;
                float secondaryNoise = Mathf.PerlinNoise(xCoord * 2, zCoord * 2) * (maxTerrainHeight / 4); // Higher frequency, lower amplitude
                float height = primaryNoise + secondaryNoise;

                vertices[x * (depth + 1) + z] = new Vector3(x, height, z);
            }
        }

        // Carve out the canyon path with smooth falloff
        for (int x = 0; x <= width; x++)
        {
            float canyonCenterZ = Mathf.Sin(x * 0.1f) * 10f + depth / 2; // Adjust the 0.1f for tighter/wider curves
            for (int z = 0; z <= depth; z++)
            {
                float distanceToCenter = Mathf.Abs(z - canyonCenterZ);
                if (distanceToCenter < canyonWidth)
                {
                    // Smooth falloff for canyon depth
                    float depthFactor = SmoothFalloff(distanceToCenter, canyonWidth);
                    vertices[x * (depth + 1) + z].y = Mathf.Lerp(vertices[x * (depth + 1) + z].y, -canyonDepth, depthFactor);
                }
            }
        }

        // Apply smoothing filter to soften terrain
        SmoothTerrain();

        // Create triangles array with reversed winding order
        triangles = new int[width * depth * 6];
        int vert = 0;
        int tris = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + 1;
                triangles[tris + 2] = vert + depth + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + depth + 2;
                triangles[tris + 5] = vert + depth + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    float SmoothFalloff(float distance, float width)
    {
        // Smooth step function for falloff
        return Mathf.SmoothStep(0, 1, 1 - (distance / width));
    }

    void SmoothTerrain()
    {
        Vector3[] smoothedVertices = (Vector3[])vertices.Clone();
        for (int x = 1; x < width; x++)
        {
            for (int z = 1; z < depth; z++)
            {
                int index = x * (depth + 1) + z;
                // Average the current vertex with its neighbors
                float averageHeight = (
                    vertices[index].y +
                    vertices[index + 1].y +
                    vertices[index - 1].y +
                    vertices[index + (depth + 1)].y +
                    vertices[index - (depth + 1)].y
                ) / 5f;
                smoothedVertices[index].y = averageHeight;
            }
        }
        vertices = smoothedVertices;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void CreateRiver()
    {
        GameObject river = GameObject.CreatePrimitive(PrimitiveType.Plane);
        river.transform.position = new Vector3(width / 2, -canyonDepth + riverHeightOffset, depth / 2);
        river.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        river.transform.localScale = new Vector3(canyonWidth / 10, 1, depth / 10);
        river.GetComponent<Renderer>().material.color = Color.blue; // Replace with a water shader if available
    }
}
