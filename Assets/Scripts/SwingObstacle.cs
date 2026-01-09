using UnityEngine;

public class PendulumRotation : MonoBehaviour
{
    public float swingAngle = 45f;     // Max rotation in degrees
    public float swingSpeed = 2f;      // How fast it swings

    private Quaternion startRotation;
    private float time;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        time += Time.deltaTime * swingSpeed;
        float angle = Mathf.Sin(time) * swingAngle;
        transform.rotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}
