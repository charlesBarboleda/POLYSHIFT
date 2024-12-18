using UnityEngine;
using UnityEngine.Events;

public class BossEnemyNetworkHealth : EnemyNetworkHealth
{
    public string BossName;
    public override void HandleDeath(ulong networkObjectId)
    {
        base.HandleDeath(networkObjectId);

        if (IsServer)
        {
            GameManager.Instance.HealAllPlayersClientRpc();
            GameManager.Instance.GiveAllPlayersLevelClientRpc(4);
        }
    }

    public override void OnHitEffects(float prev, float current)
    {
        // Do nothing
    }
}
