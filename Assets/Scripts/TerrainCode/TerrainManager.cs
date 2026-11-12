using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private List<GameObject> oakPrefabs = new List<GameObject>();
    [SerializeField] private GameObject pinePrefab;
    [Header("Detail Prefabs")]
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private GameObject shroomPrefab;
    [SerializeField] private GameObject pineStumpPrefab;
    [SerializeField] private GameObject oakStumpPrefab;
    [SerializeField] private GameObject bushPrefab;
    [Header("Perlin variables")]
    [SerializeField] private int size = 10;                 // Size of 1 tile
    [SerializeField] private int depth = 15;                // depth allowed to the terrain
    [SerializeField] private float scale = 20f;             // Scale of Perlin Noise
    [SerializeField] private float heightVariability = 10f; // Variability of height between different regions
    [SerializeField] private int halfTileCount = 12;        // how many tiles extend from the player in each direction
    [Header("Object Spawn Variables")]
    [SerializeField] private int minDetails = 2;
    [SerializeField] private int maxDetails = 8;
    [SerializeField] private int detailRadius = 3;
    [SerializeField] private Dictionary<BiomesEnum, float[]> detailOdds = new Dictionary<BiomesEnum, float[]>();
    private GameObject[] detailArray;
    // Odds ordered as grass -> flower -> shroom -> pine stump -> oak stump -> bush

    // Perlin offsets
    private float offsetX;
    private float offsetY;
    private float smoothingXOffset;
    private float smoothingYOffset;
    private float heightXOffset;
    private float heightYOffset;

    // Voronoi variables
    private int voronoiRenderedSize;                        // size of voronoi grid to keep rendered
    private int voronoiGridHalfSize;                        // How many biome cells away from the player will be rendered at once
    private int pixelsPerCell;                              // scale of biome in tiles * size
    private int tilesInBiome = 5;                           // number of tiles to count in tilesize
    private int additionalBorder = 2;                       // how many extra biomes from the player to store for the Voronoi Grid

    private Dictionary<string, GameObject> activeTiles = new Dictionary<string, GameObject>();
    private Vector2Int lastPlayerTile;
    private GameObject player;
    private Dictionary<string, BiomeCenter> activeCellCenters = new Dictionary<string, BiomeCenter>();
    
    public static TerrainManager instance;

    private void Awake()
    {
        Singleton();
        DontDestroyOnLoad(gameObject);
    }

    private void Singleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        // Debug.Log("Done for " + this.name);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue); // force first generation
        offsetX = Random.Range(-9999f, 9999f);
        offsetY = Random.Range(-9999f, 9999f);
        smoothingXOffset = Random.Range(-9999f, 9999f);
        smoothingYOffset = Random.Range(-9999f, 9999f);
        heightXOffset = Random.Range(-9999f, 9999f);
        heightYOffset = Random.Range(-9999f, 9999f);

        pixelsPerCell = tilesInBiome * size;
        voronoiGridHalfSize = Mathf.CeilToInt((float)halfTileCount / (float)tilesInBiome) + additionalBorder;
        voronoiRenderedSize = voronoiGridHalfSize * pixelsPerCell;
        
        detailArray = new GameObject[6] {grassPrefab, flowerPrefab, shroomPrefab, pineStumpPrefab, oakStumpPrefab, bushPrefab};
    }

    void FixedUpdate()
    {
        // Get the x, z of Player in Tiles for Perlin
        Vector2Int currentPlayerTile = new Vector2Int(
        Mathf.RoundToInt(player.transform.position.x / size),
        Mathf.RoundToInt(player.transform.position.z / size));

        if (currentPlayerTile == lastPlayerTile) return;
        lastPlayerTile = currentPlayerTile;

        RegenerateVoronoiCentersAroundPlayer(GetCellFromTile(currentPlayerTile));
        RegenPerlinAroundPlayer(currentPlayerTile);
        // Can calculate new Voronoi Center by dividing currentPlayerTile by 5
        // Mimic Regenerate Around Player to find which Voronoi coords we need right now
        // For the cells that have 8 neighbors, calculate pixels adjacency
    }

    void RegenPerlinAroundPlayer(Vector2Int centerTile)
    {
        var stillNeeded = new HashSet<string>();
        for (int x = -halfTileCount; x <= halfTileCount; x++)
        {
            for (int z = -halfTileCount; z <= halfTileCount; z++)
            {
                int tileX = centerTile.x + x;
                int tileZ = centerTile.y + z;
                string tileName = $"Tile_{tileX}_{tileZ}";
                stillNeeded.Add(tileName);
                if (activeTiles.ContainsKey(tileName)) continue;

                GameObject tile = InstantiateTerrainObject(tileX, tileZ);
                tile.name = tileName;
                activeTiles.Add(tileName, tile);
            }
        }

        var toRemove = new List<string>();
        foreach (var kvp in activeTiles)
        {
            if (!stillNeeded.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove) activeTiles.Remove(key);
    }

    private void RegenerateVoronoiCentersAroundPlayer(Vector2Int centerCell)
    {
        var stillNeeded = new HashSet<string>();
        for(int x = -voronoiGridHalfSize; x<= voronoiGridHalfSize; x++)
        {
            for(int z = -voronoiGridHalfSize; z <= voronoiGridHalfSize; z++)
            {
                int cellX = centerCell.x + x;
                int cellZ = centerCell.y + z;
                string cellName = GetCellNameFromGrid(cellX, cellZ);
                stillNeeded.Add(cellName);
                if(activeCellCenters.ContainsKey(cellName)) continue;

                BiomeCenter newCenter = GenerateNewCenter(cellX, cellZ);
                activeCellCenters.Add(cellName, newCenter);
            }
        }

        var toRemove = new List<string>();
        foreach (var kvp in activeCellCenters)
        {
            if (!stillNeeded.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove) activeCellCenters.Remove(key);
    }

    private GameObject InstantiateTerrainObject(int x, int z)     // x, z -> coordinates in tile grid
    {
        var pos = GetWorldPositionOfTerrain(x, z);
        var biomeCell = GetCellFromTile(x, z);
        // Create tile at specific world location
        GameObject tile = Instantiate(planePrefab, pos, Quaternion.identity, transform);
        // Generate the Vertices according to Perlin
        GenerateVerts(tile, x, z);
        // Loop Through again to populate
        PopulateTile(tile, x, z, biomeCell);
        return tile;
    }

    private void GenerateVerts(GameObject plane, int xWorld, int zWorld)
    {
        Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        for(int v = 0; v < vertices.Length; v++)
        {
            vertices[v].y = Mathf.PerlinNoise((vertices[v].x + xWorld * size) / scale + offsetX, 
                                              (vertices[v].z + zWorld * size) / scale + offsetY) * 
                            Mathf.PerlinNoise((vertices[v].x + xWorld * size) / (scale*2) - smoothingXOffset, 
                                              (vertices[v].z + zWorld * size) / (scale*2) - smoothingYOffset) * depth + 
                            (Mathf.PerlinNoise((vertices[v].x + xWorld * size) / (scale*20) + heightXOffset, 
                                              (vertices[v].z + zWorld * size) / (scale*20) + heightYOffset) - 0.5f) * heightVariability;

        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        plane.gameObject.AddComponent<MeshCollider>();
    }

    private void PopulateTile(GameObject plane, int xWorld, int zWorld, Vector2Int biomeCell)
    {
        Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;             // 11 x 11 = 121

        int v = Random.Range(0, 120);
        BiomesEnum biome = GetPointBiome(new Vector2Int((int)vertices[v].x + xWorld * size, (int)vertices[v].z + zWorld * size), biomeCell);
        
        List<Vector3> detailSpawns = GetDetailPositions(vertices, v);
        
        HandleSpawnRules(biome, vertices[v], detailSpawns, plane);
    }

    private BiomesEnum GetPointBiome(Vector2Int point, Vector2Int biomeCell)
    {                       
        // Determine which biomeCenter we are in
        float nearest = Mathf.Infinity;
        BiomeCenter nearestCenter = new BiomeCenter();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var biomeCompKey = GetCellNameFromGrid(biomeCell.x + i, biomeCell.y + j);
                BiomeCenter bc = activeCellCenters[biomeCompKey];
                float dist = Vector2Int.Distance(bc.GetCoordinates(), point);
                if (dist < nearest)
                {
                    nearest = dist;
                    nearestCenter = bc;
                }
            }
        }

        var biome = nearestCenter.GetBiome();
        // Debug.Log(nearestCenter.GetBiome() + " at " + point.x + " " + point.y);
        return biome;
    }

    private List<Vector3> GetDetailPositions(Vector3[] verts, int center)
    {
        // Define bounds of detail positions
        int xMin = Mathf.Max((int)verts[center].x - detailRadius, -5);
        int xMax = Mathf.Min((int)verts[center].x + detailRadius, 5);
        int zMin = Mathf.Max((int)verts[center].z - detailRadius, -5);
        int zMax = Mathf.Min((int)verts[center].z + detailRadius, 5);
        int toSpawn = Random.Range(minDetails, maxDetails);

        List<Vector3> detailPos = new List<Vector3>();
        while (toSpawn > 0)
        {
            int x = Random.Range(xMin, xMax) + 5;
            int z = Random.Range(zMin, zMax) + 5;
            detailPos.Add(verts[x * 11 + z]);
            toSpawn--;
        }

        return detailPos;
    }

    private void HandleSpawnRules(BiomesEnum biome, Vector3 center, List<Vector3> details, GameObject tile)
    {
        switch(biome)
        {
            case BiomesEnum.Oaks:
                int oakIdx = Random.Range(0, oakPrefabs.Count - 1);
                Instantiate(oakPrefabs[oakIdx], center + tile.transform.position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), tile.transform);
                // Instantiates with random rotation around the y and parent it to tile
                break;
            case BiomesEnum.Pines:
                Instantiate(pinePrefab, center + tile.transform.position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), tile.transform);
                // Instantiates with random rotation around the y and parent it to tile
                break;
            case BiomesEnum.Plains:
                // Don't spawn anything in plains, just details
                break;
            default:
                Debug.Log("huh");
                break;
        }

        var odds = detailOdds[biome];
        foreach (Vector3 coord in details)
        {
            // spawn random detail according to details in biome
            float rng = Random.Range(0f, 1f);
            float threshold = 0f;

            for(int i = 0; i < odds.Length; i++)
            {
                threshold += odds[i];
                if (rng <= threshold)
                {
                    Instantiate(detailArray[i], coord + tile.transform.position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), tile.transform);
                    break;
                }
            }
        }
    }

    private Vector3 GetWorldPositionOfTerrain(int x, int z)
    {
        return new Vector3((float) x * size, 0f, (float) z * size);
    }

    private Vector2Int GetCellFromTile(Vector2Int tile)
    {
        return new Vector2Int(Mathf.RoundToInt((float)tile.x / (float)tilesInBiome), Mathf.RoundToInt((float)tile.y / (float)tilesInBiome));
    }

    private Vector2Int GetCellFromTile(int x, int z)
    {
        return new Vector2Int(Mathf.RoundToInt((float)x / (float)tilesInBiome), Mathf.RoundToInt((float)z / (float)tilesInBiome));
    }

    private string GetCellNameFromGrid(int x, int z)
    {
        return $"Cell_{x}_{z}";
    }

    private BiomeCenter GenerateNewCenter(int x, int z)
    {
        var biome = (BiomesEnum)Random.Range(0, System.Enum.GetValues(typeof(BiomesEnum)).Length);
        return new BiomeCenter(biome, x * pixelsPerCell + Random.Range(0, pixelsPerCell), z * pixelsPerCell + Random.Range(0, pixelsPerCell));
    }
}