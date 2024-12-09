using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotate : MonoBehaviour
{
    [SerializeField] private Transform terrain; // The object to look at
    [SerializeField] private float rotationSpeed = 5f; // Speed for smooth rotation
    [SerializeField] private bool enableSmoothRotation = true; // Toggle for smooth rotation

    void Update()
    {
        if (terrain == null)
        {
            Debug.LogWarning("Terrain Transform is not assigned. Please assign a target in the Inspector.");
            return;
        }

        if (enableSmoothRotation)
        {
            // Smoothly rotate to look at the terrain
            Vector3 direction = terrain.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Instantly look at the terrain
            transform.LookAt(terrain);
        }
    }
}