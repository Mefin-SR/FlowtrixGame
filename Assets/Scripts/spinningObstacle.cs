using UnityEngine;

public class spinningObstacle : MonoBehaviour
{    
    public float rotationSpeed = 45f;

    private float rotation = 0;

    void Update()
    {
        rotation += Time.deltaTime * rotationSpeed;
        transform.localRotation = Quaternion.Euler(0, rotation, 0);
    }
}
