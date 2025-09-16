using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGen : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap ground, ocean, foreground, background;

    [Header("Tiles")]
    public List<TileBase[,]> premadePlatformTiles;
    public Tile water;
    public RuleTile dirt, brick;

    [Header("Prefabs")]
    public GameObject cratePrefab, enemyPrefab;

    private PrefabPool cratePool, enemyPool;
    private List<(GameObject obj, Vector3Int pos)> activeCrates = new();
    private List<(GameObject obj, Vector3Int pos)> activeEnemies = new();

    private Vector3Int lastPlatformPos, lastWaterPos;
    private HashSet<Vector3Int> oceanPositions = new(), platformPositions = new();

    [Header("Generation Settings")]
    public int trackingRange = 20, yPlatformLevel = 0;
    private int yWaterLevel = -5;
    private const int cleanupThreshold = 50;

    private Camera mainCamera;

    public enum PlatformType
    {
        Dirt,
        Brick,
        Hill,
        Stairs,
        Pyramid,
        RevPyramid,
        Triple,
        Gapped,
    }

    [Header("Decorative Tiles")]
    public Tile[] decorTiles;

    // New struct to hold platform and decoration info
    private struct PlatformConfig
    {
        public TileBase[,] platformTiles;
        public bool[,] foregroundMask;
        public bool[,] backgroundMask;
        public PlatformConfig(TileBase[,] platform, bool[,] foreground = null, bool[,] background = null)
        {
            platformTiles = platform;
            foregroundMask = foreground;
            backgroundMask = background;
        }
    }

    private Dictionary<PlatformType, PlatformConfig> platformConfigs;

    private readonly HashSet<PlatformType> enemySpawnableTypes = new()
    {
        PlatformType.Brick,
        PlatformType.RevPyramid,
    };

    void Start()
    {
        mainCamera = Camera.main;
        lastPlatformPos = lastWaterPos = Vector3Int.RoundToInt(mainCamera.transform.position);

        cratePool = cratePrefab ? new PrefabPool(cratePrefab) : null;
        enemyPool = enemyPrefab ? new PrefabPool(enemyPrefab) : null;

        // Initialize platformConfigs here, after dirt and brick are available
        platformConfigs = new Dictionary<PlatformType, PlatformConfig>
        {
            { PlatformType.Dirt, new PlatformConfig(
                new TileBase[,] { { dirt }, { dirt }, { dirt }, { dirt }, { dirt }, { dirt }, { dirt }, { dirt }, { dirt }, { dirt } },
                new bool[,] { { false }, { false }, { false }, { true }, { false }, { true }, { true }, { false }, { false }, { false } },
                new bool[,] { { false }, { true }, { true }, { false }, { true }, { false }, { false }, { true }, { true }, { false } }
            ) },
            { PlatformType.Brick, new PlatformConfig(new TileBase[,] { { brick }, { brick }, { brick }, { brick }, { brick }, { brick }, { brick }, { brick }, { brick }, { brick } }) },
            
            // Add more platforms here
            { PlatformType.Hill, new PlatformConfig(new TileBase[,] {
                { dirt, null },
                { dirt, dirt },
                { dirt, dirt },
                { dirt, null }
            }) },
            { PlatformType.Stairs, new PlatformConfig(new TileBase[,] {
                { brick, null, null },
                { brick, brick, null },
                { brick, brick, brick },
                { null, brick, brick },
                { null, null, brick }
            }) },
            { PlatformType.Pyramid, new PlatformConfig(new TileBase[,] {
                { brick, null, null },
                { brick, brick, null },
                { brick, brick, brick },
                { brick, brick, brick },
                { brick, brick, brick },
                { brick, brick, null },
                { brick, null, null }
            }) },
            { PlatformType.RevPyramid, new PlatformConfig(new TileBase[,] {
                { null, null, brick },
                { null, brick, brick },
                { brick, brick, brick },
                { brick, brick, brick },
                { brick, brick, brick },
                { null, brick, brick },
                { null, null, brick }
            }) },
            { PlatformType.Triple, new PlatformConfig(new TileBase[,] {
                { brick, null, null, null, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, null, null, null, brick }
            }) },
            { PlatformType.Gapped, new PlatformConfig(new TileBase[,] {
                { brick, null, null, null, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, brick, null, null, brick },
                { brick, null, null, null, null, null, brick },
                { null, null, null, null, null, null, null },
                { null, null, null, null, null, null, null },
                { null, brick, null, null, null, brick, null },
                { null, brick, null, null, null, brick, null },
                { null, null, null, null, null, null, null },
                { null, null, null, null, null, null, null },
                { null, null, null, brick, null, null, null },
                { null, null, null, brick, null, null, null },
            }) },
        }; ;
    }

    void Update()
    {
        float camX = mainCamera.transform.position.x;
        if (camX + trackingRange > lastPlatformPos.x)
            GeneratePlatforms(camX);
        Cleanup(camX);
    }

    public void GeneratePlatforms(float camX)
    {
        if (platformConfigs == null || platformConfigs.Count == 0) return;
        GenerateWater(camX);

        var types = new List<PlatformType>(platformConfigs.Keys);

        while (lastPlatformPos.x < camX + trackingRange)
        {
            PlatformType type = types[Random.Range(0, types.Count)];
            PlatformConfig config = platformConfigs[type];
            TileBase[,] platform = config.platformTiles;
            if (platform == null || platform.GetLength(0) == 0) continue;

            int w = platform.GetLength(0), h = platform.GetLength(1);
            int randomSpacing = Random.Range(1, 6);
            Vector3Int start = new Vector3Int(lastPlatformPos.x + randomSpacing, yPlatformLevel, 0);

            // Place platform tiles
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (platform[x, y] != null && platformPositions.Add(new Vector3Int(start.x + x, start.y + y, 0)))
                        ground.SetTile(new Vector3Int(start.x + x, start.y + y, 0), platform[x, y]);

            // Place foreground decorative tiles if present
            if (config.foregroundMask != null)
            {
                int lastDecorIndex = -1;
                for (int x = 0; x < config.foregroundMask.GetLength(0); x++)
                    for (int y = 0; y < config.foregroundMask.GetLength(1); y++)
                        if (config.foregroundMask[x, y])
                        {
                            int decorIndex;
                            do
                            {
                                decorIndex = GetWeightedDecorIndex();
                            } while (decorTiles.Length > 1 && decorIndex != 0 && decorIndex == lastDecorIndex);
                            lastDecorIndex = decorIndex;

                            Tile randomDecor = decorTiles[decorIndex];
                            foreground.SetTile(new Vector3Int(start.x + x, start.y + y + 1, 0), randomDecor);
                        }
            }

            // Place background decorative tiles if present
            if (config.backgroundMask != null)
            {
                int lastDecorIndex = -1;
                for (int x = 0; x < config.backgroundMask.GetLength(0); x++)
                    for (int y = 0; y < config.backgroundMask.GetLength(1); y++)
                        if (config.backgroundMask[x, y])
                        {
                            int decorIndex;
                            do
                            {
                                decorIndex = GetWeightedDecorIndex();
                            } while (decorTiles.Length > 1 && decorIndex != 0 && decorIndex == lastDecorIndex);
                            lastDecorIndex = decorIndex;

                            Tile randomDecor = decorTiles[decorIndex];
                            background.SetTile(new Vector3Int(start.x + x, start.y + y + 1, 0), randomDecor);
                        }
            }

            int centerX = w / 2;
            Vector3Int topCell = new Vector3Int(start.x + centerX, start.y + h, 0);

            if (enemySpawnableTypes.Contains(type))
                SpawnEnemy(topCell, start, w, h, platform, type);

            lastPlatformPos = new Vector3Int(start.x + w - 1, start.y, start.z);
        }
    }

    void GenerateWater(float camX)
    {
        int startX = Mathf.FloorToInt(camX) - trackingRange, endX = Mathf.FloorToInt(camX) + trackingRange + 10;
        for (int x = startX; x <= endX; x++)
        {
            Vector3Int waterPos = new Vector3Int(x, yWaterLevel, 0);
            if (oceanPositions.Add(waterPos))
                ocean.SetTile(waterPos, water);
        }
        lastWaterPos = new Vector3Int(endX, lastWaterPos.y, lastWaterPos.z);
    }

    void Cleanup(float camX)
    {
        oceanPositions.RemoveWhere(pos =>
        {
            if (pos.x < camX - cleanupThreshold * 2)
            {
                ocean.SetTile(pos, null);
                return true;
            }
            return false;
        });

        platformPositions.RemoveWhere(pos =>
        {
            if (pos.x < camX - cleanupThreshold)
            {
                ground.SetTile(pos, null);
                return true;
            }
            return false;
        });

        activeCrates.RemoveAll(item =>
        {
            if (item.pos.x < camX - cleanupThreshold)
            {
                cratePool?.Return(item.obj);
                return true;
            }
            return false;
        });

        activeEnemies.RemoveAll(item =>
        {
            if (item.pos.x < camX - cleanupThreshold)
            {
                enemyPool?.Return(item.obj);
                return true;
            }
            return false;
        });
    }

    void SpawnCrate(Vector3Int cell)
    {
        if (cratePool == null) return;
        Vector3 worldPos = ground.GetCellCenterWorld(cell); worldPos.z = 0f;
        activeCrates.Add((cratePool.Get(worldPos, Quaternion.identity), cell));
    }

    void SpawnEnemy(Vector3Int cell, Vector3Int start, int w, int h, TileBase[,] platform, PlatformType type)
    {
        if (enemyPool == null) return;
        Vector3 worldPos = ground.GetCellCenterWorld(cell); worldPos.z = 0f;
        GameObject enemy = enemyPool.Get(worldPos, Quaternion.identity);
        activeEnemies.Add((enemy, cell));

        List<Vector2Int> topTiles = new List<Vector2Int>();
        for (int x = 0; x < w; x++)
            for (int y = h - 1; y >= 0; y--)
                if (platform[x, y] != null) { topTiles.Add(new Vector2Int(x, y)); break; }

        if (topTiles.Count == 0) return;
        Vector2Int left = topTiles[0]; var right = topTiles[0];
        foreach (Vector2Int t in topTiles) { if (t.x < left.x) left = t; if (t.x > right.x) right = t; }

        Vector3Int leftCell = new Vector3Int(start.x + left.x, start.y + left.y, 0);
        Vector3Int rightCell = new Vector3Int(start.x + right.x, start.y + right.y, 0);

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null) enemyScript.groundTilemap = ground;
    }

    public void ResetPlatforms(Vector3 respawnPos)
    {
        ground.ClearAllTiles(); ocean.ClearAllTiles(); foreground.ClearAllTiles(); background.ClearAllTiles();
        cratePool?.ReturnAll(); enemyPool?.ReturnAll();
        platformPositions.Clear(); oceanPositions.Clear();
        activeCrates.Clear(); activeEnemies.Clear();
        lastPlatformPos = lastWaterPos = Vector3Int.RoundToInt(respawnPos);
        GeneratePlatforms(respawnPos.x);
    }

    public Vector3 GetRespawnPositionAbovePlatform(float x)
    {
        // Find the platform position closest to the given x, and highest y
        Vector3Int? best = null;
        foreach (var pos in platformPositions)
        {
            if (best == null || 
                (Mathf.Abs(pos.x - Mathf.RoundToInt(x)) < Mathf.Abs(best.Value.x - Mathf.RoundToInt(x)) ||
                (pos.x == best.Value.x && pos.y > best.Value.y)))
            {
                best = pos;
            }
        }
        if (best != null)
        {
            // Place the player slightly above the platform
            Vector3 worldPos = ground.GetCellCenterWorld(best.Value);
            worldPos.y += 1.5f; // Adjust as needed for your player height
            worldPos.z = 0f;
            return worldPos;
        }
        // Fallback: use current position
        return new Vector3(x, yPlatformLevel + 2, 0f);
    }

    // Add this helper method inside your MapGen class
    private int GetWeightedDecorIndex()
    {
        // Example: index 0 is 1x, others are 3x as likely
        int[] weights = new int[decorTiles.Length];
        weights[0] = 1; // index 0 is less likely
        for (int i = 1; i < weights.Length; i++)
            weights[i] = 3; // other indices are more likely

        int total = 0;
        foreach (int w in weights) total += w;
        int r = Random.Range(0, total);
        int sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (r < sum) return i;
        }
        return 0; // fallback
    }
}
