using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float forwardSpeed = 8f;
    [SerializeField] float maxForwardSpeed = 15f;
    [SerializeField] float speedIncreaseRate = 0.1f;

    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0.07f);
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float difficultyRampTime = 30f;
    [SerializeField] float maxDifficultyMultiplier = 2f;

    [SerializeField] float laneDistance = 2;
    [SerializeField] float laneChangeSpeed = 20;

    [SerializeField] float jumpHeight = 1f;

    [SerializeField] Animator animator;
    private CharacterController characterController;
    // private EnvironmentScanner environmentScanner;
    private ParkourController parkourController;

    private bool isGrounded;
    private float currentForwardSpeed;
    private float yVelocity;
    private float difficultyTimer;
    private float currentDifficultyMultiplier = 1f;

    private int targetLane = 0;
    private float currentLaneOffset = 0f;
    private float previousLaneOffset = 0f;

    private bool isGameActive = true;
    [SerializeField] GameObject RestartScreen;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        parkourController = GetComponent<ParkourController>();

        currentForwardSpeed = forwardSpeed;

        // QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;    
    }
    void Update()
    {
        if (!isGameActive) return;

        bool leftInput = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightInput = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        bool upInput = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool downInput = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);

        // Ground Check
        Vector3 checkPosition = transform.TransformPoint(groundCheckOffset);
        isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayer);

        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -0.5f;
        }
        else if (!isGrounded)
        {
            yVelocity += Physics.gravity.y * Time.deltaTime;
        }

        if (upInput && isGrounded && !parkourController.IsSliding)
        {
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            parkourController.Jump();
        }
        if (downInput && isGrounded && !parkourController.IsSliding)
        {
            parkourController.Slide();
        }

        if (leftInput && targetLane > -1)
        {
            targetLane--;
        }
        else if (rightInput && targetLane < 1)
        {
            targetLane++;
        }

        float targetOffset = targetLane * laneDistance;
        currentLaneOffset = Mathf.Lerp(currentLaneOffset, targetOffset, laneChangeSpeed * Time.deltaTime);
        float deltaOffset = currentLaneOffset - previousLaneOffset;
        previousLaneOffset = currentLaneOffset;

        Vector3 movement = transform.forward * currentForwardSpeed * currentDifficultyMultiplier * Time.deltaTime;
        movement += transform.right * deltaOffset;
        movement.y = yVelocity * Time.deltaTime;

        characterController.Move(movement);

        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyRampTime)
        {
            if (forwardSpeed < maxForwardSpeed)
            {
                forwardSpeed += speedIncreaseRate;
            }
            if (animator.speed < maxDifficultyMultiplier)
            {
                animator.speed += 0.05f;
            }
            difficultyTimer = 0;
        }
    }

    public void ResetLaneAfterTurn()
    {
        targetLane = 0;
        currentLaneOffset = 0f;
        previousLaneOffset = 0f;

        Vector3 localPos = transform.InverseTransformPoint(transform.position);
        localPos.x = 0;
        Vector3 worldPos = transform.TransformPoint(localPos);
        characterController.enabled = false;
        transform.position = worldPos;
        characterController.enabled = true;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            isGameActive = false;
            RestartScreen.SetActive(true);
        }
    }
}
