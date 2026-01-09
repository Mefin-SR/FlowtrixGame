using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("Coin Prefabs")]
    [SerializeField] private GameObject coinPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private int poolSize = 50;
    [SerializeField] private float coinSpawnChance = 0.4f; // 40% chance per platform
    [SerializeField] private float coinLineSpawnChance = 0.2f; // 20% chance for coin lines
    [SerializeField] private int maxCoinsPerPlatform = 3;
    [SerializeField] private float minCoinSpacing = 2f;
    [SerializeField] private float coinHeight = 1f;
    
    [Header("Coin Line Settings")]
    [SerializeField] private int coinsPerLine = 5;
    [SerializeField] private float coinLineSpacing = 1.5f;
    
    private Queue<GameObject> coinPool = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int coinValue = 5;
    private int score = 0;
    
    void Awake()
    {
        InitializeCoinPool();
    }
    
    void InitializeCoinPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coin.transform.SetParent(transform);
            coinPool.Enqueue(coin);
        }
    }
    
    public GameObject GetCoin()
    {
        GameObject coin = coinPool.Count > 0 ? coinPool.Dequeue() : Instantiate(coinPrefab);
        activeCoins.Add(coin);
        return coin;
    }

    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        coin.transform.SetParent(transform);
        activeCoins.Remove(coin);
        coinPool.Enqueue(coin);

        score += coinValue;
        scoreText.text = "Score : " + score;
    }
    
    public void ReturnAllCoins(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child.CompareTag("Coin"))
            {
                ReturnCoin(child.gameObject);
            }
        }
    }
    
    public void SpawnCoinsOnPlatform(PlatformManager.Platform platform, List<Vector3> occupiedPositions)
    {
        if (Random.value > coinSpawnChance) return;
        
        bool spawnCoinLine = Random.value < coinLineSpawnChance;
        
        if (spawnCoinLine)
        {
            SpawnCoinLine(platform, occupiedPositions);
        }
        else
        {
            SpawnRandomCoins(platform, occupiedPositions);
        }
    }
    
    void SpawnCoinLine(PlatformManager.Platform platform, List<Vector3> occupiedPositions)
    {
        float platformLength = Vector3.Distance(platform.startPosition, platform.endPoint.position);
        float[] lanes = { -2f, 0f, 2f };
        int selectedLane = Random.Range(0, lanes.Length);
        
        // Find a suitable Z position for the coin line
        float startZ = Random.Range(2f, platformLength - (coinsPerLine * coinLineSpacing) - 1f);
        
        // Check if this area is free from obstacles
        bool areaFree = true;
        for (int i = 0; i < coinsPerLine; i++)
        {
            Vector3 testPos = new Vector3(lanes[selectedLane], coinHeight, startZ + (i * coinLineSpacing));
            if (IsPositionTooClose(testPos, occupiedPositions, minCoinSpacing))
            {
                areaFree = false;
                break;
            }
        }
        
        if (!areaFree) return;
        
        // Spawn the coin line
        for (int i = 0; i < coinsPerLine; i++)
        {
            Vector3 localPos = new Vector3(lanes[selectedLane], coinHeight, startZ + (i * coinLineSpacing));
            SpawnCoinAtPosition(platform, localPos);
            occupiedPositions.Add(localPos);
        }
    }
    
    void SpawnRandomCoins(PlatformManager.Platform platform, List<Vector3> occupiedPositions)
    {
        float platformLength = Vector3.Distance(platform.startPosition, platform.endPoint.position);
        float[] lanes = { -2f, 0f, 2f };
        
        int coinsToSpawn = Random.Range(1, maxCoinsPerPlatform + 1);
        int attempts = 10;
        
        while (coinsToSpawn > 0 && attempts > 0)
        {
            float z = Random.Range(1f, platformLength - 1f);
            float x = lanes[Random.Range(0, lanes.Length)];
            Vector3 localPos = new Vector3(x, coinHeight, z);
            
            if (!IsPositionTooClose(localPos, occupiedPositions, minCoinSpacing))
            {
                SpawnCoinAtPosition(platform, localPos);
                occupiedPositions.Add(localPos);
                coinsToSpawn--;
            }
            
            attempts--;
        }
    }
    
    void SpawnCoinAtPosition(PlatformManager.Platform platform, Vector3 localPosition)
    {
        GameObject coin = GetCoin();
        coin.transform.SetParent(platform.obstacleContainer, false);
        coin.transform.localPosition = localPosition;
        coin.SetActive(true);
    }
    
    bool IsPositionTooClose(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (Vector3 existing in existingPositions)
        {
            if (Vector3.Distance(position, existing) < minDistance)
                return true;
        }
        return false;
    }
}
