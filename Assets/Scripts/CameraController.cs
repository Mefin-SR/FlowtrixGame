using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 3.5f, -6f);
    [SerializeField] float smoothSpeed = 7f;

    [SerializeField] Vector3 lookOffset = new Vector3(0, 0, 2);
    [SerializeField] float lookSmoothSpeed = 5f;


    private Vector3 velocity;

    void Start()
    {
        transform.position = player.position + cameraOffset;
        transform.LookAt(player.position + lookOffset);
    }

    void LateUpdate()
    {
        Vector3 rotatedOffset = player.rotation * cameraOffset;
        Vector3 desiredPosition = player.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);

        Vector3 rotatedLookOffset = player.rotation * lookOffset;
        Vector3 lookAtPosition = player.position + rotatedLookOffset;

        Quaternion lookRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lookSmoothSpeed * Time.deltaTime);
    }
}