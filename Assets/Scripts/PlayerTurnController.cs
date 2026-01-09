using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 5f;
    private Quaternion targetRotation;
    private bool isTurning = false;

    public void TriggerTurn(Quaternion newRotation)
    {
        targetRotation = newRotation;
        isTurning = true;
    }

    void Update()
    {
        if (!isTurning) return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime * 100f);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
        {
            transform.rotation = targetRotation;
            isTurning = false;
        }
    }
}
