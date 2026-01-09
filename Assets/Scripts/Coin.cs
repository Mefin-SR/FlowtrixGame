using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private int coinValue = 10;

    private Vector3 startPosition;
    private CoinManager coinManager;

    void Start()
    {
        startPosition = transform.localPosition;
        coinManager = FindFirstObjectByType<CoinManager>();
    }

    void Update()
    {
        // Rotate the coin
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 newPos = startPosition;
        newPos.y = newY;
        transform.localPosition = newPos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        // Add coin collection logic here (score, sound, particle effects)
        // GameManager.Instance?.AddScore(coinValue);

        // Return coin to pool
        coinManager.ReturnCoin(gameObject);
    }
}
