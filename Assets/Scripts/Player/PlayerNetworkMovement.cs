using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    private PlayerInput moveInput;
    private InputAction moveAction;

    [Header("Player Movement")]
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsIsometric = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Animator animator;
    private PlayerNetworkRotation playerNetworkRotation;
    private PlayerNetworkHealth playerNetworkHealth;

    private const float movementThreshold = 0.1f;

    public override void OnNetworkSpawn()
    {
        animator = GetComponentInChildren<Animator>();
        if (!IsOwner) return;

        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }

    void Update()
    {
        if (!IsOwner) return;

        if (playerNetworkHealth.currentHealth.Value <= 0) return;

        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        // Determine movement direction and apply animations for both first-person and isometric view

        HandleMovementAndAnimations(inputDirection);
    }

    void HandleMovementAndAnimations(Vector2 inputDirection)
    {
        Vector3 moveDirection;

        if (!IsIsometric.Value)
        {
            moveDirection = GetFirstPersonMoveDirection(inputDirection);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            moveDirection = GetIsometricMoveDirection(inputDirection);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        float movementMagnitude = moveDirection.magnitude;
        bool isMoving = movementMagnitude > movementThreshold;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("HorizontalDirection", inputDirection.x);
            animator.SetFloat("VerticalDirection", inputDirection.y);

            transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
        }
        else
        {
            animator.SetFloat("HorizontalDirection", 0);
            animator.SetFloat("VerticalDirection", 0);
        }
    }


    private Vector3 GetFirstPersonMoveDirection(Vector2 inputDirection)
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        return (forward * inputDirection.y) + (right * inputDirection.x);
    }

    private Vector3 GetIsometricMoveDirection(Vector2 inputDirection)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        return (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
    }

    private float CalculateMovementDirection(Vector2 inputDirection)
    {
        // Set values based on input direction for animation blending
        if (inputDirection.y > 0) return 1f;   // Forward
        if (inputDirection.y < 0) return -1f;  // Backward
        if (inputDirection.x > 0) return 0.5f; // Right
        if (inputDirection.x < 0) return -0.5f;// Left
        return 0f;                             // Idle
    }
}
