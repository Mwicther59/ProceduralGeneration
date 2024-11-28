using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralGenerater : MonoBehaviour
{
    public GameObject cubePrefab;         // Reference to the cube prefab
    public int totalSteps = 100;          // Total number of cubes to place in the spiral
    public float initialRadius = 10f;     // Starting radius of the spiral
    public float radiusIncrement = 0.2f;  // How much the radius increases with each step
    public float angleIncrement = 15f;    // Angle step in degrees to control spiral tightness
    public float heightOffset = 0f;       // Height offset to create a flat spiral or add vertical steps

    void Start()
    {
        GenerateSpiralOutline();
    }

    void GenerateSpiralOutline()
    {
        float currentRadius = initialRadius;
        float currentAngle = 0f;

        for (int i = 0; i < totalSteps; i++)
        {
            // Calculate the x and z position using polar coordinates
            float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * currentRadius;
            float z = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * currentRadius;
            float y = i * heightOffset; // Optional height offset for stepped spirals

            // Instantiate a cube at the calculated position
            Vector3 position = new Vector3(x, y, z);
            Instantiate(cubePrefab, position, Quaternion.identity, transform);

            // Update the radius and angle for the next cube
            currentRadius += radiusIncrement;
            currentAngle += angleIncrement;
        }
    }
}