using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    PlayerInput moveInput;
    InputAction moveAction;
    float horiontalMouseInput;

    [Header("Player Movement")]
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsIsometric = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    Animator animator;
    PlayerNetworkRotation playerNetworkRotation;
    PlayerNetworkHealth playerNetworkHealth;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }
    // Update is called once per frame
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
        // Disable the cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Get the input from the player (WASD or arrow keys)
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        // Get the main camera's forward and right vectors
        // These vectors are relative to the world, but aligned with the camera's perspective
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Since we want movement on the horizontal plane only (ignore Y-axis),
        // we need to zero out the Y component of both the forward and right vectors
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalize the vectors to ensure consistent movement speed
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the desired movement direction based on the camera's orientation
        Vector3 moveDirection = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
        animator.SetFloat("IsMoving", moveDirection.magnitude);
        // Move the player
        transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
    }


    public void MovePlayerFirstPerson()
    {
        // Enable the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        // Get input from the Move action
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        // Player's current forward and right vectors based on their rotation
        Vector3 forward = transform.forward;   // Forward based on player rotation
        Vector3 right = transform.right;       // Right based on player rotation

        // Calculate movement direction based on input and player rotation
        Vector3 moveDirection = (forward * inputDirection.y) + (right * inputDirection.x);
        Debug.Log(moveDirection);
        animator.SetFloat("IsMoving", moveDirection.magnitude);
        // Normalize to avoid faster diagonal movement
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Move the player based on the calculated direction and move speed
        transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
    }



}
