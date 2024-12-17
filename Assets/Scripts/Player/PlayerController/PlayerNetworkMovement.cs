using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using NUnit.Framework;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    private PlayerInput moveInput;
    private InputAction moveAction;
    public Vector2 inputDirection;

    [Header("Player Movement")]
    public float GhostMoveSpeed = 6f;
    public float MoveSpeed = 2f;
    public GameObject skillTreeCanvas;
    public VariableWithEvent<bool> IsIsometric = new VariableWithEvent<bool>(false);
    public bool canMove = true;
    private Animator animator;
    private PlayerStateController state;


    private const float movementThreshold = 0.1f;

    public override void OnNetworkSpawn()
    {
        animator = GetComponentInChildren<Animator>();
        if (!IsOwner) return;

        state = GetComponent<PlayerStateController>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }

    void Update()
    {
        if (!IsOwner) return;
        inputDirection = moveAction.ReadValue<Vector2>();

        if (state.playerState.Value == PlayerState.Dead)
        {
            HandleGhostMovement(inputDirection);
        }


        if (state.playerState.Value == PlayerState.Alive)
        {
            // Determine movement direction and apply animations for both first-person and isometric view
            if (canMove)
            {
                HandleMovementAndAnimations(inputDirection);
            }
        }
    }

    public void ResetMovement()
    {
        MoveSpeed = 3f;
    }
    void HandleGhostMovement(Vector2 inputDirection)
    {
        float verticalInput = Input.GetKey(KeyCode.Space) ? 1f :
                              Input.GetKey(KeyCode.LeftControl) ? -1f : 0f;

        // Camera-based movement
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
        moveDirection.y = verticalInput;

        // Apply movement
        transform.Translate(moveDirection * GhostMoveSpeed * Time.deltaTime, Space.World);
    }


    void HandleMovementAndAnimations(Vector2 inputDirection)
    {
        Vector3 moveDirection;

        if (!IsIsometric.Value)
        {
            moveDirection = GetFirstPersonMoveDirection(inputDirection);

            float movementMagnitude = moveDirection.magnitude;
            bool isMoving = movementMagnitude > movementThreshold;
            animator.SetBool("IsMoving", false);
            animator.SetBool("isMovingFirstPerson", isMoving);

            // If moving, update movement direction parameters
            if (isMoving)
            {
                // Use input direction to determine movement type
                animator.SetFloat("HorizontalDirection", inputDirection.x);
                animator.SetFloat("VerticalDirection", inputDirection.y);

                transform.position += Time.deltaTime * MoveSpeed * moveDirection;
            }
            else
            {
                // Reset direction parameters when idle
                animator.SetFloat("HorizontalDirection", 0);
                animator.SetFloat("VerticalDirection", 0);
            }



            if (GameManager.Instance != null)
            {
                Cursor.visible = skillTreeCanvas.activeSelf;

                if (skillTreeCanvas.activeSelf)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }
                {
                    // Cursor.lockState = CursorLockMode.Locked;
                }
            }
            else
            {

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }

        }
        else
        {
            moveDirection = GetIsometricMoveDirection(inputDirection);

            animator.SetBool("isMovingFirstPerson", false);
            float movementMagnitude = moveDirection.magnitude;
            bool isMoving = movementMagnitude > movementThreshold;

            animator.SetBool("IsMoving", isMoving);
            if (isMoving)
            {
                transform.position += Time.deltaTime * MoveSpeed * moveDirection;
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
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
