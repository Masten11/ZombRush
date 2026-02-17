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

    [Header("Building Configuration")]
    public BuildingType[] availableBuildings; 
    public float buildingOffset = 25f; 

    [Header("Ground/Sidewalk Configuration")]
    public GameObject concretePrefab; // Dra in din betong-prefab här
    public float concreteWidth = 40f; // Hur bred betongen ska vara utåt
    public float roadOffset = 10f;     // Hur bred hela din väg är (totalt)

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
            // Spawna betongmark och hus på båda sidor
            SpawnSide(go, -buildingOffset); // Vänster
            SpawnSide(go, buildingOffset);  // Höger
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    // Kombinerad funktion för att spawna både mark och hus
    void SpawnSide(GameObject roadTile, float xPos)
    {
    if (concretePrefab != null)
    {
        GameObject ground = Instantiate(concretePrefab, roadTile.transform);
        
        // 1. Ta reda på vilken sida vi är på (1 för höger, -1 för vänster)
        float side = Mathf.Sign(xPos); 

        // 2. Räkna ut var kanten på vägen är
        float roadEdge = roadOffset / 2f;

        // 3. Räkna ut mittenpunkten för betongplattan
        // Den ska ligga vid kanten + halva sin egen bredd
        float concreteX = (roadEdge + (concreteWidth / 2f)) * side;

        // 4. Applicera position och storlek
        ground.transform.localPosition = new Vector3(concreteX, -0.05f, tileLength / 2f);
        ground.transform.localScale = new Vector3(concreteWidth, 0.1f, tileLength);
    }

    // Spawna husen som vanligt
    float baseRotation = (xPos > 0) ? -90f : 90f;
    SpawnLane(roadTile, xPos, baseRotation);
    }
    void SpawnLane(GameObject roadTile, float xPos, float baseRotation)
    {
        float currentZ = 0f;
        while (currentZ < tileLength)
        {
            List<BuildingType> validBuildings = new List<BuildingType>();
            foreach (var b in availableBuildings)
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