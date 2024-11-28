using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CalderaGenerator : MonoBehaviour
{
    public int width = 100; // Width of the terrain
    public int depth = 100; // Depth of the terrain
    public float scale = 20f; // Scale for Perlin noise
    public float maxTerrainHeight = 10f; // Max height of the terrain
    public float calderaRadius = 40f; // Radius of the caldera
    public float calderaDepth = 20f; // Depth of the caldera
    public float rimHeight = 15f; // Height of the caldera rim
    public float smoothness = 5f; // Smoothness factor for transitions

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateCaldera();
    }

    void GenerateCaldera()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
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

                // Generate base height using Perlin noise
                float height = Mathf.PerlinNoise(xCoord, zCoord) * maxTerrainHeight;
                vertices[x * (depth + 1) + z] = new Vector3(x, height, z);
            }
        }

        // Create caldera effect
        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                // Calculate distance from the center of the caldera
                float centerX = width / 2f;
                float centerZ = depth / 2f;
                float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));

                if (distanceToCenter < calderaRadius)
                {
                    // Adjust height for the caldera
                    float depthFactor = Mathf.SmoothStep(0, 1, distanceToCenter / calderaRadius);
                    vertices[x * (depth + 1) + z].y -= calderaDepth * depthFactor;

                    // Smoothly raise the rim
                    if (distanceToCenter >= calderaRadius - smoothness)
                    {
                        float rimFactor = Mathf.SmoothStep(0, 1, (calderaRadius - distanceToCenter) / smoothness);
                        vertices[x * (depth + 1) + z].y += rimHeight * rimFactor;
                    }
                }
            }
        }

        // Apply smoothing across the terrain
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
}
