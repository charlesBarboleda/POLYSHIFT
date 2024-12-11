using UnityEngine;

public class MimicTurret : Turret
{

    public override void Update()
    {
        base.Update();

        // Update stats from the player weapon
        if (Owner != null)
        {
            AttackSpeed = playerWeapon.ShootRate;
            Damage = playerWeapon.Damage / 6;
        }

    }
}

