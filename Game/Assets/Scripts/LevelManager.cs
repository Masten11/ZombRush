using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingType
{
    public string name;         
    public GameObject prefab;   
    public float width = 10f;
    public float rotationOffset = 0f; 
}

public class LevelManager : MonoBehaviour
{
    [Header("Road Configuration")]
    public GameObject[] roadPrefabs;
    public Transform playerTransform;
    public float tileLength = 30f; 
    public int numberOfTiles = 5;  

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
    // --- UPDATED: Now it's a list! You can add Double Jump AND Smash here! ---
    public GameObject[] powerUpPrefabs; 
    [Range(0f, 1f)] public float powerUpSpawnChance = 0.3f; 
    public float hoverHeight = 1.0f; 
    public float[] lanePositions = { 0f, 10f, 20f }; 
    // -------------------------------------------------------------------------

    private float spawnZ = 0f; 
    private float safeZone = 45f; 
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i < 2) SpawnTile(0, false); 
            else SpawnTile(Random.Range(1, roadPrefabs.Length), true);
        }
    }

    void Update()
    {
        float threshold = spawnZ - (numberOfTiles * tileLength);
        float playerPos = playerTransform.position.z - safeZone;

        if (playerPos > threshold)
        {
            SpawnTile(Random.Range(1, roadPrefabs.Length), true);
            DeleteTile();
        }
    }

    void SpawnTile(int tileIndex, bool spawnItems)
    {
        GameObject go = Instantiate(roadPrefabs[tileIndex], transform.forward * spawnZ, transform.rotation);

        if (spawnItems)
        {
            SpawnSide(go, -buildingOffset); 
            SpawnSide(go, buildingOffset);  
            
            SpawnPowerUp(go);
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    void SpawnPowerUp(GameObject roadTile)
    {
        // --- UPDATED: Check if the list has any powerups in it ---
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        
        // Roll the dice to see if a powerup spawns on this tile
        if (Random.value > powerUpSpawnChance) return; 
        
        if (lanePositions == null || lanePositions.Length == 0) return;

        // --- NEW: Pick a random power-up from your list! ---
        GameObject selectedPowerUp = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        int randomLaneIndex = Random.Range(0, lanePositions.Length);
        float exactX = lanePositions[randomLaneIndex];
        float randomZ = Random.Range(0, tileLength);

        Vector3 localStartPos = new Vector3(exactX, 20f, randomZ);
        Vector3 worldStartPos = roadTile.transform.TransformPoint(localStartPos);

        if (Physics.Raycast(worldStartPos, Vector3.down, out RaycastHit hit, 50f))
        {
            Vector3 finalSpawnPosition = hit.point + (Vector3.up * hoverHeight);
            
            // Spawn the randomly selected power-up
            GameObject powerUp = Instantiate(selectedPowerUp, finalSpawnPosition, Quaternion.identity);
            powerUp.transform.SetParent(roadTile.transform, true); 
        }
    }

    void SpawnSide(GameObject roadTile, float xPos)
    {
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
        SpawnLane(roadTile, xPos, baseRotation, availableBuildings);

        float bgXPos = (xPos > 0) ? backgroundOffset : -backgroundOffset;
        SpawnLane(roadTile, bgXPos, baseRotation, backgroundBuildings);
    }
    
    void SpawnLane(GameObject roadTile, float xPos, float baseRotation, BuildingType[] buildingPool)
    {
        if (buildingPool == null || buildingPool.Length == 0) return;

        float currentZ = 0f;
        while (currentZ < tileLength)
        {
            List<BuildingType> validBuildings = new List<BuildingType>();
            
            foreach (var b in buildingPool) 
            {
                if (currentZ + b.width <= tileLength) validBuildings.Add(b);
            }

            if (validBuildings.Count == 0) break;

            BuildingType selected = validBuildings[Random.Range(0, validBuildings.Count)];
            
            if (selected.width <= 0.1f) break; 

            float zPosOnTile = currentZ + (selected.width / 2f);

            GameObject building = Instantiate(selected.prefab, roadTile.transform);
            building.transform.localPosition = new Vector3(xPos, 0, zPosOnTile);
            building.transform.localRotation = Quaternion.Euler(0, baseRotation + selected.rotationOffset, 0);

            currentZ += selected.width;
        }
    }
    
    void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}