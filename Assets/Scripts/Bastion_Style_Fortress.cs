using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bastion_Style_Fortress : MonoBehaviour
{
    public GameObject cubePrefab;           // Reference to the cube prefab
    public int points = 5;                  // Number of points on the star (e.g., 5 for a pentagon-based star)
    public float outerRadius = 15f;         // Radius for the outer points of the star
    public float innerRadius = 10f;         // Radius for the inner points (determines "sharpness" of angles)
    public int cubesPerSegment = 10;        // Number of cubes to place between each point
    public float wallHeight = 3f;           // Height of the wall for each cube

    void Start()
    {
        GenerateBastionFortress();
    }

    void GenerateBastionFortress()
    {
        float angleStep = 360f / (points * 2); // Angle between each inner and outer point
        Vector3 lastOuterPoint = Vector3.zero;

        for (int i = 0; i < points * 2; i++)
        {
            // Determine whether this is an "outer" or "inner" point in the star
            float radius = (i % 2 == 0) ? outerRadius : innerRadius;
            float angle = i * angleStep;

            // Calculate x and z using polar coordinates for each point
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            Vector3 currentPoint = new Vector3(x, 0, z);

            // Place cubes between the last and current outer points to create the wall segment
            if (i > 0) // Skip the very first iteration as it has no "last point" yet
            {
                PlaceCubesBetweenPoints(lastOuterPoint, currentPoint);
            }

            lastOuterPoint = currentPoint; // Update last outer point
        }

        // Connect the last segment to the first to close the shape
        PlaceCubesBetweenPoints(lastOuterPoint, new Vector3(Mathf.Cos(0) * outerRadius, 0, Mathf.Sin(0) * outerRadius));
    }

    void PlaceCubesBetweenPoints(Vector3 start, Vector3 end)
    {
        // Calculate the direction and distance between start and end points
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        float stepSize = distance / cubesPerSegment;

        for (int i = 0; i <= cubesPerSegment; i++)
        {
            // Calculate position along the segment
            Vector3 position = start + direction * (i * stepSize);
            position.y = wallHeight / 2; // Set height to create a raised wall effect

            // Instantiate a cube at each calculated position
            Instantiate(cubePrefab, position, Quaternion.identity, transform);
        }
    }
}