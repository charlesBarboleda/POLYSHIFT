using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    private PlayerInput moveInput;
    private InputAction moveAction;

    [Header("Player Movement")]
    public float MoveSpeed = 5f;
    public bool IsIsometric = false;
    public bool canMove = true;
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
        if (canMove)
        {
            HandleMovementAndAnimations(inputDirection);
        }
    }

    void HandleMovementAndAnimations(Vector2 inputDirection)
    {
        Vector3 moveDirection;

        if (!IsIsometric)
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

            transform.position += moveDirection * Time.deltaTime * MoveSpeed;
        }
        else
        {
            animator.SetFloat("HorizontalDirection", 0);
            animator.SetFloat("VerticalDirection", 0);
        }
    }

    public void MoveSpeedIncreaseBy(float amount)
    {
        MoveSpeed += amount;
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

}
