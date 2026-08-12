using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;


public class Handgun : Weapon
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float raycastRange = 50f;
    [SerializeField] private float delay = 0.25f;

    private Vector3 currentAimDirection = Vector3.forward;
    private Health playerHealth;
    private string weaponName = "Pistola";

    [Networked] private TickTimer cooldown { get; set; }
    [Networked] public int CurrentAmmo { get; private set; }

    public int MagazineSize => magazineSize;

    public string GetWeaponName()
    {
        return weaponName;
    }

    public override void Spawned()
    {
        base.Spawned();
        playerHealth = GetComponent<Health>();

        if (Object.HasStateAuthority)
            CurrentAmmo = actualAmmo;
    }

    public override void FixedUpdateNetwork()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (!GetInput(out NetworkInfoData input))
            return;

        if (input.aimDirection.sqrMagnitude > 0.0001f)
            currentAimDirection = input.aimDirection.normalized;

        if (input.reloadPressed && Object.HasStateAuthority)
        {
            Reload();
            return;
        }

        if (!input.shootPressed || !cooldown.ExpiredOrNotRunning(Runner))
            return;

        cooldown = TickTimer.CreateFromSeconds(Runner, delay);

        // Solo el State Authority crea el proyectil durante el tick hacia adelante.
        if (!Runner.IsForward || !Object.HasStateAuthority)
            return;

        if (shootType == ShootType.Raycast)
            RaycastShoot();
        else
            PhysicShoot();
    }


public override void RaycastShoot()
    {
        //if (actualAmmo <= 0)
        if (CurrentAmmo <= 0)
        {
            Debug.Log("No tienes balas :c");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Handgun necesita un Fire Point.");
            return;
        }

        //actualAmmo--;
        CurrentAmmo--;
        actualAmmo = CurrentAmmo;
        Ray ray = new Ray(firePoint.position, firePoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange))
            Debug.Log(hit.collider.name);
    }


public override void PhysicShoot()
    {
        //if (actualAmmo <= 0)
        if (CurrentAmmo <= 0)
        {
            Debug.Log("No tienes balas :c");
            return;
        }

        if (firePoint == null || projectile == null)
        {
            Debug.LogError("Handgun necesita un Fire Point y el prefab Projectile.");
            return;
        }

        //actualAmmo--;
        CurrentAmmo--;
        actualAmmo = CurrentAmmo;

        SpawnNetworkProjectile();
    }


    public override void Reload()
    {
        //actualAmmo = magazineSize; //bien facil. nomas recargamos al numero maximo del cargador
        CurrentAmmo = magazineSize;
        actualAmmo = CurrentAmmo;
    }

    public bool TryCollectAmmo()
    {
        if (!Object.HasStateAuthority || CurrentAmmo >= magazineSize)
            return false;

        Reload();
        RPC_HideAmmoPickup();
        return true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideAmmoPickup()
    {
        GameObject ammoPickup = GameObject.Find("Ammo Pickup");

        if (ammoPickup != null)
            ammoPickup.SetActive(false);
    }

    public bool TryCollectRifle()
    {
        if (!Object.HasStateAuthority || weaponName == "Rifle")
            return false;

        RPC_EquipRifle();
        return true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EquipRifle()
    {
        weaponName = "Rifle";
        damage = 5;
        magazineSize = 30;
        delay = 0.1f;
        actualAmmo = 30;

        if (Object.HasStateAuthority)
            CurrentAmmo = 30;

        GameObject riflePickup = GameObject.Find("Rifle Pickup");

        if (riflePickup != null)
            riflePickup.SetActive(false);
    }


private void SpawnNetworkProjectile()
    {
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        if (projectileNetworkObject == null)
        {
            Debug.LogError("El prefab de Projectile debe tener un NetworkObject.");
            return;
        }

        //Runner.Spawn(projectileNetworkObject, firePoint.position, firePoint.rotation, Object.InputAuthority);

        Quaternion projectileRotation = GetProjectileAimRotation();
        //Runner.Spawn(projectileNetworkObject, firePoint.position, projectileRotation, Object.InputAuthority);
        NetworkObject spawnedProjectile = Runner.Spawn(
            projectileNetworkObject,
            firePoint.position,
            projectileRotation,
            Object.InputAuthority);

        Projectile projectileScript = spawnedProjectile.GetComponent<Projectile>();

        if (projectileScript != null)
            projectileScript.SetDamage(damage);
    }

private Quaternion GetProjectileAimRotation()
    {
        //SimpleKCC kcc = GetComponent<SimpleKCC>();
        Camera playerCamera = GetComponentInChildren<Camera>(true);

        //if (kcc == null || playerCamera == null)
        //{
        //    Debug.LogWarning("No se encontro SimpleKCC o la camara. Se usara la rotacion del Fire Point.");
        //    return firePoint.rotation;
        //}

        if (playerCamera == null)
        {
            Debug.LogWarning("No se encontro la camara. Se usara la rotacion del Fire Point.");
            return firePoint.rotation;
        }

        // La rotacion del KCC esta sincronizada y se puede usar tambien cuando
        // la camara del jugador remoto esta desactivada en el Host.
        //Vector2 lookRotation = kcc.GetLookRotation();
        //Quaternion aimRotation = kcc.TransformRotation *
        //                         Quaternion.Euler(lookRotation.x, 0f, 0f);
        //Vector3 aimDirection = aimRotation * Vector3.forward;

        // Esta direccion viene directamente de la camara del jugador que
        // dispara y viaja dentro del input de Fusion hasta el Host.
        Vector3 aimDirection = currentAimDirection.normalized;
        Vector3 aimOrigin = playerCamera.transform.position;
        Vector3 targetPoint = aimOrigin + aimDirection * raycastRange;

        RaycastHit[] hits = Physics.RaycastAll(
            aimOrigin,
            aimDirection,
            raycastRange,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Health health = hit.collider.GetComponentInParent<Health>();

            // La mira no debe converger hacia el collider del jugador que dispara.
            if (health != null && health.Object.InputAuthority == Object.InputAuthority)
                continue;

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                targetPoint = hit.point;
            }
        }

        Vector3 directionFromMuzzle = targetPoint - firePoint.position;

        if (directionFromMuzzle.sqrMagnitude <= 0.0001f)
            return firePoint.rotation;

        return Quaternion.LookRotation(directionFromMuzzle.normalized, Vector3.up);
    }
}
