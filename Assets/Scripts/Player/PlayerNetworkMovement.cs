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


    PlayerNetworkRotation playerNetworkRotation;

    public override void OnNetworkSpawn()
    {
        MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
    }

    void Start()
    {
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (!playerNetworkRotation.IsIsometric.Value)
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
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * Time.deltaTime * MoveSpeed.Value;
    }

    public void MovePlayerFirstPerson()
    {
        // Get input from the Move action
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        // Player's current forward and right vectors based on their rotation
        Vector3 forward = transform.forward;   // Forward based on player rotation
        Vector3 right = transform.right;       // Right based on player rotation

        // Calculate movement direction based on input and player rotation
        Vector3 moveDirection = (forward * inputDirection.y) + (right * inputDirection.x);

        // Normalize to avoid faster diagonal movement
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Move the player based on the calculated direction and move speed
        transform.position += moveDirection * Time.deltaTime * MoveSpeed.Value;
    }


    void OnMoveSpeedChanged(float previousValue, float newValue)
    {
        MoveSpeed.Value = newValue;
    }
}
