using System.Collections;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] Animator animator;
    private CharacterController characterController;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private float slideColliderHeight = 0.6f;
    private bool isSliding = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        originalColliderHeight = characterController.height;
        originalColliderCenter = characterController.center;
    }

    public void Jump()
    {
        animator.SetTrigger("Jump");
    }

    public void Slide()
    {
        StartCoroutine(SlideCollider());
        animator.SetTrigger("Slide");
        // animator.SetTrigger("BackSlide");
    }

    IEnumerator SlideCollider()
    {
        characterController.height = slideColliderHeight;
        characterController.center = new Vector3(originalColliderCenter.x, slideColliderHeight / 2f, originalColliderCenter.z);
        isSliding = true;

        yield return new WaitForSeconds(1.5f);

        characterController.height = originalColliderHeight;
        characterController.center = originalColliderCenter;
        isSliding = false;
    }

    public bool IsSliding => isSliding;
}
