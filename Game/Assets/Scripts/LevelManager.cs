using System.Collections.Generic;
using UnityEngine;

// Defines the properties for buildings we want to spawn alongside the road
[System.Serializable]
public class BuildingType
{
    public string name;         
    public GameObject prefab;   
    public float width = 10f; // Used to calculate how many buildings fit on one tile
    public float rotationOffset = 0f; 
}

public class LevelManager : MonoBehaviour
{
    [Header("Road Configuration")]
    public GameObject[] roadPrefabs;
    public Transform playerTransform;
    public float tileLength = 30f; 
    public int numberOfTiles = 5;  // How many tiles exist at once

    [Header("Foreground Buildings")]
    public BuildingType[] availableBuildings; 
    public float buildingOffset = 25f; 

    [Header("Background Buildings")]
    public BuildingType[] backgroundBuildings; 
    public float backgroundOffset = 45f; 

    [Header("Ground/Sidewalk Configuration")]
    public GameObject concretePrefab; 
    public float concreteWidth = 40f; 
    public float roadOffset = 10f;     

    [Header("PowerUp Configuration")]
    public GameObject[] powerUpPrefabs; 
    [Range(0f, 1f)] public float powerUpSpawnChance = 0.3f; // 30% chance per tile
    public float hoverHeight = 1.0f; 
    public float[] lanePositions = { 0f, 10f, 20f }; // Exact X coordinates for the lanes

    private float spawnZ = 0f; 
    private float safeZone = 45f; // Prevents tiles from deleting while the player is still on them
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // Pre-spawn the first few tiles so the player has a road to start on
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i < 2) SpawnTile(0, false); // First 2 tiles are empty/safe
            else SpawnTile(Random.Range(1, roadPrefabs.Length), true);
        }
    }

    void Update()
    {
        // Check if the player has moved far enough to need a new tile ahead of them
        float threshold = spawnZ - (numberOfTiles * tileLength);
        float playerPos = playerTransform.position.z - safeZone;

        if (playerPos > threshold)
        {
            SpawnTile(Random.Range(1, roadPrefabs.Length), true);
            DeleteTile(); // Delete the one behind to save memory!
        }
    }

    // Creates the main road tile and triggers all the decorations to spawn
    void SpawnTile(int tileIndex, bool spawnItems)
    {
        GameObject go = Instantiate(roadPrefabs[tileIndex], transform.forward * spawnZ, transform.rotation);

        if (spawnItems)
        {
            SpawnSide(go, -buildingOffset); // Left side scenery
            SpawnSide(go, buildingOffset);  // Right side scenery
            
            SpawnPowerUp(go); // Try to spawn a power-up on the road
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    // Rolls a dice and spawns a random power-up hovering above a specific lane
    void SpawnPowerUp(GameObject roadTile)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        
        // Did we win the 30% chance to spawn?
        if (Random.value > powerUpSpawnChance) return; 
        
        if (lanePositions == null || lanePositions.Length == 0) return;

        // Pick a random power-up and a random lane
        GameObject selectedPowerUp = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        int randomLaneIndex = Random.Range(0, lanePositions.Length);
        float exactX = lanePositions[randomLaneIndex];
        float randomZ = Random.Range(0, tileLength);

        // Start high in the air and shoot a laser down to find the ground
        Vector3 localStartPos = new Vector3(exactX, 20f, randomZ);
        Vector3 worldStartPos = roadTile.transform.TransformPoint(localStartPos);

        if (Physics.Raycast(worldStartPos, Vector3.down, out RaycastHit hit, 50f))
        {
            // Spawn the item slightly above whatever the laser hit
            Vector3 finalSpawnPosition = hit.point + (Vector3.up * hoverHeight);
            GameObject powerUp = Instantiate(selectedPowerUp, finalSpawnPosition, Quaternion.identity);
            
            // Parent it to the road so it gets deleted when the road gets deleted
            powerUp.transform.SetParent(roadTile.transform, true); 
        }
    }

    // Creates the sidewalks and triggers building generation
    void SpawnSide(GameObject roadTile, float xPos)
    {
        // 1. Spawn the concrete floor
        if (concretePrefab != null)
        {
            GameObject ground = Instantiate(concretePrefab, roadTile.transform);
            float side = Mathf.Sign(xPos); 
            float roadEdge = roadOffset / 2f;
            float concreteX = (roadEdge + (concreteWidth / 2f)) * side;

            ground.transform.localPosition = new Vector3(concreteX, -0.05f, tileLength / 2f);
            ground.transform.localScale = new Vector3(concreteWidth, 0.1f, tileLength);
        }

        float baseRotation = (xPos > 0) ? -90f : 90f;
        
        // 2. Spawn foreground buildings
        SpawnLane(roadTile, xPos, baseRotation, availableBuildings);

        // 3. Spawn background buildings further out
        float bgXPos = (xPos > 0) ? backgroundOffset : -backgroundOffset;
        SpawnLane(roadTile, bgXPos, baseRotation, backgroundBuildings);
    }
    
    // Packs random buildings closely together until the tile is completely full
    void SpawnLane(GameObject roadTile, float xPos, float baseRotation, BuildingType[] buildingPool)
    {
        if (buildingPool == null || buildingPool.Length == 0) return;

        float currentZ = 0f;
        while (currentZ < tileLength)
        {
            List<BuildingType> validBuildings = new List<BuildingType>();
            
            // Check which buildings are small enough to fit in the remaining space
            foreach (var b in buildingPool) 
            {
                if (currentZ + b.width <= tileLength) validBuildings.Add(b);
            }

            if (validBuildings.Count == 0) break; // No space left!

            // Pick a random valid building
            BuildingType selected = validBuildings[Random.Range(0, validBuildings.Count)];
            
            if (selected.width <= 0.1f) break; // Failsafe to prevent infinite loops/crashes

            float zPosOnTile = currentZ + (selected.width / 2f);

            GameObject building = Instantiate(selected.prefab, roadTile.transform);
            building.transform.localPosition = new Vector3(xPos, 0, zPosOnTile);
            building.transform.localRotation = Quaternion.Euler(0, baseRotation + selected.rotationOffset, 0);

            // Move our starting point forward by the width of the building we just placed
            currentZ += selected.width;
        }
    }
    
    // Removes the oldest tile behind the player
    void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}