using System.Collections.Generic;
using UnityEngine;

public class BaseLayoutGenerator : MonoBehaviour
{
    // Grid dimensions
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;

    // Define types of zones
    private enum ZoneType { Empty, Entry, Corridor, Command, Defense, Wall }
    private ZoneType[,] grid;

    // Colors for visualizing zones
    [Header("Zone Colors")]
    public Color entryColor = Color.green;
    public Color corridorColor = Color.yellow;
    public Color commandColor = Color.red;
    public Color defenseColor = Color.blue;
    public Color wallColor = Color.black;
    public Color emptyColor = Color.gray;

    // Zone allocation percentages
    [Header("Zone Percentages")]
    [Range(0, 0.5f)] public float commandZonePercentage = 0.1f;
    [Range(0, 1)] public float corridorZonePercentage = 0.25f;
    [Range(0, 1)] public float wallZonePercentage = 0.1f;
    [Range(0, 1)] public float defenseZonePercentage = 0.2f;
    [Range(0, 1)] public float entryZonePercentage = 0.05f;

    private void OnValidate()
    {
        GenerateBaseLayout();
    }

    private void Start()
    {
        GenerateBaseLayout();
    }

    private void GenerateBaseLayout()
    {
        InitializeGrid();
        AssignEntryZones();
        AssignCorridorZones();
        AssignCommandZones();
        AssignDefensiveZones();
        AssignWallZones();
        MakeEmptyZonesCorridors();
    }

    private void InitializeGrid()
    {
        grid = new ZoneType[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = ZoneType.Empty;
            }
        }
    }

    private void AssignEntryZones()
    {
        int entryZoneCount = Mathf.FloorToInt(gridWidth * gridHeight * entryZonePercentage);

        for (int i = 0; i < entryZoneCount; i++)
        {
            int edge = Random.Range(0, 4);
            int x = 0, y = 0;

            switch (edge)
            {
                case 0: x = Random.Range(0, gridWidth); y = 0; break;
                case 1: x = Random.Range(0, gridWidth); y = gridHeight - 1; break;
                case 2: x = 0; y = Random.Range(0, gridHeight); break;
                case 3: x = gridWidth - 1; y = Random.Range(0, gridHeight); break;
            }

            if (grid[x, y] == ZoneType.Empty)
            {
                grid[x, y] = ZoneType.Entry;
            }
        }
    }

    private void AssignCorridorZones()
    {
        int corridorZoneCount = Mathf.FloorToInt(gridWidth * gridHeight * corridorZonePercentage);

        for (int i = 0; i < corridorZoneCount; i++)
        {
            Vector2 entryPos = FindRandomEntryZone();
            if (entryPos != Vector2.negativeInfinity)
            {
                CreateCorridor((int)entryPos.x, (int)entryPos.y);
            }
        }
    }

    private Vector2 FindRandomEntryZone()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ZoneType.Entry)
                {
                    return new Vector2(x, y);
                }
            }
        }
        return Vector2.negativeInfinity;
    }

    private void CreateCorridor(int entryX, int entryY)
    {
        for (int x = entryX; x < gridWidth; x++)
        {
            if (grid[x, entryY] == ZoneType.Empty)
                grid[x, entryY] = ZoneType.Corridor;
        }

        for (int y = entryY; y < gridHeight; y++)
        {
            if (grid[entryX, y] == ZoneType.Empty)
                grid[entryX, y] = ZoneType.Corridor;
        }
    }

    private void AssignCommandZones()
    {
        int commandZoneCount = Mathf.FloorToInt(gridWidth * gridHeight * commandZonePercentage);
        int placedCommandZones = 0;

        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ZoneType.Empty)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        ShuffleList(availablePositions);

        int clusterSize = Mathf.FloorToInt(commandZoneCount * 0.5f);
        Vector2Int clusterCenter = availablePositions[Random.Range(0, availablePositions.Count)];
        for (int i = 0; i < clusterSize; i++)
        {
            int x = clusterCenter.x + Random.Range(-1, 3);
            int y = clusterCenter.y + Random.Range(-1, 3);

            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && grid[x, y] == ZoneType.Empty)
            {
                grid[x, y] = ZoneType.Command;
                placedCommandZones++;
            }
        }

        foreach (var position in availablePositions)
        {
            if (placedCommandZones >= commandZoneCount)
                break;

            int x = position.x;
            int y = position.y;

            if (grid[x, y] == ZoneType.Empty)
            {
                grid[x, y] = ZoneType.Command;
                placedCommandZones++;
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void AssignDefensiveZones()
    {
        int defenseZoneCount = Mathf.FloorToInt(gridWidth * gridHeight * defenseZonePercentage);
        int placedDefenseZones = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ZoneType.Command)
                {
                    PlaceDefensiveZonesAroundCommand(x, y, ref defenseZoneCount, ref placedDefenseZones);
                }
            }
        }

        if (placedDefenseZones < defenseZoneCount)
        {
            Debug.LogWarning("Not enough space to place all defensive zones around command centers.");
        }
    }

    private void PlaceDefensiveZonesAroundCommand(int commandX, int commandY, ref int defenseZoneCount, ref int placedDefenseZones)
    {
        int radius = 2;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int newX = commandX + dx;
                int newY = commandY + dy;

                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight && grid[newX, newY] == ZoneType.Empty)
                {
                    grid[newX, newY] = ZoneType.Defense;
                    placedDefenseZones++;

                    if (placedDefenseZones >= defenseZoneCount)
                    {
                        return;
                    }
                }
            }
        }
    }

    private void AssignWallZones()
    {
        int wallZoneCount = Mathf.FloorToInt(gridWidth * gridHeight * wallZonePercentage);
        int placedWallZones = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ZoneType.Defense)
                {
                    PlaceWallsAroundZone(x, y, ref placedWallZones, wallZoneCount);
                }
            }
        }
    }

    private void PlaceWallsAroundZone(int zoneX, int zoneY, ref int placedWallZones, int maxWallCount)
    {
        int wallRadius = 1;

        for (int dx = -wallRadius; dx <= wallRadius; dx++)
        {
            for (int dy = -wallRadius; dy <= wallRadius; dy++)
            {
                int newX = zoneX + dx;
                int newY = zoneY + dy;

                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight && grid[newX, newY] == ZoneType.Empty && placedWallZones < maxWallCount)
                {
                    grid[newX, newY] = ZoneType.Wall;
                    placedWallZones++;
                }
            }
        }
    }

    private void MakeEmptyZonesCorridors()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ZoneType.Empty)
                {
                    grid[x, y] = ZoneType.Corridor;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Gizmos.color = GetZoneColor(grid[x, y]);
                Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
            }
        }
    }

    private Color GetZoneColor(ZoneType zoneType)
    {
        return zoneType switch
        {
            ZoneType.Entry => entryColor,
            ZoneType.Corridor => corridorColor,
            ZoneType.Command => commandColor,
            ZoneType.Defense => defenseColor,
            ZoneType.Wall => wallColor,
            _ => emptyColor,
        };
    }
}
