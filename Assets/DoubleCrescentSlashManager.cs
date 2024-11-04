using Unity.Netcode;
using UnityEngine;

public class DoubleCrescentSlashManager : NetworkBehaviour
{
    public float stepDistance = 0.5f;
    public float attackRange = 3f;
    public float coneAngle = 90f;
    public float knockbackForce = 2f;
    public float damage = 10f;
    PlayerMelee playerMelee;
    GameObject player;


    void Start()
    {
        playerMelee = GetComponent<PlayerMelee>();
        player = GetComponentInParent<PlayerNetworkHealth>().gameObject;
    }


    [ServerRpc]
    public void OnDoubleCrescentSlashSpawnServerRpc()
    {
        OnDoubleCrescentSlashSpawnClientRpc();
    }
    [ClientRpc]
    void OnDoubleCrescentSlashSpawnClientRpc()
    {

        GameObject slash = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + transform.forward * 3f, transform.rotation);
        slash.transform.localScale = new Vector3(attackRange / 2, attackRange / 2, attackRange / 2);
        ParticleSystem[] childParticleSystems = slash.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem mainParticleSystem = slash.GetComponent<ParticleSystem>();

        var mainModule = mainParticleSystem.main;
        mainModule.simulationSpeed = 0.5f; // Adjust this value to slow down
        foreach (ParticleSystem ps in childParticleSystems)
        {
            var mainModule2 = ps.main;
            mainModule2.simulationSpeed = 0.5f; // Adjust this value to slow down
        }
    }

    public void frontStep()
    {
        player.transform.position += transform.forward * stepDistance;
    }

    public void frontStepLarge()
    {
        player.transform.position += transform.forward * stepDistance * 4;
    }
    public void DealSmallConeDamage()
    {
        playerMelee.DealDamageInCone(playerMelee.transform.position, attackRange, coneAngle, damage, knockbackForce);

    }


    public void DealBigConeDamage()
    {
        playerMelee.DealDamageInCone(playerMelee.transform.position, attackRange + 3, 100f, damage * 2, knockbackForce + 1);

    }



}
