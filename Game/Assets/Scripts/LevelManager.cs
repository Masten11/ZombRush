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

    // --- NEW: Dedicated array for your background prefabs ---
    [Header("Background Buildings")]
    public BuildingType[] backgroundBuildings; 
    public float backgroundOffset = 45f; 
    // --------------------------------------------------------

    [Header("Ground/Sidewalk Configuration")]
    public GameObject concretePrefab; 
    public float concreteWidth = 40f; 
    public float roadOffset = 10f;     

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
            SpawnSide(go, -buildingOffset); // Left
            SpawnSide(go, buildingOffset);  // Right
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
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
        
        // --- UPDATED: Pass the correct array to SpawnLane ---
        
        // 1. Spawn foreground buildings
        SpawnLane(roadTile, xPos, baseRotation, availableBuildings);

        // 2. Spawn background buildings further out
        float bgXPos = (xPos > 0) ? backgroundOffset : -backgroundOffset;
        SpawnLane(roadTile, bgXPos, baseRotation, backgroundBuildings);
        
        // ----------------------------------------------------
    }
    
    // --- UPDATED: Added a 'buildingPool' parameter ---
    void SpawnLane(GameObject roadTile, float xPos, float baseRotation, BuildingType[] buildingPool)
    {
        // Safety check: skip if the array is empty so Unity doesn't throw errors
        if (buildingPool == null || buildingPool.Length == 0) return;

        float currentZ = 0f;
        while (currentZ < tileLength)
        {
            List<BuildingType> validBuildings = new List<BuildingType>();
            
            // Look through the specific array we passed in
            foreach (var b in buildingPool) 
            {
                if (currentZ + b.width <= tileLength) validBuildings.Add(b);
            }

            if (validBuildings.Count == 0) break;

            BuildingType selected = validBuildings[Random.Range(0, validBuildings.Count)];
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