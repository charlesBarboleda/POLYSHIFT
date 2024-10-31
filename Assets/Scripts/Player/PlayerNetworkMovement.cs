using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    PlayerInput moveInput;
    InputAction moveAction;

    [Header("Player Movement")]
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsIsometric = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    Animator animator;
    PlayerNetworkRotation playerNetworkRotation;
    PlayerNetworkHealth playerNetworkHealth;

    private const float movementThreshold = 0.1f; // Adjust threshold to filter out small movements

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        animator = GetComponentInChildren<Animator>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner of the object should be able to move it

        if (playerNetworkHealth.currentHealth.Value <= 0) return; // If the player is dead, they should not be able to move

        if (!IsIsometric.Value)
        {
            MovePlayerFirstPerson();
        }
        else
        {
            MovePlayerIsometric();
        }
    }

    public void MovePlayerIsometric()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);

        // Determine whether the player is moving based on the threshold
        float movementMagnitude = moveDirection.magnitude;
        animator.SetBool("isMoving", movementMagnitude > movementThreshold);

        // Move the player
        if (movementMagnitude > movementThreshold)
        {
            transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
        }
    }

    public void MovePlayerFirstPerson()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        Vector2 inputDirection = moveAction.ReadValue<Vector2>();
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDirection = (forward * inputDirection.y) + (right * inputDirection.x);


        // Determine whether the player is moving based on the threshold
        float movementMagnitude = moveDirection.magnitude;
        animator.SetBool("isMoving", movementMagnitude > movementThreshold);
        Debug.Log(movementMagnitude > movementThreshold);


        if (movementMagnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Move the player
        if (movementMagnitude > movementThreshold)
        {
            transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
        }
    }
}
