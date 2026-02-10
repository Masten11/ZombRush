using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] roadPrefabs;
    public Transform playerTransform;

    public float tileLength = 30f; // Hur lång varje väg-bit är (Måste stämma med Prefab!)
    public int numberOfTiles = 5;  // Hur många väg-bitar som ska finnas samtidigt

    private float spawnZ = 0f; // Vart nästa bit ska placeras (Z-led)
    private float safeZone = 45f; // Hur långt bakom spelaren vi tar bort vägen

    // Lista som håller koll på de aktiva vägarna i spelet
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // Skapa de första vägarna så spelaren har mark att stå på direkt
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i == 0)
                SpawnTile(0); // Första biten är alltid index 0 
            else
                SpawnTile(Random.Range(0, roadPrefabs.Length));
        }
    }

    void Update()
    {
        // 1. Calculate the threshold
    float threshold = spawnZ - (numberOfTiles * tileLength);
    
    // 2. Calculate player position with buffer
    float playerPos = playerTransform.position.z - safeZone;

    // 3. Print values to Console (Only do this while debugging!)
    // If PlayerPos is SMALLER than Threshold, nothing happens.
    Debug.Log($"Player: {playerPos}  vs  Threshold: {threshold}");

    if (playerPos > threshold)
    {
        Debug.Log("SPAWNING NEW TILE!"); // This should appear if it works
        SpawnTile(Random.Range(0, roadPrefabs.Length));
        DeleteTile();
    }
    }

    // Funktion för att skapa en väg-bit
    void SpawnTile(int tileIndex)
    {
        // Instansiera (skapa) kopia av Prefaben
        GameObject go = Instantiate(roadPrefabs[tileIndex], transform.forward * spawnZ, transform.rotation);

        // Lägg till i listan så vi kan ta bort den sen
        activeTiles.Add(go);

        // Flytta fram spawn-punkten inför nästa bit
        spawnZ += tileLength;
    }
    
    // Funktion för att ta bort den gamla vägen bakom spelaren
    void DeleteTile()
    {
        // Ta bort objektet från spelet
        Destroy(activeTiles[0]);
        
        // Ta bort referensen från listan
        activeTiles.RemoveAt(0);
    }
}
