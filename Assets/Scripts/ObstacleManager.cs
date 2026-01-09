using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private int poolSizePerType = 10;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        foreach (var prefab in obstaclePrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < poolSizePerType; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
            }
            pools[prefab] = pool;
        }
    }

    public GameObject GetRandomObstaclePrefab()
    {
        return obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
    }

    public GameObject GetObstacle(GameObject prefab)
    {
        Queue<GameObject> pool = pools[prefab];
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObstacle(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        GameObject prefab = GetPrefabFromInstance(obj);
        if (pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool.Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Returning unknown obstacle to pool: " + obj.name);
            Destroy(obj);
        }
    }

    public void ReturnAllObstacles(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            ReturnObstacle(child);
        }
    }

    private GameObject GetPrefabFromInstance(GameObject instance)
    {
        string cleanName = instance.name.Replace("(Clone)", "").Trim();
        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab.name == cleanName)
                return prefab;
        }

        return obstaclePrefabs[0]; // fallback
    }
}
