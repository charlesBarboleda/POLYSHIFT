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

[RequireComponent(typeof(AIKinematics))]
public class Enemy : NetworkBehaviour
{
    [Header("Script References")]
    AIKinematics AIkinematics;

    void Start()
    {
        AIkinematics = GetComponent<AIKinematics>();
    }

}
