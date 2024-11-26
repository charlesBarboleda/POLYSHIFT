using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkLevel : NetworkBehaviour
{

    public NetworkVariable<int> Level = new NetworkVariable<int>(1);
    public NetworkVariable<float> Experience = new NetworkVariable<float>(0);

    public NetworkVariable<float> NeededExperience = new NetworkVariable<float>(0);
    LevelProgression LevelProgression = new LevelProgression();
    PlayerNetworkHealth playerNetworkHealth;
    PlayerNetworkMovement playerNetworkMovement;
    PlayerWeapon playerWeapon;
    ISkillManager[] meleeSkillsManager;
    PlayerAudioManager audioManager;
    PlayerSkills playerSkills;
    [SerializeField] SkillTreeManager skillTreeManager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        audioManager = GetComponent<PlayerAudioManager>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerWeapon = GetComponent<PlayerWeapon>();
        Level.OnValueChanged += OnLevelUp;
        meleeSkillsManager = GetComponentsInChildren<ISkillManager>();
        playerSkills = GetComponent<PlayerSkills>();
        if (IsServer)
        {

            Level.Value = 1;
            Experience.Value = LevelProgression.CurrentExperience;
            NeededExperience.Value = LevelProgression.NeededExperience;
        }

    }

    public void AddExperience(float experience)
    {
        if (!IsServer) return;
        bool hasLeveledUp = LevelProgression.AddExperience(experience);
        Level.Value = LevelProgression.CurrentLevel;
        Experience.Value = LevelProgression.CurrentExperience;
        NeededExperience.Value = LevelProgression.NeededExperience;
        if (hasLeveledUp)
        {
            LevelUpAnimClientRpc();
        }

    }

    void OnLevelUp(int prev, int current)
    {
        // Increase player stats & heal player back to full health
        playerNetworkHealth.maxHealth.Value += Level.Value;
        playerNetworkHealth.currentHealth.Value = playerNetworkHealth.maxHealth.Value;
        playerNetworkMovement.MoveSpeed += 0.05f;
        playerWeapon.Damage += 1;
        playerWeapon.DecreaseFireRateByServerRpc(0.01f);

        foreach (ISkillManager skill in meleeSkillsManager)
        {
            skill.Damage += Level.Value;
            skill.AttackSpeedMultiplier.Value += 0.01f;
        }

        skillTreeManager.skillPoints += 1;



    }





    [ClientRpc]
    void LevelUpAnimClientRpc()
    {
        audioManager.PlayLevelUpSound();
        for (int i = 1; i <= 4; i++)
        {
            GameObject anim = ObjectPooler.Instance.Spawn($"LevelUpAnim{i}", transform.position, Quaternion.Euler(-90, 0, 0));
            anim.GetComponent<NetworkObject>().Spawn();
            anim.transform.SetParent(transform);
        }

    }



}
