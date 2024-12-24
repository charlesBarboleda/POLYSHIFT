using UnityEngine;
using Unity.Netcode;

public interface IStaggerable
{
    void StaggerRpc();
    void ApplyStaggerDamageServerRpc(float damage);

}
