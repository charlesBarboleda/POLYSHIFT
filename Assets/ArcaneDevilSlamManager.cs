using System.Collections;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class ArcaneDevilSlamManager : NetworkBehaviour
{
    public float damage = 100f;
    public float knockbackForce = 10f;

    GameObject Devil;
    GameObject DevilPortal;
    GameObject DevilPortal2;

    [ServerRpc]
    public void OnArcaneDevilSlamSpawnServerRpc()
    {
        // Spawn portal and devil on the server
        SpawnPortalAndDevil();
    }

    void SpawnPortalAndDevil()
    {
        // Server-only spawning of the portals and devil
        DevilPortal = ObjectPooler.Instance.Spawn("DevilPortal", transform.position + -transform.forward * 12f, transform.rotation);
        DevilPortal2 = ObjectPooler.Instance.Spawn("DevilPortal2", transform.position + -transform.forward * 12f, transform.rotation);
        DevilPortal.GetComponent<NetworkObject>().Spawn(); // Make sure portal is only spawned on the server
        DevilPortal2.GetComponent<NetworkObject>().Spawn(); // Make sure portal is only spawned on the server

        Devil = ObjectPooler.Instance.Spawn("Devil", DevilPortal.transform.position + Vector3.down * 20f, transform.rotation);
        Devil.GetComponent<NetworkObject>().Spawn(); // Make sure Devil is only spawned on the server
        Devil.GetComponent<DevilManager>().SetPlayer(transform, damage);

        DevilPortal.transform.localScale = Vector3.zero;
        DevilPortal2.transform.localScale = Vector3.zero;

        DevilPortal.transform.DOScale(Vector3.one, 3f);
        DevilPortal2.transform.DOScale(Vector3.one, 3f);

        Devil.transform.DOMove(DevilPortal.transform.position + Vector3.down * 10f, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisablePortalsServerRpc()
    {
        // Notify clients to disable portals
        DisablePortalsClientRpc();
    }

    [ClientRpc]
    void DisablePortalsClientRpc()
    {
        // Client-side portal disable logic
        if (DevilPortal != null) DisablePortal();
        if (DevilPortal2 != null) StartCoroutine(DisablePortal2());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisableDevilServerRpc()
    {

        DisableDevilClientRpc();
    }

    [ClientRpc]
    void DisableDevilClientRpc()
    {
        ObjectPooler.Instance.Despawn("Devil", Devil);
        if (IsServer)
        {
            Devil.GetComponent<NetworkObject>().Despawn(false);
        }
    }

    IEnumerator DisablePortal2()
    {
        DevilPortal2.transform.DOScale(Vector3.zero, 2f);
        yield return new WaitForSeconds(2.5f);
        DevilPortal2.GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("DevilPortal2", DevilPortal2);
    }

    void DisablePortal()
    {
        DevilPortal.GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("DevilPortal", DevilPortal);
    }
}
