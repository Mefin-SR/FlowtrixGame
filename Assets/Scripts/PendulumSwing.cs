using UnityEngine;

public class PendulumSwing : MonoBehaviour
{
    public float swingAngle = 45f;     // Max swing angle in degrees
    public float swingSpeed = 2f;      // Swinging speed

    private float time;

    void Update()
    {
        time += Time.deltaTime * swingSpeed;
        float angle = Mathf.Sin(time) * swingAngle;
        transform.localRotation = Quaternion.Euler(0, 0, angle);  // Swinging on Z-axis
    }
}
