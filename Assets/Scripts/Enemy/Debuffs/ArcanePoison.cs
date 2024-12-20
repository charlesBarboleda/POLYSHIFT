using System.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ArcanePoison", menuName = "Debuffs/ArcanePoison")]
public class ArcanePoison : Debuff
{
    public float damagePerTick = 1f;
    public float tickRate = 0.1f;
    private float tickTimer;
    [SerializeField] ArcanePoison arcanePoison;
    EnemyNetworkHealth enemyHealth;
    GameObject DebuffEffect;
    GameObject ExplosionEffect;


    public override void Initialize(GameObject target)
    {
        enemyHealth = target.GetComponent<EnemyNetworkHealth>();
        target.GetComponent<NetworkBehaviour>().StartCoroutine(DebuffEffectCoroutine(target));
    }

    public override void UpdateEffect(GameObject target)
    {
        duration -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (duration <= 0)
        {
            RemoveEffect(target);
        }
        else if (tickTimer <= 0)
        {
            ApplyEffect(target);
            tickTimer = tickRate;  // Reset the tick timer
        }
        if (enemyHealth.CurrentHealth.Value <= 0 && !enemyHealth.IsDead)
        {
            Implode(target);
            RemoveEffect(target);
        }
    }

    IEnumerator DebuffEffectCoroutine(GameObject target)
    {
        SpawnArcaneDOTRpc();
        DebuffEffect.transform.position = target.transform.position + new Vector3(0, 0.5f, 0);
        DebuffEffect.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        DebuffEffect.transform.SetParent(target.transform);
        yield return new WaitForSeconds(duration);
        DespawnArcaneDOTRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnArcaneDOTRpc()
    {
        DebuffEffect = ObjectPooler.Instance.Spawn("ArcaneDOT", Vector3.zero, Quaternion.identity);


    }
    [Rpc(SendTo.ClientsAndHost)]
    void DespawnArcaneDOTRpc()
    {
        ObjectPooler.Instance.Despawn("ArcaneDOT", DebuffEffect);

    }




    void Implode(GameObject target)
    {
        Debug.Log("Imploding");
        SpawnExplosionEffectRpc();
        ExplosionEffect.transform.position = target.transform.position;
        RaycastHit[] hitObjects = Physics.SphereCastAll(target.transform.position, 10, Vector3.up, 0);
        foreach (RaycastHit hitObject in hitObjects)
        {
            if (hitObject.collider.CompareTag("Enemy"))
            {
                hitObject.collider.GetComponent<IDamageable>().RequestTakeDamageServerRpc(target.GetComponent<EnemyNetworkHealth>().MaxHealth / 10, 0);
                ArcanePoison arcanePoisonInstance = Instantiate(arcanePoison);
                arcanePoisonInstance.duration = duration;
                arcanePoisonInstance.damagePerTick = damagePerTick * 2;

                hitObject.collider.GetComponent<DebuffManager>().AddDebuff(arcanePoisonInstance);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnExplosionEffectRpc()
    {
        ExplosionEffect = ObjectPooler.Instance.Spawn("ArcaneExplosion", Vector3.zero, Quaternion.identity);

    }

    public override void ApplyEffect(GameObject target)
    {
        target.GetComponent<IDamageable>().RequestTakeDamageServerRpc(damagePerTick, 0);
    }

    public override void RemoveEffect(GameObject target)
    {
        Debug.Log("Removing debuff");
        target.GetComponent<DebuffManager>().RemoveDebuff(this);
        ObjectPooler.Instance.Despawn("ArcaneDOT", DebuffEffect);
        Debug.Log("Debuff removed and effect despawned");

    }


}
