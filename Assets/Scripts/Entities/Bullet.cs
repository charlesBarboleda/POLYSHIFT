using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;

public abstract class Bullet : NetworkBehaviour
{
    public float Damage = 10f;
    public string bulletTag;
    public float Speed = 10f;
    public float Lifetime = 3f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroyBullet), Lifetime);
        }
    }

    public void Initialize(float damage, float speed, float lifetime, string tag)
    {
        Damage = damage;
        Speed = speed;
        Lifetime = lifetime;
        bulletTag = tag;
    }



    public void DestroyBullet()
    {
        NetworkObjectPool.Instance.ReturnNetworkObject(gameObject.GetComponent<NetworkObject>(), bulletTag);
    }

}
