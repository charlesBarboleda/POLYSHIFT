using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using NUnit.Framework;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("Player Inputs")]
    private PlayerInput moveInput;
    private InputAction moveAction;
    private InputAction dashAction;
    public Vector2 inputDirection;

    [Header("Player Movement")]
    public float GhostMoveSpeed = 6f;
    public float MoveSpeed = 2f;
    public float DashSpeed = 10f;
    public float DashDuration = 0.2f;
    public float DashCooldown = 5f;
    bool canDash = true;
    bool isDashing = false;
    float dashTimeRemaining = 0f;
    float dashCooldownRemaining = 0f;
    public GameObject skillTreeCanvas;
    public VariableWithEvent<bool> IsIsometric = new VariableWithEvent<bool>(false);
    public bool canMove = true;
    private Animator animator;
    private PlayerAudioManager audioManager;
    private PlayerStateController state;
    [SerializeField] Image dashCooldownFill;
    [SerializeField] Volume localVolume;
    [SerializeField] GameObject dashEffectPrefab;
    Vignette vignette;




    private const float movementThreshold = 0.1f;

    public override void OnNetworkSpawn()
    {
        animator = GetComponentInChildren<Animator>();
        audioManager = GetComponent<PlayerAudioManager>();
        if (!IsOwner) return;


        localVolume.profile.TryGet(out vignette);
        canDash = true;
        isDashing = false;
        canMove = true;
        DashSpeed = 10f;
        DashDuration = 0.2f;
        state = GetComponent<PlayerStateController>();
        moveInput = GetComponent<PlayerInput>();
        moveAction = moveInput.actions["Move"];
        dashAction = moveInput.actions["Dash"];
    }

    void Update()
    {
        if (!IsOwner) return;

        UpdateDashCooldownFill();

        // Handle cooldown timer
        if (!canDash)
        {
            dashCooldownRemaining += Time.deltaTime;
            if (dashCooldownRemaining >= DashCooldown) canDash = true;
        }

        // Handle dashing logic
        if (isDashing)
        {
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0f)
            {
                EndDash();
            }
        }

        inputDirection = moveAction.ReadValue<Vector2>();

        if (state.playerState.Value == PlayerState.Dead)
        {
            HandleGhostMovement(inputDirection);
        }

        if (state.playerState.Value == PlayerState.Alive && canMove)
        {
            if (dashAction.triggered && canDash) StartCoroutine(StartDash()); // Trigger dash
            else HandleMovementAndAnimations(inputDirection);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DashEffectRpc()
    {
        Debug.Log("Spawning Dash Effect for " + OwnerClientId);
        var dashEffect = ObjectPooler.Instance.Spawn("DashEffect", transform.position, Quaternion.identity);
        // var dashEffect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        Debug.Log($"Dash Effect Is Active: {dashEffect.activeInHierarchy} & {dashEffect.activeSelf}");
    }



    IEnumerator StartDash()
    {
        audioManager.PlayDashSound();
        DashScreenEffects();
        DashEffectRpc();

        isDashing = true;
        dashTimeRemaining = DashDuration;
        canDash = false;
        dashCooldownRemaining = 0f;

        // Apply dash movement

        MoveSpeed += DashSpeed;

        yield return new WaitForSeconds(DashDuration / 2);

        DashEffectRpc();

    }

    void DashScreenEffects()
    {
        if (vignette != null)
        {
            vignette.color.value = Color.cyan;
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, DashDuration).OnComplete(() =>
            {
                DOTween.To(() => vignette.color.value, x => vignette.color.value = x, Color.black, 0.25f);
                DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.3f, 0.1f);


            });
        }
    }

    private void EndDash()
    {
        DashEffectRpc();
        isDashing = false;
        MoveSpeed -= DashSpeed;
    }

    void UpdateDashCooldownFill()
    {
        dashCooldownFill.fillAmount = dashCooldownRemaining / DashCooldown;
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
