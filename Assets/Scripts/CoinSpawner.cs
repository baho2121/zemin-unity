using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject coinPrefab;   // The coin object to spawn
    public int maxCoins = 50;       // Maximum number of coins in the scene
    public float spawnInterval = 3f;// Time between spawns (seconds)
    public float spawnHeight = 0.1f;  // Height above ground
    public Vector3 spawnRotation = new Vector3(0f, 0f, 90f); // User requested Z=90
    public float minSpawnDistance = 1.0f; // Minimum distance between coins

    [Header("Area Settings")]
    public Collider spawnArea;      // The ground collider defining the area

    private float timer;
    private List<GameObject> spawnedCoins = new List<GameObject>();

    void Start()
    {
        // If no spawn area assigned, try to find one on this object
        if (spawnArea == null)
            spawnArea = GetComponent<Collider>();
            
        if (spawnArea == null)
        {
            Debug.LogError("CoinSpawner: No Spawn Area (Collider) assigned!");
            enabled = false;
        }
    }

    void Update()
    {
        // Cleanup nulls (destroyed coins) from list
        spawnedCoins.RemoveAll(item => item == null);

        // Check limits
        if (spawnedCoins.Count >= maxCoins) return;

        // Timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCoin();
            timer = 0f;
        }
    }

    void SpawnCoin()
    {
        if (coinPrefab == null || spawnArea == null) return;

        Bounds bounds = spawnArea.bounds;
        Vector3 spawnPos = Vector3.zero;
        bool validPositionFound = false;

        // Try 10 times to find a valid position without overlap
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randZ = Random.Range(bounds.min.z, bounds.max.z);
            
            // Raycast for ground height
            Vector3 rayStart = new Vector3(randX, bounds.max.y + 2f, randZ);
            RaycastHit hit;
            
            Vector3 potentialPos;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f))
            {
                potentialPos = hit.point;
                potentialPos.y += spawnHeight;
            }
            else
            {
                potentialPos = new Vector3(randX, bounds.center.y + spawnHeight, randZ);
            }

            // Check overlap
            // We use a sphere check to see if any other coin is close
            // LayerMask could be used if coins are on specific layer
            if (!Physics.CheckSphere(potentialPos, minSpawnDistance)) 
            {
                spawnPos = potentialPos;
                validPositionFound = true;
                break; // Found one!
            }
             // However, CheckSphere checks for ANY collider. 
             // If the ground is detected, it always fails. 
             // We need to check ONLY against other coins.
             // Better approach: Check distance against our list of spawned coins.
            
             bool tooClose = false;
             foreach (var coin in spawnedCoins)
             {
                 if (coin != null && Vector3.Distance(potentialPos, coin.transform.position) < minSpawnDistance)
                 {
                     tooClose = true;
                     break;
                 }
             }

             if (!tooClose)
             {
                 spawnPos = potentialPos;
                 validPositionFound = true;
                 break;
             }
        }

        if (validPositionFound)
        {
            Quaternion rotation = Quaternion.Euler(spawnRotation);
            GameObject newCoin = Instantiate(coinPrefab, spawnPos, rotation);
            newCoin.name = "Coin_" + System.DateTime.Now.Ticks;
            newCoin.SetActive(true);
            spawnedCoins.Add(newCoin);
        }
    }
}
