using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerMelee : NetworkBehaviour
{
    private List<MeleeAttack> meleeAttacks;
    private MeleeAttack currentAttack;
    private bool isAttacking = false;
    private Animator animator;
    private PlayerNetworkMovement playerMovement;
    private PlayerNetworkRotation playerRotation;
    public int attackIndex;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerNetworkMovement>();
        playerRotation = GetComponent<PlayerNetworkRotation>();

        // Initialize available attacks
        meleeAttacks = new List<MeleeAttack>
        {
            gameObject.AddComponent<DoubleCrescentSlash>(),
            // Add more attacks here as needed
        };

        foreach (MeleeAttack attack in meleeAttacks)
        {
            attack.Initialize(animator, playerMovement, playerRotation);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isAttacking)
        {
            StartCoroutine(PerformAttack(attackIndex)); // Perform the first attack for now
        }
    }
    public void AddAttack(MeleeAttack attack)
    {
        meleeAttacks.Add(attack);
        attack.Initialize(animator, playerMovement, playerRotation);
    }

    private IEnumerator PerformAttack(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= meleeAttacks.Count) yield break;

        isAttacking = true;
        currentAttack = meleeAttacks[attackIndex];
        yield return StartCoroutine(currentAttack.ExecuteAttack());
        isAttacking = false;
    }
}
