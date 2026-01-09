using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [System.Serializable]
    public class Platform
    {
        public GameObject gameObject;
        public Transform endPoint;
        public Vector3 startPosition;
        public Vector3 forwardDirection;
        public int index;
        public Transform obstacleContainer;
    }

    [Header("Platform Prefabs")]
    [SerializeField] private GameObject straightPlatformPrefab;
    [SerializeField] private GameObject leftTurnPlatformPrefab;
    [SerializeField] private GameObject rightTurnPlatformPrefab;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private int poolSizePerType = 5;
    [SerializeField] private int platformsAhead = 5;
    [SerializeField] private float passThreshold = 10f;

    [Header("Obstacle")]
    [SerializeField] private ObstacleManager obstacleManager;
    [SerializeField] private float obstacleSpawnDelay = 3f;
    [SerializeField] private float minObstacleSpacing = 3f;
    [SerializeField] private int maxObstaclesAtStart = 1;
    [SerializeField] private int maxObstaclesMaxDifficulty = 5;
    [SerializeField] private float difficultyRampTime = 20f;

    [Header("Coins")]
    [SerializeField] private CoinManager coinManager;

    private float obstacleTimer = 0f;
    private int currentMaxObstacles = 1;
    private bool hasStartedSpawningObstacles = false;

    private Dictionary<GameObject, Queue<GameObject>> platformPools = new Dictionary<GameObject, Queue<GameObject>>();
    private List<Platform> activePlatforms = new List<Platform>();

    private Vector3 currentPosition = Vector3.zero;
    private Quaternion currentRotation = Quaternion.identity;

    private int playerPlatformIndex = 0;

    private enum TurnType { Straight, Left, Right }
    private TurnType? lastTurn = null;
    private int consecutiveTurnCount = 0;
    private int maxConsecutiveSameTurns = 1;

    void Awake()
    {
        // platformPools[straightPlatformPrefab] = CreatePool(straightPlatformPrefab);
        platformPools[leftTurnPlatformPrefab] = CreatePool(leftTurnPlatformPrefab);
        platformPools[rightTurnPlatformPrefab] = CreatePool(rightTurnPlatformPrefab);
    }

    void Start()
    {
        for (int i = 0; i < platformsAhead; i++)
        {
            GameObject prefab = (i < 2) ? rightTurnPlatformPrefab : GetWeightedRandomPrefab();
            Platform platform = GetNextPlatform(prefab);
            platform.index = i;
            SpawnPlatform(platform);
            activePlatforms.Add(platform);
        }
    }

    void Update()
    {
        if (playerPlatformIndex >= activePlatforms.Count)
            return;

        Platform current = activePlatforms[playerPlatformIndex];
        Vector3 toPlayer = player.position - current.startPosition;
        float forwardDistance = Vector3.Dot(toPlayer, current.forwardDirection);

        if (forwardDistance > passThreshold)
        {
            playerPlatformIndex++;

            // Recycle platform behind
            int recycleIndex = playerPlatformIndex - 2;
            if (recycleIndex >= 0 && recycleIndex < activePlatforms.Count)
            {
                RecyclePlatformAtIndex(recycleIndex);
            }

            // Spawn new platform
            Platform newPlatform = GetNextPlatform();
            if (newPlatform != null)
            {
                newPlatform.index = activePlatforms.Count;
                SpawnPlatform(newPlatform);
                activePlatforms.Add(newPlatform);

                // Spawn obstacles and coins on the new platform
                SpawnContentOnPlatform(newPlatform);
            }
        }

        if (!hasStartedSpawningObstacles)
        {
            obstacleTimer += Time.deltaTime;
            if (obstacleTimer >= obstacleSpawnDelay)
            {
                hasStartedSpawningObstacles = true;
                obstacleTimer = 0f;
            }
        }
        else
        {
            obstacleTimer += Time.deltaTime;
            if (obstacleTimer >= difficultyRampTime)
            {
                currentMaxObstacles = Mathf.Min(currentMaxObstacles + 1, maxObstaclesMaxDifficulty);
                obstacleTimer = 0f;
            }
        }

        if (hasStartedSpawningObstacles)
        {
            SpawnObstaclesOnActivePlatforms();
        }
    }

    void RecyclePlatformAtIndex(int index)
    {
        if (index < 0 || index >= activePlatforms.Count)
            return;

        Platform platform = activePlatforms[index];
        platform.gameObject.SetActive(false);
        var prefab = GetPrefabFromInstance(platform.gameObject);
        if (platformPools.TryGetValue(prefab, out var pool))
        {
            pool.Enqueue(platform.gameObject);
        }
        activePlatforms[index] = null;

        if (platform.obstacleContainer != null)
        {
            obstacleManager.ReturnAllObstacles(platform.obstacleContainer);
            coinManager.ReturnAllCoins(platform.obstacleContainer);
        }
    }

    Platform GetNextPlatform(GameObject forcedPrefab = null)
    {
        GameObject prefab = forcedPrefab ?? GetWeightedRandomPrefab();
        TurnType currentTurn = GetTurnType(prefab);

        if (lastTurn.HasValue && currentTurn == lastTurn && currentTurn != TurnType.Straight)
        {
            if (consecutiveTurnCount >= maxConsecutiveSameTurns)
            {
                prefab = GetDifferentTurnPrefab(currentTurn);
                currentTurn = GetTurnType(prefab);
            }
        }

        Queue<GameObject> pool = platformPools[prefab];
        GameObject obj = (pool.Count > 0) ? pool.Dequeue() : Instantiate(prefab);
        obj.SetActive(false);

        Transform end = obj.transform.Find("EndPoint");
        if (!end)
        {
            Debug.LogError($"Prefab '{prefab.name}' is missing an EndPoint child.");
            return null;
        }

        if (currentTurn == lastTurn)
            consecutiveTurnCount++;
        else
            consecutiveTurnCount = 1;

        lastTurn = currentTurn;

        return new Platform
        {
            gameObject = obj,
            endPoint = end
        };
    }

    void SpawnPlatform(Platform platform)
    {
        platform.gameObject.transform.SetPositionAndRotation(currentPosition, currentRotation);
        platform.gameObject.SetActive(true);

        platform.startPosition = platform.gameObject.transform.position;
        platform.forwardDirection = platform.gameObject.transform.forward;

        currentPosition = platform.endPoint.position;
        currentRotation = platform.endPoint.rotation;

        var container = platform.gameObject.transform.Find("Obstacles");
        platform.obstacleContainer = container;
    }

    void SpawnContentOnPlatform(Platform platform)
    {
        if (!hasStartedSpawningObstacles) return;
        
        List<Vector3> occupiedPositions = new List<Vector3>();
        
        // Spawn obstacles first
        SpawnObstacleOnPlatform(platform, occupiedPositions);
        
        // Then spawn coins, avoiding obstacle positions
        if (coinManager != null)
        {
            coinManager.SpawnCoinsOnPlatform(platform, occupiedPositions);
        }
    }

    void SpawnObstaclesOnActivePlatforms()
    {
        for (int i = playerPlatformIndex; i < playerPlatformIndex + 3 && i < activePlatforms.Count; i++)
        {
            Platform platform = activePlatforms[i];
            if (platform == null) continue;

            // Check if platform already has any content (obstacles or coins)
            bool hasContent = platform.obstacleContainer.childCount > 0;

            if (!hasContent)
            {
                Debug.Log($"Spawning content on platform {platform.index}");
                SpawnContentOnPlatform(platform);
            }
            else
            {
                // Platform has content, but check if we need more obstacles based on difficulty
                int obstacleCount = 0;
                foreach (Transform child in platform.obstacleContainer)
                {
                    if (child.CompareTag("Obstacle"))
                        obstacleCount++;
                }

                if (obstacleCount < currentMaxObstacles)
                {
                    List<Vector3> occupiedPositions = GetOccupiedPositions(platform);
                    SpawnObstacleOnPlatform(platform, occupiedPositions);
                }
            }
        }
    }

    void SpawnObstacleOnPlatform(Platform platform, List<Vector3> occupiedPositions = null)
    {
        if (occupiedPositions == null) occupiedPositions = new List<Vector3>();

        TurnType platformType = GetTurnType(GetPrefabFromInstance(platform.gameObject));

        // Get existing obstacle positions
        foreach (Transform child in platform.obstacleContainer)
        {
            if (child.CompareTag("Obstacle"))
            {
                Vector3 localPos = platform.obstacleContainer.InverseTransformPoint(child.position);
                occupiedPositions.Add(localPos);
            }
        }

        float platformLength = Vector3.Distance(platform.startPosition, platform.endPoint.position);
        float[] lanes;
        float minZ, maxZ;

        // Adjust spawn parameters based on platform type
        switch (platformType)
        {
            case TurnType.Left:
            case TurnType.Right:
                lanes = new float[] { -1f, 0f, 1f }; // Narrower lanes for curves
                minZ = platformLength * 0.3f; // Start spawning later
                maxZ = platformLength * 0.7f; // End spawning earlier
                break;
            default: // Straight
                lanes = new float[] { -2f, 0f, 2f };
                minZ = 1f;
                maxZ = platformLength - 1f;
                break;
        }

        int attempts = 10;
        while (attempts-- > 0)
        {
            float z = Random.Range(minZ, maxZ);
            float x = lanes[Random.Range(0, lanes.Length)];
            Vector3 localPos = new Vector3(x, 0f, z);

            // Check spacing with existing obstacles and coins
            bool tooClose = false;
            foreach (Vector3 existing in occupiedPositions)
            {
                if (Vector3.Distance(localPos, existing) < minObstacleSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                GameObject prefab = obstacleManager.GetRandomObstaclePrefab();
                GameObject obstacle = obstacleManager.GetObstacle(prefab);
                Quaternion facePlayer = Quaternion.LookRotation(-platform.forwardDirection);

                obstacle.transform.SetParent(platform.obstacleContainer, false);
                obstacle.transform.localPosition = localPos;
                obstacle.transform.rotation = facePlayer;
                obstacle.SetActive(true);

                occupiedPositions.Add(localPos);
                break;
            }
        }
    }
    
    GameObject GetWeightedRandomPrefab()
    {
        int rand = Random.Range(0, 10);
        if (rand < 6) return leftTurnPlatformPrefab;
        else return rightTurnPlatformPrefab;
    }

    Queue<GameObject> CreatePool(GameObject prefab)
    {
        Queue<GameObject> pool = new Queue<GameObject>();
        for (int i = 0; i < poolSizePerType; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        return pool;
    }

    GameObject GetPrefabFromInstance(GameObject instance)
    {
        string baseName = instance.name.Replace("(Clone)", "").Trim();
        // if (baseName == straightPlatformPrefab.name) return straightPlatformPrefab;
        if (baseName == leftTurnPlatformPrefab.name) return leftTurnPlatformPrefab;
        if (baseName == rightTurnPlatformPrefab.name) return rightTurnPlatformPrefab;
        Debug.LogWarning("Unknown prefab name: " + instance.name);
        return straightPlatformPrefab;
    }

    TurnType GetTurnType(GameObject prefab)
    {
        // if (prefab == straightPlatformPrefab) return TurnType.Straight;
        if (prefab == leftTurnPlatformPrefab) return TurnType.Left;
        if (prefab == rightTurnPlatformPrefab) return TurnType.Right;
        return TurnType.Straight;
    }

    GameObject GetDifferentTurnPrefab(TurnType turnToAvoid)
    {
        List<GameObject> candidates = new List<GameObject>();

        // if (turnToAvoid != TurnType.Straight)
            // candidates.Add(straightPlatformPrefab);

        if (turnToAvoid != TurnType.Left)
            candidates.Add(leftTurnPlatformPrefab);

        if (turnToAvoid != TurnType.Right)
            candidates.Add(rightTurnPlatformPrefab);

        if (candidates.Count == 0) return rightTurnPlatformPrefab;

        return candidates[Random.Range(0, candidates.Count)];
    }
    
    List<Vector3> GetOccupiedPositions(Platform platform)
    {
        List<Vector3> occupiedPositions = new List<Vector3>();
        
        foreach (Transform child in platform.obstacleContainer)
        {
            Vector3 localPos = platform.obstacleContainer.InverseTransformPoint(child.position);
            occupiedPositions.Add(localPos);
        }
        
        return occupiedPositions;
    }
}
