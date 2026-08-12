using Fusion;
using System;
//using UnityEditor;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth;

    public static event Action<PlayerRef, PlayerRef> PlayerDied;
    public static event Action<int, int> DeathAnnounced;
    /// <summary>
    /// Para una variable networked es necesaria el get set
    /// La propiedad Networked hace que Photon sincronice este valor desde el server/Host/ State Authority hacia el resto de clientes
    /// Nos pide la obtencion para que los demas clientes puedan obtener el mismo valor, pero la modificacion sigue siendo privado
    /// Una regla no escrita para variables de red, es usar guion bajo al inicio, al final o entre cada palabra
    /// </summary>

    [Networked] public int _actualHealth { get; private set; }
    [Networked] public NetworkBool _isDead { get; private set; }

    public int MaxHealth => maxHealth;
    public int ActualHealth => _actualHealth;
    public bool IsDead => _isDead;

    public override void Spawned()
    {
        // Solo State Authority (Host/Server) puede inicializar variables de red.
        if (HasStateAuthority)
        {
            _actualHealth = maxHealth;
            _isDead = false;
        }

        // Cada computadora crea un solo HUD desde su jugador local.
        if (HasInputAuthority && GetComponent<HealthHUD>() == null)
            gameObject.AddComponent<HealthHUD>();

        if (HasInputAuthority && GetComponent<GameOverHUD>() == null)
            gameObject.AddComponent<GameOverHUD>();
    }

    public void TakeDamage(int damage)
    {
        // Se conserva este metodo para no romper llamadas existentes.
        TakeDamage(damage, PlayerRef.None);
    }

    public void TakeDamage(int damage, PlayerRef attacker)
    {
        // La vida solo se modifica en el Host/Server.
        if (!HasStateAuthority || _isDead || damage <= 0)
            return;

        _actualHealth = Mathf.Max(0, _actualHealth - damage);

        Debug.Log($"{GetPlayerName(Object.InputAuthority)} recibio {damage} de dano. Vida restante: {_actualHealth}");

        if (_actualHealth <= 0)
        {
            //Alan mato a Sam
            Die(attacker);
        }
    }

    private void Die(PlayerRef attacker)
    {
        if (_isDead)
            return;

        _isDead = true;

        PlayerRef victim = Object.InputAuthority;
        PlayerDied?.Invoke(attacker, victim);

        int attackerId = attacker == PlayerRef.None ? -1 : attacker.PlayerId;
        RPC_AnnounceDeath(attackerId, victim.PlayerId);

        Debug.Log($"{GetPlayerName(attacker)} mato a {GetPlayerName(victim)}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AnnounceDeath(int attackerId, int victimId)
    {
        DeathAnnounced?.Invoke(attackerId, victimId);
    }

    private static string GetPlayerName(PlayerRef player)
    {
        return player == PlayerRef.None
            ? "La tormenta"
            : $"Jugador {player.PlayerId}";
    }
}


