using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingType
{
    public string name;         
    public GameObject prefab;   
    public float width = 10f;
    
    // NYTT: Här kan du ställa in extra rotation för specifika hus
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

    private float spawnZ = 0f; 
    private float safeZone = 45f; 

    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i < 2) 
            {
                // Inga byggnader på de första 2 (False)
                SpawnTile(0, false); 
            }
            else
            {
                SpawnTile(Random.Range(1, roadPrefabs.Length), true);
            }
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

        if (spawnItems && availableBuildings != null && availableBuildings.Length > 0)
        {
            SpawnLane(go, -buildingOffset, 90f);  // Vänster sida
            SpawnLane(go, buildingOffset, -90f);  // Höger sida
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
    }
    
    void SpawnLane(GameObject roadTile, float xPos, float baseRotation)
    {
        float currentZ = 0f;

        while (currentZ < tileLength)
        {
            List<BuildingType> validBuildings = new List<BuildingType>();
            foreach (var b in availableBuildings)
            {
                if (currentZ + b.width <= tileLength)
                {
                    validBuildings.Add(b);
                }
            }

            if (validBuildings.Count == 0) break;

            BuildingType selected = validBuildings[Random.Range(0, validBuildings.Count)];
            float zPosOnTile = currentZ + (selected.width / 2f);

            GameObject building = Instantiate(selected.prefab, roadTile.transform);
            building.transform.localPosition = new Vector3(xPos, 0, zPosOnTile);

            // ÄNDRAT: Lägger till husets specifika offset till grundrotationen
            float totalRotation = baseRotation + selected.rotationOffset;
            building.transform.localRotation = Quaternion.Euler(0, totalRotation, 0);

            currentZ += selected.width;
        }
    }
    
    void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}