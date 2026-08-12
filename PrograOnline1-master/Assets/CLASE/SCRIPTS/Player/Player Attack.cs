using UnityEngine;
using Fusion;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] private NetworkObject projectile;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform container;
    [SerializeField] private float delay = 0.25f;

    private float cooldown;
    private Health playerHealth;

    public override void Spawned()
    {
        base.Spawned();
        playerHealth = GetComponent<Health>();
    }

    public override void FixedUpdateNetwork()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (GetInput(out NetworkInfoData input))
        {
            if (input.shootPressed && Time.time >= cooldown)
            {
                cooldown = Time.time + delay;

                Vector3 spawnPos = shootPoint != null
                    ? shootPoint.position
                    : container.position;

                Quaternion spawnRot = shootPoint != null
                    ? shootPoint.rotation
                    : container.rotation;

                Runner.Spawn(
                    projectile,
                    spawnPos,
                    spawnRot,
                    Object.InputAuthority
                );
            }
        }
    }
}
