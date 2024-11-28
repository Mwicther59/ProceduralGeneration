using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RiftValleyGenerator : MonoBehaviour
{
    public int width = 100;         // Width of the terrain
    public int depth = 100;         // Depth of the terrain
    public float scale = 20f;       // Scale for Perlin noise
    public float maxTerrainHeight = 10f; // Max height of the surrounding terrain
    public float valleyWidth = 30f; // Width of the rift valley
    public float valleyDepth = 20f; // Depth of the valley
    public float slopeSmoothness = 15f; // Smoothness factor for the valley slopes

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateRiftValley();
    }

    void GenerateRiftValley()
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
                float height = Mathf.PerlinNoise(xCoord, zCoord) * maxTerrainHeight;

                vertices[x * (depth + 1) + z] = new Vector3(x, height, z);
            }
        }

        // Create rift valley effect
        for (int x = 0; x <= width; x++)
        {
            float valleyCenterZ = depth / 2;  // Center of the valley along the z-axis

            for (int z = 0; z <= depth; z++)
            {
                float distanceToCenter = Mathf.Abs(z - valleyCenterZ);

                if (distanceToCenter < valleyWidth)
                {
                    // Adjust height for the valley
                    float valleyFactor = Mathf.SmoothStep(1, 0, distanceToCenter / valleyWidth);
                    vertices[x * (depth + 1) + z].y -= valleyDepth * valleyFactor;

                    // Smooth transition for valley slopes
                    float slopeAdjustment = Mathf.SmoothStep(0, 1, distanceToCenter / slopeSmoothness);
                    vertices[x * (depth + 1) + z].y -= slopeAdjustment * valleyDepth * 0.5f;
                }
            }
        }

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

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
