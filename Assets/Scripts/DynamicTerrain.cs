using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTerrain : MonoBehaviour
{
    public GameObject terrainPrefab;  // Prefab for terrain chunk
    public float chunkSize = 100f;    // Size of each terrain chunk
    public int renderDistance = 5;    // How many chunks to generate ahead of the player
    public float playerSpeed = 5f;    // Speed of the player

    private Transform player;         // Reference to the player's transform
    private Vector3 lastPlayerPosition;
    private Vector3 lastGeneratedPosition;

    private void Start()
    {
        // Try to find the player (main camera or tagged object)
        if (player == null)
        {
            // Find player by tag (ensure your player has a tag 'Player' in the Unity editor)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null)
            {
                Debug.LogError("Player object not found. Please assign the player manually.");
                return;
            }
        }

        lastPlayerPosition = player.position;
        lastGeneratedPosition = player.position;

        // Initial terrain generation
        GenerateTerrain();
    }

    private void Update()
    {
        if (player == null) return;  // Early return if player is null

        // If the player has moved far enough in one direction (either X or Z), update the terrain
        float distanceTraveled = Vector3.Distance(player.position, lastGeneratedPosition);
        if (distanceTraveled > chunkSize)
        {
            lastGeneratedPosition = player.position;
            GenerateTerrain();
        }
    }

    private void GenerateTerrain()
    {
        Vector3 playerPos = player.position;

        // Determine the center position of the new terrain based on player's position
        Vector3 newTerrainPos = new Vector3(playerPos.x, 0, playerPos.z);

        // Create new terrain chunks around the player
        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance; j++)
            {
                // Calculate spawn position for each chunk based on the player's position
                Vector3 spawnPosition = new Vector3(
                    Mathf.Round((newTerrainPos.x + i * chunkSize) / chunkSize) * chunkSize,
                    0,
                    Mathf.Round((newTerrainPos.z + j * chunkSize) / chunkSize) * chunkSize
                );

                // If the chunk hasn't been generated yet, create it
                if (!IsTerrainGenerated(spawnPosition))
                {
                    CreateTerrainChunk(spawnPosition);
                }
            }
        }

        // Remove old terrain chunks (those that are no longer within the render distance)
        DestroyOldChunks(newTerrainPos);
    }

    private void CreateTerrainChunk(Vector3 position)
    {
        // Add a random height offset to the Y-axis of the position
        float randomHeight = Random.Range(-10f, 10f); // Adjust the range as needed
        Vector3 chunkPosition = new Vector3(position.x, randomHeight, position.z);

        // Instantiate a new terrain chunk at the specified position with the random height
        Instantiate(terrainPrefab, chunkPosition, Quaternion.identity);
    }

    private void DestroyOldChunks(Vector3 newTerrainPos)
    {
        // Find all the terrain objects in the scene and destroy the ones that are too far from the player
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        foreach (Terrain terrain in terrains)
        {
            if (Vector3.Distance(terrain.transform.position, newTerrainPos) > (chunkSize * renderDistance))
            {
                Destroy(terrain.gameObject);
            }
        }
    }

    private bool IsTerrainGenerated(Vector3 position)
    {
        // Check if there's already a terrain chunk at the specified position
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        foreach (Terrain terrain in terrains)
        {
            if (terrain.transform.position == position)
            {
                return true; // A chunk already exists at this position
            }
        }
        return false;
    }
}