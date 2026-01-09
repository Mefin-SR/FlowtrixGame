using UnityEngine;

public class TurnTrigger : MonoBehaviour
{
    public Transform targetRotationPoint;

    private void OnTriggerEnter(Collider other)

    {
        if (other.CompareTag("Player"))
        {
            PlayerTurnController turner = other.GetComponent<PlayerTurnController>();
            PlayerController controller = other.GetComponent<PlayerController>();
            if (turner != null && targetRotationPoint != null)
            {
                turner.TriggerTurn(targetRotationPoint.rotation);
                controller.ResetLaneAfterTurn();
            }
        }
    }
}
