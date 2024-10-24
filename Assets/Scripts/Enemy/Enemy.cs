using UnityEngine;
using Unity.Netcode;

public enum EnemyType
{
    Melee,
    Ranged,
    Flying,
    Tank,
    Boss,
}

public abstract class Enemy : NetworkBehaviour
{
    public EnemyType enemyType;
    public EnemyHealth enemyHealth;
    public AIKinematics enemyMovement;

}
