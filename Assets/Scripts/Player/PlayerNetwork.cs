using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Player Inputs")]
    PlayerInput moveInput;
    InputAction moveAction;

    [Header("Player Movement")]

    NetworkVariable<float> moveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        moveSpeed.OnValueChanged += OnMoveSpeedChanged;
    }

    void Start()
    {
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * Time.deltaTime * moveSpeed.Value;
    }

    void OnMoveSpeedChanged(float previousValue, float newValue)
    {
        moveSpeed.Value = newValue;
    }
}
