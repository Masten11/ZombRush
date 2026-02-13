using System.Collections.Generic;
using UnityEngine;

// Klass för att ställa in storlek på byggnader
[System.Serializable]
public class BuildingType
{
    public string name;         // Namn (t.ex. "Litet Hus")
    public GameObject prefab;   // Modellen
    public float width = 10f;   // Hur mycket plats den tar i Z-led
}

public class LevelManager : MonoBehaviour
{
    [Header("Road Configuration")]
    public GameObject[] roadPrefabs;
    public Transform playerTransform;

    public float tileLength = 30f; 
    public int numberOfTiles = 5;  

    [Header("Building Configuration")]
    // Lista med byggnader där du kan ställa in bredd
    public BuildingType[] availableBuildings; 
    
    public float buildingOffset = 25f;   // Avstånd från mitten

    private float spawnZ = 0f; 
    private float safeZone = 45f; 

    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i < 2) 
            {
                // FALSE här betyder: Inga byggnader på de första 2 bitarna
                SpawnTile(0, false); 
            }
            else
            {
                // TRUE betyder: Nu börjar vi spawna byggnader
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

        // Vi kollar om spawnItems är sant OCH om vi har några byggnader inlagda
        if (spawnItems && availableBuildings != null && availableBuildings.Length > 0)
        {
            SpawnLane(go, -buildingOffset, 90f);  // Vänster sida
            SpawnLane(go, buildingOffset, -90f);  // Höger sida
        }

        activeTiles.Add(go);
        spawnZ += tileLength;
    }
    
    // Funktion som fyller en sida av vägen baserat på byggnadernas storlek
    void SpawnLane(GameObject roadTile, float xPos, float yRotation)
    {
        float currentZ = 0f;

        // Så länge det finns plats kvar på vägbiten...
        while (currentZ < tileLength)
        {
            // Hitta alla byggnader som får plats i utrymmet som är kvar
            List<BuildingType> validBuildings = new List<BuildingType>();
            foreach (var b in availableBuildings)
            {
                if (currentZ + b.width <= tileLength)
                {
                    validBuildings.Add(b);
                }
            }

            // Om inget passar, sluta spawna på denna rad
            if (validBuildings.Count == 0) break;

            // Välj en slumpmässig byggnad som passar
            BuildingType selected = validBuildings[Random.Range(0, validBuildings.Count)];

            // Räkna ut mitten-positionen för byggnaden
            float zPosOnTile = currentZ + (selected.width / 2f);

            // Skapa byggnaden
            GameObject building = Instantiate(selected.prefab, roadTile.transform);
            building.transform.localPosition = new Vector3(xPos, 0, zPosOnTile);
            building.transform.localRotation = Quaternion.Euler(0, yRotation, 0);

            // Flytta fram "markören" med byggnadens bredd
            currentZ += selected.width;
        }
    }
    
    void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}