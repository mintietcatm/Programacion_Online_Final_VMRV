using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float impactMarkLifeTime = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float ownerCollisionIgnoreDistance = 0.75f;

    private Vector3 spawnPosition;

    [Networked] private TickTimer LifeTimer { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            spawnPosition = transform.position;
            LifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        Vector3 nextPosition = transform.position +
                               transform.forward * speed * Runner.DeltaTime;

        if (CheckSweptCollision(nextPosition))
            return;

        transform.position = nextPosition;

        if (LifeTimer.Expired(Runner))
        {
            Debug.Log("Projectile desaparecio porque termino su tiempo de vida.");
            Runner.Despawn(Object);
        }
    }

    private bool CheckSweptCollision(Vector3 nextPosition)
    {
        Vector3 movement = nextPosition - transform.position;
        float distance = movement.magnitude;

        if (distance <= 0f)
            return false;

        SphereCollider projectileCollider = GetComponent<SphereCollider>();
        float radius = projectileCollider != null
            ? projectileCollider.radius * Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.y,
                transform.lossyScale.z)
            : 0.05f;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            radius,
            movement.normalized,
            distance,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        RaycastHit nearestHit = default;
        Health nearestHealth = null;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // Los proyectiles no deben bloquearse ni destruirse entre ellos.
            if (hit.collider.GetComponentInParent<Projectile>() != null)
                continue;

            // SimpleKCC puede crear su collider auxiliar fuera de la jerarquia
            // del Player. Ignoramos el espacio inmediato al canon para que el
            // proyectil pueda salir del collider de quien disparo.
            if (Vector3.Distance(spawnPosition, hit.point) <= ownerCollisionIgnoreDistance)
                continue;

            Health health = hit.collider.GetComponentInParent<Health>();

            // El proyectil puede iniciar dentro del KCC de quien disparo.
            if (health != null && health.Object.InputAuthority == Object.InputAuthority)
                continue;

            if (hit.distance < nearestDistance)
            {
                nearestHit = hit;
                nearestHealth = health;
                nearestDistance = hit.distance;
            }
        }

        if (nearestDistance == float.MaxValue)
            return false;

        if (nearestHealth != null)
            nearestHealth.TakeDamage(damage, Object.InputAuthority);

        Debug.Log($"Projectile impacto {nearestHit.collider.name} despues de recorrer " +
                  $"{Vector3.Distance(spawnPosition, nearestHit.point):0.00} metros.");

        Vector3 impactNormal = nearestHit.normal.sqrMagnitude > 0f
            ? nearestHit.normal
            : -movement.normalized;

        RPC_ShowImpact(nearestHit.point, impactNormal);
        Runner.Despawn(Object);
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        // Una bala no debe destruir otra bala.
        if (collision.collider.GetComponentInParent<Projectile>() != null)
            return;

        Health health = collision.collider.GetComponentInParent<Health>();

        ContactPoint contact = collision.GetContact(0);

        if (Vector3.Distance(spawnPosition, contact.point) <= ownerCollisionIgnoreDistance)
            return;

        // El proyectil nace cerca del KCC del jugador que disparo.
        // Si lo toca, debe continuar su recorrido en lugar de destruirse.
        if (health != null && health.Object.InputAuthority == Object.InputAuthority)
            return;

        if (health != null && health.Object.InputAuthority != Object.InputAuthority)
        {
            health.TakeDamage(damage, Object.InputAuthority);
        }

        Debug.Log($"Projectile colisiono con {collision.collider.name} despues de recorrer " +
                  $"{Vector3.Distance(spawnPosition, contact.point):0.00} metros.");
        RPC_ShowImpact(contact.point, contact.normal);
        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowImpact(Vector3 position, Vector3 normal)
    {
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mark.name = "Impact Mark";
        mark.transform.SetPositionAndRotation(position + normal * 0.002f, Quaternion.LookRotation(normal));
        mark.transform.localScale = Vector3.one * 0.15f;

        Collider markCollider = mark.GetComponent<Collider>();
        if (markCollider != null)
            Destroy(markCollider);

        Destroy(mark, impactMarkLifeTime);
    }
}
