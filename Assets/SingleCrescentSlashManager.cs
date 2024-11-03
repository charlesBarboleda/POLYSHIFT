using Unity.Netcode;
using UnityEngine;

public class SingleCrescentSlashManager : NetworkBehaviour
{
    public float attackRange = 3f;
    public float coneAngle = 90f;
    public float knockbackForce = 2f;
    public float damage = 10f;
    PlayerMelee playerMelee;

    void Start()
    {
        playerMelee = GetComponent<PlayerMelee>();
    }


    [ServerRpc]
    public void OnSingleCrescentSlashSpawnServerRpc()
    {
        OnSingleCrescentSlashSpawnClientRpc();
    }
    [ClientRpc]
    void OnSingleCrescentSlashSpawnClientRpc()
    {

        GameObject slash = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + transform.forward * 2f, transform.rotation);
        slash.transform.localScale = new Vector3(attackRange / 6, attackRange / 6, attackRange / 6);

    }




    public void DealConeDamage()
    {
        playerMelee.DealDamageInCone(playerMelee.transform.position, attackRange, coneAngle, damage, knockbackForce);

    }




}
